namespace ContabBackApp.DTOs;

public class ClienteDto
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Cedula { get; set; }
    public string? Rnc { get; set; }
    public string TipoCliente { get; set; } = "Persona";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal LimiteCredito { get; set; }
    public string? Estado { get; set; }
}

public class CreateClienteDto
{
    public string Nombre { get; set; } = null!;
    public string? Cedula { get; set; }
    public string? Rnc { get; set; }
    public string TipoCliente { get; set; } = "Persona";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal LimiteCredito { get; set; }
}

public class UpdateClienteDto
{
    public string Nombre { get; set; } = null!;
    public string? Cedula { get; set; }
    public string? Rnc { get; set; }
    public string TipoCliente { get; set; } = "Persona";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal LimiteCredito { get; set; }
    public string? Estado { get; set; }
}
