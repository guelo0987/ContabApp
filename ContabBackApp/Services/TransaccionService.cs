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
}

public class TransaccionService : ITransaccionService
{
    private readonly MyDbContext _context;

    // --- CONSTANTES DE CUENTAS (Basadas en 'Parametrizacion.csv') ---
    // En un sistema real, esto podría venir de una tabla de configuración global.
    private const int CUENTA_INGRESOS_VENTA = 13; // ID 13: Ingresos x Ventas
    private const int CUENTA_CAJA_GENERAL = 3;    // ID 3: Caja General
    private const int CUENTA_ITBIS_POR_PAGAR = 4; // ID 4: ITBIS por Pagar

    public TransaccionService(MyDbContext context)
    {
        _context = context;
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

        // 1.2 Validar Tipo de Documento (Traemos la configuración contable)
        var tipoDoc = await _context.TiposDocumentos.FindAsync(dto.IdTipoDocumento);
        if (tipoDoc == null)
            throw new KeyNotFoundException($"El tipo de documento {dto.IdTipoDocumento} no existe.");

        // 1.3 Validar Límite de Crédito (Solo para Venta/Deuda "DB")
        if (dto.TipoMovimiento == "DB")
        {
            decimal saldoActual = await CalcularSaldoCliente(dto.IdCliente);
            decimal nuevoSaldo = saldoActual + dto.Monto;
            if (nuevoSaldo > cliente.LimiteCredito)
            {
                throw new InvalidOperationException(
                    $"Crédito excedido. Límite: {cliente.LimiteCredito:C}, Saldo Actual: {saldoActual:C}, Intento: {dto.Monto:C}");
            }
        }

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

            // LÓGICA DE CONTABILIZACIÓN SEGÚN MOVIMIENTO
            if (dto.TipoMovimiento == "DB") // FACTURA DE VENTA
            {
                // Definir Tasa de Impuesto (Podría venir de config o DTO, asumimos 18%)
                decimal tasaItbis = 0.18m;
                
                // Matemática financiera: Desglosar el monto bruto
                // Si Monto es 118, Base es 100 e Impuesto es 18.
                decimal montoBase = Math.Round(dto.Monto / (1 + tasaItbis), 2);
                decimal montoItbis = dto.Monto - montoBase;

                // 1. DÉBITO: CxC Clientes (El cliente me debe TODO: 118)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = tipoDoc.IdCuentaContable ?? throw new InvalidOperationException("El tipo de documento no tiene cuenta contable configurada."),
                    TipoMovimiento = "DB",
                    Monto = dto.Monto
                });

                // 2. CRÉDITO: Ingreso por Venta (Lo que realmente gané: 100)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = CUENTA_INGRESOS_VENTA,
                    TipoMovimiento = "CR",
                    Monto = montoBase
                });

                // 3. CRÉDITO: ITBIS por Pagar (Lo que debo al gobierno: 18)
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = CUENTA_ITBIS_POR_PAGAR,
                    TipoMovimiento = "CR",
                    Monto = montoItbis
                });
            }
            else // "CR" -> COBRO / RECIBO DE INGRESO
            {
                // Aquí NO hay desglose de ITBIS, solo movimiento de dinero.
                // Entra dinero al Banco (DB) y baja la deuda del Cliente (CR).
                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = CUENTA_CAJA_GENERAL,
                    TipoMovimiento = "DB",
                    Monto = dto.Monto
                });

                detalles.Add(new AsientosDetalle
                {
                    IdAsiento = nuevoAsiento.IdAsiento,
                    IdCuentaContable = tipoDoc.IdCuentaContable ?? throw new InvalidOperationException("El tipo de documento no tiene cuenta contable configurada."),
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
}
