using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

// --- INTERFAZ TIPOS DE DOCUMENTO ---
public interface ITipoDocumentoService
{
    Task<List<TipoDocumentoDto>> GetAllAsync();
    Task<List<TipoDocumentoDto>> GetByTipoMovimientoAsync(string tipoMovimiento);
    Task<TipoDocumentoDto> CreateAsync(CreateTipoDocumentoDto dto);
}

// --- IMPLEMENTACIÓN TIPOS DE DOCUMENTO ---
public class TipoDocumentoService : ITipoDocumentoService
{
    private readonly MyDbContext _context;

    public TipoDocumentoService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoDocumentoDto>> GetAllAsync()
    {
        var tipos = await _context.TiposDocumentos
            .Include(t => t.IdCuentaContableNavigation)
            .Where(t => t.Estado == "Activo")
            .ToListAsync();

        return tipos.Select(t => MapToDto(t)).ToList();
    }

    public async Task<List<TipoDocumentoDto>> GetByTipoMovimientoAsync(string tipoMovimiento)
    {
        var tipos = await _context.TiposDocumentos
            .Include(t => t.IdCuentaContableNavigation)
            .Where(t => t.Estado == "Activo" && t.TipoMovimientoEsperado == tipoMovimiento)
            .ToListAsync();

        return tipos.Select(t => MapToDto(t)).ToList();
    }

    private TipoDocumentoDto MapToDto(TiposDocumento t)
    {
        return new TipoDocumentoDto
        {
            IdTipoDocumento = t.IdTipoDocumento,
            Descripcion = t.Descripcion,
            IdCuentaContable = t.IdCuentaContable,
            NombreCuentaContable = t.IdCuentaContableNavigation?.Descripcion,
            Estado = t.Estado,
            TipoMovimientoEsperado = t.TipoMovimientoEsperado ?? "DB",
            AplicaItbis = t.AplicaItbis,
            TasaItbis = t.TasaItbis
        };
    }

    public async Task<TipoDocumentoDto> CreateAsync(CreateTipoDocumentoDto dto)
    {
        // Validar tipo de movimiento
        if (dto.TipoMovimientoEsperado != "DB" && dto.TipoMovimientoEsperado != "CR")
            throw new ArgumentException("El tipo de movimiento debe ser 'DB' o 'CR'");

        // Regla de Negocio: Validar que la cuenta contable sea válida para imputar
        var cuentaValida = await _context.CuentasContables
            .AnyAsync(c => c.IdCuentaContable == dto.IdCuentaContable && c.PermiteMovimiento == true);

        if (!cuentaValida)
        {
            throw new ArgumentException("La cuenta contable no existe o es una cuenta padre (no permite movimientos).");
        }

        var nuevoTipo = new TiposDocumento
        {
            Descripcion = dto.Descripcion,
            IdCuentaContable = dto.IdCuentaContable,
            TipoMovimientoEsperado = dto.TipoMovimientoEsperado,
            AplicaItbis = dto.AplicaItbis,
            TasaItbis = dto.TasaItbis,
            Estado = "Activo"
        };

        _context.TiposDocumentos.Add(nuevoTipo);
        await _context.SaveChangesAsync();

        return MapToDto(nuevoTipo);
    }
}
