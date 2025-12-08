namespace ContabBackApp.DTOs;

public class TipoDocumentoDto
{
    public int IdTipoDocumento { get; set; }
    public string Descripcion { get; set; } = null!;
    public int? IdCuentaContable { get; set; }
    public string? NombreCuentaContable { get; set; }
    public string? Estado { get; set; }
    
    /// <summary>
    /// Tipo de movimiento esperado: "DB" (Venta) o "CR" (Cobro)
    /// </summary>
    public string? TipoMovimientoEsperado { get; set; }

    /// <summary>
    /// Indica si aplica ITBIS
    /// </summary>
    public bool AplicaItbis { get; set; }

    /// <summary>
    /// Tasa de ITBIS (ej: 18.00)
    /// </summary>
    public decimal TasaItbis { get; set; }
}

public class CreateTipoDocumentoDto
{
    public string Descripcion { get; set; } = null!;
    public int IdCuentaContable { get; set; }
    public string TipoMovimientoEsperado { get; set; } = "DB";
    public bool AplicaItbis { get; set; } = true;
    public decimal TasaItbis { get; set; } = 18.00m;
}
