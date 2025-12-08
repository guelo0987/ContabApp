using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

public interface ICatalogosService
{
    // Auxiliares
    Task<List<AuxiliarDto>> GetAuxiliaresAsync();
    Task<AuxiliarDto> GetAuxiliarByIdAsync(int id);
    Task<AuxiliarDto> CreateAuxiliarAsync(CreateAuxiliarDto dto);
    Task<AuxiliarDto> UpdateAuxiliarAsync(int id, UpdateAuxiliarDto dto);
    Task<bool> DeleteAuxiliarAsync(int id);

    // Monedas
    Task<List<MonedaDto>> GetMonedasAsync();
    Task<MonedaDto> GetMonedaByIdAsync(int id);
    Task<MonedaDto> CreateMonedaAsync(CreateMonedaDto dto);
    Task<MonedaDto> UpdateMonedaAsync(int id, UpdateMonedaDto dto);
    Task<bool> DeleteMonedaAsync(int id);

    // Tipos Cuenta
    Task<List<TipoCuentaDto>> GetTiposCuentaAsync();
    Task<TipoCuentaDto> GetTipoCuentaByIdAsync(int id);
    Task<TipoCuentaDto> CreateTipoCuentaAsync(CreateTipoCuentaDto dto);
    Task<TipoCuentaDto> UpdateTipoCuentaAsync(int id, UpdateTipoCuentaDto dto);
    Task<bool> DeleteTipoCuentaAsync(int id);

    // Cuentas Contables
    Task<List<CuentaContableDto>> GetCuentasContablesAsync();
    Task<CuentaContableDto> GetCuentaContableByIdAsync(int id);
    Task<CuentaContableDto> CreateCuentaContableAsync(CreateCuentaContableDto dto);
    Task<CuentaContableDto> UpdateCuentaContableAsync(int id, UpdateCuentaContableDto dto);
    Task<bool> DeleteCuentaContableAsync(int id);
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

    public async Task<AuxiliarDto> GetAuxiliarByIdAsync(int id)
    {
        var entity = await _context.Auxiliares.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Auxiliar con ID {id} no encontrado.");
        return new AuxiliarDto { IdAuxiliar = entity.IdAuxiliar, Descripcion = entity.Descripcion, Activo = entity.Activo };
    }

    public async Task<AuxiliarDto> CreateAuxiliarAsync(CreateAuxiliarDto dto)
    {
        var entity = new Auxiliare { Descripcion = dto.Descripcion, Activo = true };
        _context.Auxiliares.Add(entity);
        await _context.SaveChangesAsync();
        return new AuxiliarDto { IdAuxiliar = entity.IdAuxiliar, Descripcion = entity.Descripcion, Activo = entity.Activo };
    }

    public async Task<AuxiliarDto> UpdateAuxiliarAsync(int id, UpdateAuxiliarDto dto)
    {
        var entity = await _context.Auxiliares.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Auxiliar con ID {id} no encontrado.");

        entity.Descripcion = dto.Descripcion;
        if (dto.Activo.HasValue) entity.Activo = dto.Activo.Value;

        await _context.SaveChangesAsync();
        return new AuxiliarDto { IdAuxiliar = entity.IdAuxiliar, Descripcion = entity.Descripcion, Activo = entity.Activo };
    }

    public async Task<bool> DeleteAuxiliarAsync(int id)
    {
        var entity = await _context.Auxiliares
            .Include(a => a.AsientosCabeceras)
            .FirstOrDefaultAsync(a => a.IdAuxiliar == id);
        
        if (entity == null) throw new KeyNotFoundException($"Auxiliar con ID {id} no encontrado.");

        // Validar si tiene asientos asociados
        if (entity.AsientosCabeceras.Any())
            throw new InvalidOperationException("No se puede eliminar el auxiliar porque tiene asientos contables asociados.");

        _context.Auxiliares.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- MONEDAS ---
    public async Task<List<MonedaDto>> GetMonedasAsync()
    {
        return await _context.Monedas
            .Select(x => new MonedaDto { IdMoneda = x.IdMoneda, CodigoIso = x.CodigoIso, Descripcion = x.Descripcion, TasaCambio = x.TasaCambio })
            .ToListAsync();
    }

    public async Task<MonedaDto> GetMonedaByIdAsync(int id)
    {
        var entity = await _context.Monedas.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Moneda con ID {id} no encontrada.");
        return new MonedaDto { IdMoneda = entity.IdMoneda, CodigoIso = entity.CodigoIso, Descripcion = entity.Descripcion, TasaCambio = entity.TasaCambio };
    }

    public async Task<MonedaDto> CreateMonedaAsync(CreateMonedaDto dto)
    {
        var entity = new Moneda { CodigoIso = dto.CodigoIso, Descripcion = dto.Descripcion, TasaCambio = dto.TasaCambio };
        _context.Monedas.Add(entity);
        await _context.SaveChangesAsync();
        return new MonedaDto { IdMoneda = entity.IdMoneda, CodigoIso = entity.CodigoIso, Descripcion = entity.Descripcion, TasaCambio = entity.TasaCambio };
    }

    public async Task<MonedaDto> UpdateMonedaAsync(int id, UpdateMonedaDto dto)
    {
        var entity = await _context.Monedas.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Moneda con ID {id} no encontrada.");

        entity.CodigoIso = dto.CodigoIso;
        entity.Descripcion = dto.Descripcion;
        entity.TasaCambio = dto.TasaCambio;

        await _context.SaveChangesAsync();
        return new MonedaDto { IdMoneda = entity.IdMoneda, CodigoIso = entity.CodigoIso, Descripcion = entity.Descripcion, TasaCambio = entity.TasaCambio };
    }

    public async Task<bool> DeleteMonedaAsync(int id)
    {
        var entity = await _context.Monedas
            .Include(m => m.AsientosCabeceras)
            .FirstOrDefaultAsync(m => m.IdMoneda == id);
        
        if (entity == null) throw new KeyNotFoundException($"Moneda con ID {id} no encontrada.");

        // Validar si tiene asientos asociados
        if (entity.AsientosCabeceras.Any())
            throw new InvalidOperationException("No se puede eliminar la moneda porque tiene asientos contables asociados.");

        _context.Monedas.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- TIPOS CUENTA ---
    public async Task<List<TipoCuentaDto>> GetTiposCuentaAsync()
    {
        return await _context.TiposCuenta
            .Select(x => new TipoCuentaDto { IdTipoCuenta = x.IdTipoCuenta, Descripcion = x.Descripcion, Origen = x.Origen })
            .ToListAsync();
    }

    public async Task<TipoCuentaDto> GetTipoCuentaByIdAsync(int id)
    {
        var entity = await _context.TiposCuenta.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Tipo de cuenta con ID {id} no encontrado.");
        return new TipoCuentaDto { IdTipoCuenta = entity.IdTipoCuenta, Descripcion = entity.Descripcion, Origen = entity.Origen };
    }

    public async Task<TipoCuentaDto> CreateTipoCuentaAsync(CreateTipoCuentaDto dto)
    {
        var entity = new TiposCuentum { Descripcion = dto.Descripcion, Origen = dto.Origen };
        _context.TiposCuenta.Add(entity);
        await _context.SaveChangesAsync();
        return new TipoCuentaDto { IdTipoCuenta = entity.IdTipoCuenta, Descripcion = entity.Descripcion, Origen = entity.Origen };
    }

    public async Task<TipoCuentaDto> UpdateTipoCuentaAsync(int id, UpdateTipoCuentaDto dto)
    {
        var entity = await _context.TiposCuenta.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Tipo de cuenta con ID {id} no encontrado.");

        entity.Descripcion = dto.Descripcion;
        entity.Origen = dto.Origen;

        await _context.SaveChangesAsync();
        return new TipoCuentaDto { IdTipoCuenta = entity.IdTipoCuenta, Descripcion = entity.Descripcion, Origen = entity.Origen };
    }

    public async Task<bool> DeleteTipoCuentaAsync(int id)
    {
        var entity = await _context.TiposCuenta
            .Include(t => t.CuentasContables)
            .FirstOrDefaultAsync(t => t.IdTipoCuenta == id);
        
        if (entity == null) throw new KeyNotFoundException($"Tipo de cuenta con ID {id} no encontrado.");

        // Validar si tiene cuentas contables asociadas
        if (entity.CuentasContables.Any())
            throw new InvalidOperationException("No se puede eliminar el tipo de cuenta porque tiene cuentas contables asociadas.");

        _context.TiposCuenta.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // --- CUENTAS CONTABLES ---
    public async Task<List<CuentaContableDto>> GetCuentasContablesAsync()
    {
        return await _context.CuentasContables
            .Include(c => c.IdTipoCuentaNavigation)
            .Select(x => new CuentaContableDto 
            { 
                IdCuentaContable = x.IdCuentaContable,
                Codigo = x.Codigo,
                Descripcion = x.Descripcion, 
                PermiteMovimiento = x.PermiteMovimiento ?? false,
                Nivel = x.Nivel,
                IdTipoCuenta = x.IdTipoCuenta,
                IdCuentaPadre = x.IdCuentaPadre,
                Balance = x.Balance ?? 0,
                TipoCuentaDescripcion = x.IdTipoCuentaNavigation != null ? x.IdTipoCuentaNavigation.Descripcion : null,
                OrigenCuenta = x.IdTipoCuentaNavigation != null ? x.IdTipoCuentaNavigation.Origen : null
            })
            .ToListAsync();
    }

    public async Task<CuentaContableDto> GetCuentaContableByIdAsync(int id)
    {
        var entity = await _context.CuentasContables
            .Include(c => c.IdTipoCuentaNavigation)
            .FirstOrDefaultAsync(c => c.IdCuentaContable == id);
        
        if (entity == null) throw new KeyNotFoundException($"Cuenta contable con ID {id} no encontrada.");
        
        return new CuentaContableDto 
        { 
            IdCuentaContable = entity.IdCuentaContable,
            Codigo = entity.Codigo,
            Descripcion = entity.Descripcion, 
            PermiteMovimiento = entity.PermiteMovimiento ?? false,
            Nivel = entity.Nivel,
            IdTipoCuenta = entity.IdTipoCuenta,
            IdCuentaPadre = entity.IdCuentaPadre,
            Balance = entity.Balance ?? 0,
            TipoCuentaDescripcion = entity.IdTipoCuentaNavigation != null ? entity.IdTipoCuentaNavigation.Descripcion : null,
            OrigenCuenta = entity.IdTipoCuentaNavigation != null ? entity.IdTipoCuentaNavigation.Origen : null
        };
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
            Codigo = dto.Codigo,
            Descripcion = dto.Descripcion, 
            PermiteMovimiento = dto.PermiteMovimiento, 
            IdTipoCuenta = dto.IdTipoCuenta, 
            IdCuentaPadre = dto.IdCuentaPadre,
            Nivel = nivel,
            Balance = 0
        };
        _context.CuentasContables.Add(entity);
        await _context.SaveChangesAsync();
        
        // Cargar la navegación para obtener el nombre del tipo de cuenta
        await _context.Entry(entity)
            .Reference(e => e.IdTipoCuentaNavigation)
            .LoadAsync();
        
        return new CuentaContableDto 
        { 
            IdCuentaContable = entity.IdCuentaContable,
            Codigo = entity.Codigo,
            Descripcion = entity.Descripcion, 
            PermiteMovimiento = entity.PermiteMovimiento ?? false,
            Nivel = entity.Nivel,
            IdTipoCuenta = entity.IdTipoCuenta,
            IdCuentaPadre = entity.IdCuentaPadre,
            Balance = 0,
            TipoCuentaDescripcion = entity.IdTipoCuentaNavigation?.Descripcion ?? "",
            OrigenCuenta = entity.IdTipoCuentaNavigation?.Origen
        };
    }

    public async Task<CuentaContableDto> UpdateCuentaContableAsync(int id, UpdateCuentaContableDto dto)
    {
        var entity = await _context.CuentasContables.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Cuenta contable con ID {id} no encontrada.");

        // Validar si se está cambiando el padre
        if (dto.IdCuentaPadre.HasValue && dto.IdCuentaPadre != entity.IdCuentaPadre)
        {
            var nuevoPadre = await _context.CuentasContables.FindAsync(dto.IdCuentaPadre.Value);
            if (nuevoPadre == null) throw new KeyNotFoundException("Cuenta padre no encontrada");
            if (nuevoPadre.PermiteMovimiento == true) 
                throw new InvalidOperationException("No se puede mover la cuenta debajo de una cuenta que permite movimientos.");
            
            // Validar que no se esté moviendo a una cuenta hija (evitar ciclos)
            if (await EsCuentaHijaAsync(id, dto.IdCuentaPadre.Value))
                throw new InvalidOperationException("No se puede mover una cuenta a una de sus subcuentas.");

            entity.IdCuentaPadre = dto.IdCuentaPadre;
            entity.Nivel = nuevoPadre.Nivel + 1;
        }

        // Validar si se está cambiando PermiteMovimiento
        if (dto.PermiteMovimiento != entity.PermiteMovimiento)
        {
            // Si se está activando PermiteMovimiento, verificar que no tenga subcuentas
            if (dto.PermiteMovimiento)
            {
                var tieneSubcuentas = await _context.CuentasContables
                    .AnyAsync(c => c.IdCuentaPadre == id);
                if (tieneSubcuentas)
                    throw new InvalidOperationException("No se puede activar 'PermiteMovimiento' en una cuenta que tiene subcuentas.");
            }
        }

        entity.Codigo = dto.Codigo;
        entity.Descripcion = dto.Descripcion;
        entity.PermiteMovimiento = dto.PermiteMovimiento;
        
        if (dto.IdTipoCuenta.HasValue) entity.IdTipoCuenta = dto.IdTipoCuenta;

        await _context.SaveChangesAsync();

        // Cargar la navegación para obtener el nombre del tipo de cuenta
        await _context.Entry(entity)
            .Reference(e => e.IdTipoCuentaNavigation)
            .LoadAsync();

        return new CuentaContableDto 
        { 
            IdCuentaContable = entity.IdCuentaContable,
            Codigo = entity.Codigo,
            Descripcion = entity.Descripcion, 
            PermiteMovimiento = entity.PermiteMovimiento ?? false,
            Nivel = entity.Nivel,
            IdTipoCuenta = entity.IdTipoCuenta,
            IdCuentaPadre = entity.IdCuentaPadre,
            Balance = entity.Balance ?? 0,
            TipoCuentaDescripcion = entity.IdTipoCuentaNavigation?.Descripcion ?? "",
            OrigenCuenta = entity.IdTipoCuentaNavigation?.Origen
        };
    }

    public async Task<bool> DeleteCuentaContableAsync(int id)
    {
        var entity = await _context.CuentasContables
            .Include(c => c.AsientosDetalles)
            .Include(c => c.InverseIdCuentaPadreNavigation)
            .FirstOrDefaultAsync(c => c.IdCuentaContable == id);
        
        if (entity == null) throw new KeyNotFoundException($"Cuenta contable con ID {id} no encontrada.");

        // Validar si tiene asientos asociados
        if (entity.AsientosDetalles.Any())
            throw new InvalidOperationException("No se puede eliminar la cuenta contable porque tiene asientos contables asociados.");

        // Validar si tiene subcuentas
        if (entity.InverseIdCuentaPadreNavigation.Any())
            throw new InvalidOperationException("No se puede eliminar la cuenta contable porque tiene subcuentas asociadas.");

        _context.CuentasContables.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // Método auxiliar para validar si una cuenta es hija de otra (evitar ciclos)
    private async Task<bool> EsCuentaHijaAsync(int idCuenta, int idPosiblePadre)
    {
        var cuenta = await _context.CuentasContables.FindAsync(idPosiblePadre);
        if (cuenta == null || cuenta.IdCuentaPadre == null) return false;
        if (cuenta.IdCuentaPadre == idCuenta) return true;
        return await EsCuentaHijaAsync(idCuenta, cuenta.IdCuentaPadre.Value);
    }
}
