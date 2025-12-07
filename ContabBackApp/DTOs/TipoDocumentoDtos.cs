namespace ContabBackApp.DTOs;

public class TipoDocumentoDto
{
    public int IdTipoDocumento { get; set; }
    public string Descripcion { get; set; } = null!;
    public int? IdCuentaContable { get; set; }
    public string? Estado { get; set; }
}

public class CreateTipoDocumentoDto
{
    public string Descripcion { get; set; } = null!;
    public int IdCuentaContable { get; set; }
}
