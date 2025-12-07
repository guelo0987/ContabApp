using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

// --- INTERFAZ TIPOS DE DOCUMENTO ---
public interface ITipoDocumentoService
{
    Task<List<TipoDocumentoDto>> GetAllAsync();
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
        return await _context.TiposDocumentos
            .Select(t => new TipoDocumentoDto
            {
                IdTipoDocumento = t.IdTipoDocumento,
                Descripcion = t.Descripcion,
                IdCuentaContable = t.IdCuentaContable,
                Estado = t.Estado
            }).ToListAsync();
    }

    public async Task<TipoDocumentoDto> CreateAsync(CreateTipoDocumentoDto dto)
    {
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
            Estado = "Activo"
        };

        _context.TiposDocumentos.Add(nuevoTipo);
        await _context.SaveChangesAsync();

        return new TipoDocumentoDto
        {
            IdTipoDocumento = nuevoTipo.IdTipoDocumento,
            Descripcion = nuevoTipo.Descripcion,
            IdCuentaContable = nuevoTipo.IdCuentaContable,
            Estado = nuevoTipo.Estado
        };
    }
}
