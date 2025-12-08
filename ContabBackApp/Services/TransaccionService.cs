using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

public interface ITransaccionService
{
    /// <summary>
    /// Procesa una transacción de CxC y genera su asiento contable automáticamente.
    /// </summary>
    /// <param name="dto">Datos de la venta o cobro</param>
    /// <param name="idUsuarioAuxiliar">ID del sistema que llama (ej: 5 para CxC)</param>
    Task<TransaccionResponseDto> RegistrarTransaccionAsync(RegistrarTransaccionDto dto, int idUsuarioAuxiliar);

    /// <summary>
    /// Obtiene el saldo actual del cliente (cuánto debe)
    /// </summary>
    Task<SaldoClienteDto> ObtenerSaldoClienteAsync(int idCliente);

    /// <summary>
    /// Obtiene el historial de transacciones de un cliente
    /// </summary>
    Task<List<TransaccionHistorialDto>> ObtenerHistorialClienteAsync(int idCliente);
}

public class TransaccionService : ITransaccionService
{
    private readonly MyDbContext _context;

    public TransaccionService(MyDbContext context)
    {
        _context = context;
    }

    // Obtener configuración de BD (con caché en memoria para no consultar cada vez)
    private async Task<int> ObtenerConfiguracionInt(string clave)
    {
        var config = await _context.ConfiguracionSistema
            .FirstOrDefaultAsync(c => c.Clave == clave);
        
        if (config == null)
            throw new InvalidOperationException($"Configuración '{clave}' no encontrada. Ejecute el script de inicialización.");
        
        return int.Parse(config.Valor);
    }

    private async Task<decimal> ObtenerConfiguracionDecimal(string clave)
    {
        var config = await _context.ConfiguracionSistema
            .FirstOrDefaultAsync(c => c.Clave == clave);
        
        if (config == null)
            throw new InvalidOperationException($"Configuración '{clave}' no encontrada.");
        
        return decimal.Parse(config.Valor);
    }

    public async Task<TransaccionResponseDto> RegistrarTransaccionAsync(RegistrarTransaccionDto dto, int idUsuarioAuxiliar)
    {
        // =================================================================
        // PASO 1: VALIDACIONES PREVIAS (Fail Fast)
        // =================================================================

        // 1.1 Validar Cliente
        var cliente = await _context.Clientes.FindAsync(dto.IdCliente);
        if (cliente == null)
            throw new KeyNotFoundException($"El cliente con ID {dto.IdCliente} no existe.");
        if (cliente.Estado != "Activo")
            throw new InvalidOperationException("El cliente está inactivo y no puede operar.");

        // 1.2 Validar Tipo de Documento y su Configuración Contable
        var tipoDoc = await _context.TiposDocumentos
            .Include(td => td.IdCuentaContableNavigation)
            .ThenInclude(cc => cc.IdTipoCuentaNavigation)
            .FirstOrDefaultAsync(td => td.IdTipoDocumento == dto.IdTipoDocumento);

        if (tipoDoc == null)
            throw new KeyNotFoundException($"El tipo de documento {dto.IdTipoDocumento} no existe.");

        if (tipoDoc.IdCuentaContableNavigation == null)
            throw new InvalidOperationException($"El tipo de documento '{tipoDoc.Descripcion}' no tiene una cuenta contable configurada. Contacte al contador.");

        // =================================================================
        // AUDITORIA CONTABLE INTELIGENTE (MODO PROFESOR 🎓)
        // =================================================================
        await ValidarReglasContables(dto, tipoDoc, cliente);


        // =================================================================
        // PASO 2: EJECUCIÓN TRANSACCIONAL (Atomicidad)
        // =================================================================
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // -----------------------------------------------------------
            // A. CREAR CABECERA DEL ASIENTO (Accounting Header)
            // -----------------------------------------------------------
            var nuevoAsiento = new AsientosCabecera
            {
                Descripcion = dto.Concepto ?? $"{tipoDoc.Descripcion} No. {dto.NumeroDocumento}",
                IdAuxiliar = idUsuarioAuxiliar, // Viene del Token (Seguridad)
                FechaAsiento = DateOnly.FromDateTime(DateTime.Now),
                IdMoneda = 1, // Por defecto Peso Dominicano
                TasaCambio = 1,
                Estado = "Registrado", // Estado final 'R'
                IdCliente = dto.IdCliente // Requerimiento PPT
            };

            _context.AsientosCabeceras.Add(nuevoAsiento);
            await _context.SaveChangesAsync(); // Guardamos para obtener el ID generado

            // -----------------------------------------------------------
            // B. GENERAR DETALLES (Partida Doble Automática con ITBIS)
            // -----------------------------------------------------------
            var detalles = new List<AsientosDetalle>();

            // Obtener configuración de cuentas desde BD
            int cuentaIngresosVenta = await ObtenerConfiguracionInt("CUENTA_INGRESOS_VENTA");
            int cuentaCajaGeneral = await ObtenerConfiguracionInt("CUENTA_CAJA_GENERAL");
            int cuentaItbisPorPagar = await ObtenerConfiguracionInt("CUENTA_ITBIS_POR_PAGAR");

            // LÓGICA DE CONTABILIZACIÓN SEGÚN MOVIMIENTO
            if (dto.TipoMovimiento == "DB") // FACTURA DE VENTA
            {
                // Usar la tasa de ITBIS del tipo de documento
                decimal tasaItbis = tipoDoc.AplicaItbis ? (tipoDoc.TasaItbis / 100m) : 0m;
                
                // Matemática financiera: Desglosar el monto bruto
                decimal montoBase = tasaItbis > 0 ? Math.Round(dto.Monto / (1 + tasaItbis), 2) : dto.Monto;
                decimal montoItbis = dto.Monto - montoBase;

                // 1. DÉBITO: CxC Clientes (El cliente me debe TODO)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = tipoDoc.IdCuentaContable.Value,
                    TipoMovimiento = "DB",
                    Monto = dto.Monto
                });

                // 2. CRÉDITO: Ingreso por Venta (Lo que realmente gané)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = cuentaIngresosVenta,
                    TipoMovimiento = "CR",
                    Monto = montoBase
                });

                // 3. CRÉDITO: ITBIS por Pagar (Solo si aplica)
                if (montoItbis > 0)
                {
                    detalles.Add(new AsientosDetalle
                    {
                        IdAsiento = nuevoAsiento.IdAsiento,
                        IdCuentaContable = cuentaItbisPorPagar,
                        TipoMovimiento = "CR",
                        Monto = montoItbis
                    });
                }
            }
            else // "CR" -> COBRO / RECIBO DE INGRESO
            {
                // 1. DÉBITO: Caja/Bancos (Entra dinero)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = cuentaCajaGeneral,
                    TipoMovimiento = "DB",
                    Monto = dto.Monto
                });

                // 2. CRÉDITO: CxC Clientes (Disminuye la deuda)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = tipoDoc.IdCuentaContable.Value,
                    TipoMovimiento = "CR",
                    Monto = dto.Monto
                });
            }

            // Verificación de seguridad (DB debe ser igual a CR)
            decimal totalDebito = detalles.Where(d => d.TipoMovimiento == "DB").Sum(d => d.Monto);
            decimal totalCredito = detalles.Where(d => d.TipoMovimiento == "CR").Sum(d => d.Monto);
            
            // Usamos un margen de error pequeño por redondeo de decimales
            if (Math.Abs(totalDebito - totalCredito) > 0.01m)
                throw new Exception($"Error de cuadre contable: Débito {totalDebito} vs Crédito {totalCredito}");

            _context.AsientosDetalles.AddRange(detalles);

            // -----------------------------------------------------------
            // C. REGISTRAR TRANSACCIÓN CxC (Historial Administrativo)
            // -----------------------------------------------------------
            var nuevaTransaccion = new TransaccionesCxc
            {
                TipoMovimiento = dto.TipoMovimiento,
                IdTipoDocumento = dto.IdTipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                FechaTransaccion = DateOnly.FromDateTime(DateTime.Now),
                IdCliente = dto.IdCliente,
                Monto = dto.Monto,
                IdAsientoGenerado = nuevoAsiento.IdAsiento // <--- EL VÍNCULO FINAL
            };

            _context.TransaccionesCxcs.Add(nuevaTransaccion);
            await _context.SaveChangesAsync();

            // D. Confirmar todo en la BD
            await dbTransaction.CommitAsync();

            return new TransaccionResponseDto
            {
                IdTransaccion = nuevaTransaccion.IdTransaccion,
                IdAsientoGenerado = nuevoAsiento.IdAsiento,
                Mensaje = "Transacción guardada y contabilizada correctamente."
            };
        }
        catch (Exception)
        {
            await dbTransaction.RollbackAsync(); // Si falla algo, revertimos todo
            throw; // Relanzamos el error al Controller
        }
    }

    // =================================================================
    // MÉTODOS DE AUDITORÍA Y VALIDACIÓN (EL PROFESOR)
    // =================================================================
    
    private async Task ValidarReglasContables(RegistrarTransaccionDto dto, TiposDocumento tipoDoc, Cliente cliente)
    {
        // 1. REGLA DE NATURALEZA DE CUENTA
        // Verificamos que la cuenta configurada en el documento tenga sentido para la operación.
        // Para transacciones de Clientes, la cuenta debe ser de origen DEUDOR (DB) -> Activo.
        var cuentaConfigurada = tipoDoc.IdCuentaContableNavigation;
        var origenCuenta = cuentaConfigurada.IdTipoCuentaNavigation?.Origen; // "DB" o "CR"

        if (origenCuenta != "DB")
        {
            throw new InvalidOperationException(
                $"Error Contable: El documento '{tipoDoc.Descripcion}' está vinculado a la cuenta '{cuentaConfigurada.Descripcion}' que es de origen ACREEDOR (CR). " +
                $"Las cuentas para clientes/ventas deben ser de origen DEUDOR (DB) (Activos).");
        }

        // 2. REGLA DE COHERENCIA DOCUMENTAL
        // El tipo de movimiento debe coincidir con lo configurado en el tipo de documento
        var movimientoEsperado = tipoDoc.TipoMovimientoEsperado ?? "DB";
        
        if (dto.TipoMovimiento != movimientoEsperado)
        {
            string tipoOperacion = movimientoEsperado == "DB" ? "Venta (Débito)" : "Cobro (Crédito)";
            string tipoEnviado = dto.TipoMovimiento == "DB" ? "Venta (Débito)" : "Cobro (Crédito)";
            
            throw new InvalidOperationException(
                $"Incoherencia: El documento '{tipoDoc.Descripcion}' está configurado para {tipoOperacion}, " +
                $"pero estás intentando usarlo como {tipoEnviado}.");
        }

        // 3. REGLA DE INTEGRIDAD DE SALDOS
        decimal saldoActual = await CalcularSaldoCliente(dto.IdCliente);
        decimal nuevoSaldo;

        if (dto.TipoMovimiento == "DB")
        {
            // Aumenta la deuda
            nuevoSaldo = saldoActual + dto.Monto;
            
            // Validar Límite de Crédito
            if (nuevoSaldo > cliente.LimiteCredito)
            {
                throw new InvalidOperationException(
                    $"Riesgo Financiero: La operación excede el límite de crédito del cliente. " +
                    $"Límite: {cliente.LimiteCredito:C}, Saldo Actual: {saldoActual:C}, Saldo proyectado: {nuevoSaldo:C}");
            }
        }
        else // "CR"
        {
            // Disminuye la deuda
            nuevoSaldo = saldoActual - dto.Monto;

            // Validar Saldo Negativo (No puedes cobrar más de lo que te deben)
            // Nota: En algunos negocios se permiten anticipos, pero por defecto lo bloqueamos para enseñar orden.
            if (nuevoSaldo < 0)
            {
                throw new InvalidOperationException(
                    $"Error de Lógica: El cobro de {dto.Monto:C} excede la deuda actual del cliente ({saldoActual:C}). " +
                    $"No se permiten saldos negativos en Cuentas por Cobrar sin autorización de anticipo.");
            }
        }
    }

    // Método auxiliar para saber cuánto debe el cliente hoy
    private async Task<decimal> CalcularSaldoCliente(int idCliente)
    {
        var movimientos = await _context.TransaccionesCxcs
            .Where(t => t.IdCliente == idCliente)
            .ToListAsync();

        decimal debitos = movimientos.Where(t => t.TipoMovimiento == "DB").Sum(t => t.Monto);
        decimal creditos = movimientos.Where(t => t.TipoMovimiento == "CR").Sum(t => t.Monto);

        return debitos - creditos;
    }

    // =================================================================
    // CONSULTAS PÚBLICAS
    // =================================================================

    public async Task<SaldoClienteDto> ObtenerSaldoClienteAsync(int idCliente)
    {
        var cliente = await _context.Clientes.FindAsync(idCliente);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {idCliente} no encontrado.");

        var movimientos = await _context.TransaccionesCxcs
            .Where(t => t.IdCliente == idCliente)
            .ToListAsync();

        decimal debitos = movimientos.Where(t => t.TipoMovimiento == "DB").Sum(t => t.Monto);
        decimal creditos = movimientos.Where(t => t.TipoMovimiento == "CR").Sum(t => t.Monto);
        decimal saldoActual = debitos - creditos;

        return new SaldoClienteDto
        {
            IdCliente = idCliente,
            NombreCliente = cliente.Nombre,
            SaldoActual = saldoActual,
            LimiteCredito = cliente.LimiteCredito,
            CreditoDisponible = cliente.LimiteCredito - saldoActual,
            CantidadFacturas = movimientos.Count(t => t.TipoMovimiento == "DB"),
            CantidadPagos = movimientos.Count(t => t.TipoMovimiento == "CR")
        };
    }

    public async Task<List<TransaccionHistorialDto>> ObtenerHistorialClienteAsync(int idCliente)
    {
        var cliente = await _context.Clientes.FindAsync(idCliente);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {idCliente} no encontrado.");

        var movimientos = await _context.TransaccionesCxcs
            .Include(t => t.IdTipoDocumentoNavigation)
            .Where(t => t.IdCliente == idCliente)
            .OrderBy(t => t.FechaTransaccion)
            .ThenBy(t => t.IdTransaccion)
            .ToListAsync();

        var historial = new List<TransaccionHistorialDto>();
        decimal saldoAcumulado = 0;

        foreach (var mov in movimientos)
        {
            if (mov.TipoMovimiento == "DB")
                saldoAcumulado += mov.Monto;
            else
                saldoAcumulado -= mov.Monto;

            historial.Add(new TransaccionHistorialDto
            {
                IdTransaccion = mov.IdTransaccion,
                TipoMovimiento = mov.TipoMovimiento ?? "N/A",
                TipoDocumento = mov.IdTipoDocumentoNavigation?.Descripcion ?? "N/A",
                NumeroDocumento = mov.NumeroDocumento,
                Fecha = mov.FechaTransaccion,
                Monto = mov.Monto,
                SaldoAcumulado = saldoAcumulado
            });
        }

        return historial;
    }
}