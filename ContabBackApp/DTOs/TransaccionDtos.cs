using System.ComponentModel.DataAnnotations;

namespace ContabBackApp.DTOs;

public class RegistrarTransaccionDto
{
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    public int IdTipoDocumento { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio")]
    public string NumeroDocumento { get; set; } = null!; // Ej: "B01-00001"

    [Required]
    // Solo aceptamos "DB" (Deuda/Venta) o "CR" (Abono/Cobro)
    [RegularExpression("^(DB|CR)$", ErrorMessage = "El tipo de movimiento debe ser 'DB' o 'CR'")]
    public string TipoMovimiento { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    public string? Concepto { get; set; } // Opcional
}

public class TransaccionResponseDto
{
    public int IdTransaccion { get; set; }
    public int IdAsientoGenerado { get; set; }
    public string Mensaje { get; set; } = null!;
}
