using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

// --- INTERFAZ CLIENTES ---
public interface IClienteService
{
    Task<List<ClienteDto>> GetAllAsync();
    Task<ClienteDto> CreateAsync(CreateClienteDto dto);
}

// --- IMPLEMENTACIÓN CLIENTES ---
public class ClienteService : IClienteService
{
    private readonly MyDbContext _context;

    public ClienteService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteDto>> GetAllAsync()
    {
        return await _context.Clientes
            .Select(c => new ClienteDto
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Cedula = c.Cedula,
                LimiteCredito = c.LimiteCredito,
                Estado = c.Estado
            }).ToListAsync();
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        // Regla de Negocio: Validar unicidad de cédula
        if (!string.IsNullOrEmpty(dto.Cedula) && await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula))
        {
            throw new InvalidOperationException($"El cliente con cédula {dto.Cedula} ya existe.");
        }

        var nuevoCliente = new Cliente
        {
            Nombre = dto.Nombre,
            Cedula = dto.Cedula,
            LimiteCredito = dto.LimiteCredito,
            Estado = "Activo"
        };

        _context.Clientes.Add(nuevoCliente);
        await _context.SaveChangesAsync();

        return new ClienteDto
        {
            IdCliente = nuevoCliente.IdCliente,
            Nombre = nuevoCliente.Nombre,
            Cedula = nuevoCliente.Cedula,
            LimiteCredito = nuevoCliente.LimiteCredito,
            Estado = nuevoCliente.Estado
        };
    }
}
