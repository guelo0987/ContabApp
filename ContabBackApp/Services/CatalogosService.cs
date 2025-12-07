using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

public interface ICatalogosService
{
    // Auxiliares
    Task<List<AuxiliarDto>> GetAuxiliaresAsync();
    Task<AuxiliarDto> CreateAuxiliarAsync(CreateAuxiliarDto dto);

    // Monedas
    Task<List<MonedaDto>> GetMonedasAsync();
    Task<MonedaDto> CreateMonedaAsync(CreateMonedaDto dto);

    // Tipos Cuenta
    Task<List<TipoCuentaDto>> GetTiposCuentaAsync();
    Task<TipoCuentaDto> CreateTipoCuentaAsync(CreateTipoCuentaDto dto);

    // Cuentas Contables
    Task<List<CuentaContableDto>> GetCuentasContablesAsync();
    Task<CuentaContableDto> CreateCuentaContableAsync(CreateCuentaContableDto dto);
}

public class CatalogosService : ICatalogosService
{
    private readonly MyDbContext _context;

    public CatalogosService(MyDbContext context)
    {
        _context = context;
    }

    // --- AUXILIARES ---
    public async Task<List<AuxiliarDto>> GetAuxiliaresAsync()
    {
        return await _context.Auxiliares
            .Select(x => new AuxiliarDto { IdAuxiliar = x.IdAuxiliar, Descripcion = x.Descripcion, Activo = x.Activo })
            .ToListAsync();
    }

    public async Task<AuxiliarDto> CreateAuxiliarAsync(CreateAuxiliarDto dto)
    {
        var entity = new Auxiliare { Descripcion = dto.Descripcion, Activo = true };
        _context.Auxiliares.Add(entity);
        await _context.SaveChangesAsync();
        return new AuxiliarDto { IdAuxiliar = entity.IdAuxiliar, Descripcion = entity.Descripcion, Activo = entity.Activo };
    }

    // --- MONEDAS ---
    public async Task<List<MonedaDto>> GetMonedasAsync()
    {
        return await _context.Monedas
            .Select(x => new MonedaDto { IdMoneda = x.IdMoneda, CodigoIso = x.CodigoIso, Descripcion = x.Descripcion, TasaCambio = x.TasaCambio })
            .ToListAsync();
    }

    public async Task<MonedaDto> CreateMonedaAsync(CreateMonedaDto dto)
    {
        var entity = new Moneda { CodigoIso = dto.CodigoIso, Descripcion = dto.Descripcion, TasaCambio = dto.TasaCambio };
        _context.Monedas.Add(entity);
        await _context.SaveChangesAsync();
        return new MonedaDto { IdMoneda = entity.IdMoneda, CodigoIso = entity.CodigoIso, Descripcion = entity.Descripcion, TasaCambio = entity.TasaCambio };
    }

    // --- TIPOS CUENTA ---
    public async Task<List<TipoCuentaDto>> GetTiposCuentaAsync()
    {
        return await _context.TiposCuenta
            .Select(x => new TipoCuentaDto { IdTipoCuenta = x.IdTipoCuenta, Descripcion = x.Descripcion, Origen = x.Origen })
            .ToListAsync();
    }

    public async Task<TipoCuentaDto> CreateTipoCuentaAsync(CreateTipoCuentaDto dto)
    {
        var entity = new TiposCuentum { Descripcion = dto.Descripcion, Origen = dto.Origen };
        _context.TiposCuenta.Add(entity);
        await _context.SaveChangesAsync();
        return new TipoCuentaDto { IdTipoCuenta = entity.IdTipoCuenta, Descripcion = entity.Descripcion, Origen = entity.Origen };
    }

    // --- CUENTAS CONTABLES ---
    public async Task<List<CuentaContableDto>> GetCuentasContablesAsync()
    {
        return await _context.CuentasContables
            .Include(c => c.IdTipoCuentaNavigation)
            .Select(x => new CuentaContableDto 
            { 
                IdCuentaContable = x.IdCuentaContable, 
                Descripcion = x.Descripcion, 
                PermiteMovimiento = x.PermiteMovimiento ?? false,
                Nivel = x.Nivel,
                IdTipoCuenta = x.IdTipoCuenta,
                IdCuentaPadre = x.IdCuentaPadre,
                Balance = x.Balance ?? 0,
                TipoCuentaDescripcion = x.IdTipoCuentaNavigation != null ? x.IdTipoCuentaNavigation.Descripcion : ""
            })
            .ToListAsync();
    }

    public async Task<CuentaContableDto> CreateCuentaContableAsync(CreateCuentaContableDto dto)
    {
        // Lógica de Nivel: Si tiene padre, nivel = nivel padre + 1. Si no, nivel 1.
        int nivel = 1;
        if (dto.IdCuentaPadre.HasValue)
        {
            var padre = await _context.CuentasContables.FindAsync(dto.IdCuentaPadre.Value);
            if (padre == null) throw new KeyNotFoundException("Cuenta padre no encontrada");
            if (padre.PermiteMovimiento == true) throw new InvalidOperationException("No se puede crear una subcuenta debajo de una cuenta que permite movimientos.");
            nivel = padre.Nivel + 1;
            
            // Heredar tipo de cuenta si no se envía
            if (!dto.IdTipoCuenta.HasValue) dto.IdTipoCuenta = padre.IdTipoCuenta;
        }

        var entity = new CuentasContable 
        { 
            Descripcion = dto.Descripcion, 
            PermiteMovimiento = dto.PermiteMovimiento, 
            IdTipoCuenta = dto.IdTipoCuenta, 
            IdCuentaPadre = dto.IdCuentaPadre,
            Nivel = nivel,
            Balance = 0
        };
        _context.CuentasContables.Add(entity);
        await _context.SaveChangesAsync();
        
        return new CuentaContableDto 
        { 
            IdCuentaContable = entity.IdCuentaContable, 
            Descripcion = entity.Descripcion, 
            PermiteMovimiento = entity.PermiteMovimiento ?? false,
            Nivel = entity.Nivel,
            IdTipoCuenta = entity.IdTipoCuenta,
            IdCuentaPadre = entity.IdCuentaPadre,
            Balance = 0
        };
    }
}
