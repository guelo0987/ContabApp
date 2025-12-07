namespace ContabBackApp.DTOs;

// --- REPORTE 1: ESTADO DE CUENTA (CxC) ---
public class EstadoCuentaDto
{
    public int IdCliente { get; set; }
    public string NombreCliente { get; set; } = null!;
    public decimal SaldoTotal { get; set; }
    public List<MovimientoClienteDto> Movimientos { get; set; } = new();
}

public class MovimientoClienteDto
{
    public DateOnly Fecha { get; set; }
    public string TipoDoc { get; set; } = null!; // "Factura" o "Recibo"
    public string Numero { get; set; } = null!;
    public decimal Debito { get; set; }  // Aumenta deuda
    public decimal Credito { get; set; } // Disminuye deuda
    public int? IdAsientoRef { get; set; } // Para auditar
}

// --- REPORTE 2: DIARIO GENERAL (CONTABILIDAD) ---
public class AsientoReporteDto
{
    public int IdAsiento { get; set; }
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = null!;
    public string Origen { get; set; } = null!; // "CxC", "Nómina", etc.
    public List<DetalleAsientoDto> Detalles { get; set; } = new();
    public bool EstaCuadrado { get; set; } // Validación visual
}

public class DetalleAsientoDto
{
    public string Cuenta { get; set; } = null!; // Nombre de la cuenta
    public decimal Debito { get; set; }
    public decimal Credito { get; set; }
}
