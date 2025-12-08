using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Services;

// --- INTERFAZ CLIENTES ---
public interface IClienteService
{
    Task<List<ClienteDto>> GetAllAsync();
    Task<ClienteDto> GetByIdAsync(int id);
    Task<ClienteDto> CreateAsync(CreateClienteDto dto);
    Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto);
    Task<bool> DeleteAsync(int id);
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
                Rnc = c.Rnc,
                TipoCliente = c.TipoCliente,
                Telefono = c.Telefono,
                Email = c.Email,
                Direccion = c.Direccion,
                LimiteCredito = c.LimiteCredito,
                Estado = c.Estado
            }).ToListAsync();
    }

    public async Task<ClienteDto> GetByIdAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");

        return new ClienteDto
        {
            IdCliente = cliente.IdCliente,
            Nombre = cliente.Nombre,
            Cedula = cliente.Cedula,
            Rnc = cliente.Rnc,
            TipoCliente = cliente.TipoCliente,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            Direccion = cliente.Direccion,
            LimiteCredito = cliente.LimiteCredito,
            Estado = cliente.Estado
        };
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        // Validar tipo de cliente
        if (dto.TipoCliente != "Persona" && dto.TipoCliente != "Empresa")
            throw new ArgumentException("El tipo de cliente debe ser 'Persona' o 'Empresa'");

        // Validar unicidad de cédula
        if (!string.IsNullOrEmpty(dto.Cedula) && await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula))
            throw new InvalidOperationException($"El cliente con cédula {dto.Cedula} ya existe.");

        // Validar unicidad de RNC
        if (!string.IsNullOrEmpty(dto.Rnc) && await _context.Clientes.AnyAsync(c => c.Rnc == dto.Rnc))
            throw new InvalidOperationException($"El cliente con RNC {dto.Rnc} ya existe.");

        var nuevoCliente = new Cliente
        {
            Nombre = dto.Nombre,
            Cedula = dto.Cedula,
            Rnc = dto.Rnc,
            TipoCliente = dto.TipoCliente,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
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
            Rnc = nuevoCliente.Rnc,
            TipoCliente = nuevoCliente.TipoCliente,
            Telefono = nuevoCliente.Telefono,
            Email = nuevoCliente.Email,
            Direccion = nuevoCliente.Direccion,
            LimiteCredito = nuevoCliente.LimiteCredito,
            Estado = nuevoCliente.Estado
        };
    }

    public async Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");

        // Validar tipo de cliente
        if (dto.TipoCliente != "Persona" && dto.TipoCliente != "Empresa")
            throw new ArgumentException("El tipo de cliente debe ser 'Persona' o 'Empresa'");

        // Validar unicidad de cédula (excluyendo el actual)
        if (!string.IsNullOrEmpty(dto.Cedula) && 
            await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula && c.IdCliente != id))
            throw new InvalidOperationException($"Otro cliente ya tiene la cédula {dto.Cedula}.");

        // Validar unicidad de RNC (excluyendo el actual)
        if (!string.IsNullOrEmpty(dto.Rnc) && 
            await _context.Clientes.AnyAsync(c => c.Rnc == dto.Rnc && c.IdCliente != id))
            throw new InvalidOperationException($"Otro cliente ya tiene el RNC {dto.Rnc}.");

        cliente.Nombre = dto.Nombre;
        cliente.Cedula = dto.Cedula;
        cliente.Rnc = dto.Rnc;
        cliente.TipoCliente = dto.TipoCliente;
        cliente.Telefono = dto.Telefono;
        cliente.Email = dto.Email;
        cliente.Direccion = dto.Direccion;
        cliente.LimiteCredito = dto.LimiteCredito;
        if (!string.IsNullOrEmpty(dto.Estado)) cliente.Estado = dto.Estado;

        await _context.SaveChangesAsync();

        return new ClienteDto
        {
            IdCliente = cliente.IdCliente,
            Nombre = cliente.Nombre,
            Cedula = cliente.Cedula,
            Rnc = cliente.Rnc,
            TipoCliente = cliente.TipoCliente,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            Direccion = cliente.Direccion,
            LimiteCredito = cliente.LimiteCredito,
            Estado = cliente.Estado
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.TransaccionesCxcs)
            .FirstOrDefaultAsync(c => c.IdCliente == id);

        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");

        // Validar si tiene transacciones
        if (cliente.TransaccionesCxcs.Any())
            throw new InvalidOperationException("No se puede eliminar el cliente porque tiene transacciones asociadas.");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return true;
    }
}
