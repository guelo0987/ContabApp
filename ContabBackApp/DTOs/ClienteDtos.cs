namespace ContabBackApp.DTOs;

public class ClienteDto
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Cedula { get; set; }
    public decimal LimiteCredito { get; set; }
    public string? Estado { get; set; }
}

public class CreateClienteDto
{
    public string Nombre { get; set; } = null!;
    public string? Cedula { get; set; }
    public decimal LimiteCredito { get; set; }
}
