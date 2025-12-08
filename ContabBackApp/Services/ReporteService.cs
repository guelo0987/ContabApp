using ContabBackApp.Context;
using ContabBackApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

public interface IReporteService
{
    Task<EstadoCuentaDto> ObtenerEstadoCuentaAsync(int idCliente);
    Task<List<AsientoReporteDto>> ObtenerDiarioGeneralAsync(DateOnly? desde, DateOnly? hasta);
    Task<DashboardDto> ObtenerDashboardAsync();
}

public class ReporteService : IReporteService
{
    private readonly MyDbContext _context;

    public ReporteService(MyDbContext context)
    {
        _context = context;
    }

    // REPORTE DE CLIENTES (CxC)
    public async Task<EstadoCuentaDto> ObtenerEstadoCuentaAsync(int idCliente)
    {
        var cliente = await _context.Clientes.FindAsync(idCliente);
        if (cliente == null) throw new KeyNotFoundException("Cliente no encontrado");

        var transacciones = await _context.TransaccionesCxcs
            .Include(t => t.IdTipoDocumentoNavigation)
            .Where(t => t.IdCliente == idCliente)
            .OrderBy(t => t.FechaTransaccion)
            .ToListAsync();

        var reporte = new EstadoCuentaDto
        {
            IdCliente = cliente.IdCliente,
            NombreCliente = cliente.Nombre,
            Movimientos = transacciones.Select(t => new MovimientoClienteDto
            {
                Fecha = t.FechaTransaccion,
                TipoDoc = t.IdTipoDocumentoNavigation?.Descripcion ?? "Sin tipo",
                Numero = t.NumeroDocumento,
                // Si es DB (Factura) va en Débito, si es CR (Recibo) va en Crédito
                Debito = t.TipoMovimiento == "DB" ? t.Monto : 0,
                Credito = t.TipoMovimiento == "CR" ? t.Monto : 0,
                IdAsientoRef = t.IdAsientoGenerado
            }).ToList()
        };

        // Calcular Saldo Final
        reporte.SaldoTotal = reporte.Movimientos.Sum(m => m.Debito - m.Credito);

        return reporte;
    }

    // REPORTE CONTABLE (ASIENTOS)
    public async Task<List<AsientoReporteDto>> ObtenerDiarioGeneralAsync(DateOnly? desde, DateOnly? hasta)
    {
        var query = _context.AsientosCabeceras
            .Include(a => a.IdAuxiliarNavigation)
            .Include(a => a.AsientosDetalles)
                .ThenInclude(d => d.IdCuentaContableNavigation) // Join doble para traer nombre cuenta
            .AsQueryable();

        if (desde.HasValue) query = query.Where(a => a.FechaAsiento >= desde.Value);
        if (hasta.HasValue) query = query.Where(a => a.FechaAsiento <= hasta.Value);

        var asientos = await query.OrderByDescending(a => a.IdAsiento).ToListAsync();

        return asientos.Select(a => new AsientoReporteDto
        {
            IdAsiento = a.IdAsiento,
            Fecha = a.FechaAsiento,
            Descripcion = a.Descripcion,
            Origen = a.IdAuxiliarNavigation?.Descripcion ?? "Sin auxiliar", // Ej: "Cuentas x Cobrar"
            Detalles = a.AsientosDetalles.Select(d => new DetalleAsientoDto
            {
                Cuenta = d.IdCuentaContableNavigation != null 
                    ? $"{d.IdCuentaContable} - {d.IdCuentaContableNavigation.Descripcion}"
                    : $"Cuenta {d.IdCuentaContable}",
                Debito = d.TipoMovimiento == "DB" ? d.Monto : 0,
                Credito = d.TipoMovimiento == "CR" ? d.Monto : 0
            }).ToList(),
            // Verificación rápida: ¿Suma Débitos == Suma Créditos?
            EstaCuadrado = a.AsientosDetalles.Where(d => d.TipoMovimiento == "DB").Sum(d => d.Monto) ==
                           a.AsientosDetalles.Where(d => d.TipoMovimiento == "CR").Sum(d => d.Monto)
        }).ToList();
    }

    // DASHBOARD: Métricas generales del sistema
    public async Task<DashboardDto> ObtenerDashboardAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        // Clientes
        var clientes = await _context.Clientes.ToListAsync();
        var clientesActivos = clientes.Count(c => c.Estado == "Activo");

        // Transacciones del día
        var transaccionesHoy = await _context.TransaccionesCxcs
            .Where(t => t.FechaTransaccion == hoy)
            .ToListAsync();

        var debitosHoy = transaccionesHoy
            .Where(t => t.TipoMovimiento == "DB")
            .Sum(t => t.Monto);

        var creditosHoy = transaccionesHoy
            .Where(t => t.TipoMovimiento == "CR")
            .Sum(t => t.Monto);

        // Totales generales
        var todasTransacciones = await _context.TransaccionesCxcs.ToListAsync();
        var totalDebitos = todasTransacciones.Where(t => t.TipoMovimiento == "DB").Sum(t => t.Monto);
        var totalCreditos = todasTransacciones.Where(t => t.TipoMovimiento == "CR").Sum(t => t.Monto);
        var saldoCxCTotal = totalDebitos - totalCreditos;

        // Asientos del día
        var asientosHoy = await _context.AsientosCabeceras
            .CountAsync(a => a.FechaAsiento == hoy);

        // Top clientes deudores
        var saldosPorCliente = todasTransacciones
            .GroupBy(t => t.IdCliente)
            .Select(g => new
            {
                IdCliente = g.Key,
                Debitos = g.Where(t => t.TipoMovimiento == "DB").Sum(t => t.Monto),
                Creditos = g.Where(t => t.TipoMovimiento == "CR").Sum(t => t.Monto),
                CantFacturas = g.Count(t => t.TipoMovimiento == "DB")
            })
            .Select(x => new { x.IdCliente, Saldo = x.Debitos - x.Creditos, x.CantFacturas })
            .Where(x => x.Saldo > 0)
            .OrderByDescending(x => x.Saldo)
            .Take(5)
            .ToList();

        var topDeudores = new List<ResumenClienteDto>();
        foreach (var item in saldosPorCliente)
        {
            if (item.IdCliente == null) continue;
            var cliente = clientes.FirstOrDefault(c => c.IdCliente == item.IdCliente);
            if (cliente != null)
            {
                topDeudores.Add(new ResumenClienteDto
                {
                    IdCliente = item.IdCliente.Value,
                    Nombre = cliente.Nombre,
                    SaldoPendiente = item.Saldo,
                    CantidadFacturas = item.CantFacturas
                });
            }
        }

        return new DashboardDto
        {
            TotalClientes = clientes.Count,
            ClientesActivos = clientesActivos,
            DebitosDelDia = debitosHoy,
            CreditosDelDia = creditosHoy,
            SaldoCxCTotal = saldoCxCTotal,
            AsientosDelDia = asientosHoy,
            TotalFacturas = todasTransacciones.Count(t => t.TipoMovimiento == "DB"),
            TotalPagos = todasTransacciones.Count(t => t.TipoMovimiento == "CR"),
            TopClientesDeudores = topDeudores
        };
    }
}
