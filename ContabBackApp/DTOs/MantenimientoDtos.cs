using System.ComponentModel.DataAnnotations;

namespace ContabBackApp.DTOs;

// --- AUXILIARES ---
public class AuxiliarDto
{
    public int IdAuxiliar { get; set; }
    public string Descripcion { get; set; } = null!;
    public bool? Activo { get; set; }
}

public class CreateAuxiliarDto
{
    [Required]
    public string Descripcion { get; set; } = null!;
}

public class UpdateAuxiliarDto
{
    [Required]
    public string Descripcion { get; set; } = null!;
    public bool? Activo { get; set; }
}

// --- MONEDAS ---
public class MonedaDto
{
    public int IdMoneda { get; set; }
    public string CodigoIso { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public decimal TasaCambio { get; set; }
}

public class CreateMonedaDto
{
    [Required]
    [StringLength(3)]
    public string CodigoIso { get; set; } = null!;
    [Required]
    public string Descripcion { get; set; } = null!;
    [Required]
    public decimal TasaCambio { get; set; }
}

public class UpdateMonedaDto
{
    [Required]
    [StringLength(3)]
    public string CodigoIso { get; set; } = null!;
    [Required]
    public string Descripcion { get; set; } = null!;
    [Required]
    public decimal TasaCambio { get; set; }
}

// --- TIPOS CUENTA ---
public class TipoCuentaDto
{
    public int IdTipoCuenta { get; set; }
    public string Descripcion { get; set; } = null!;
    public string? Origen { get; set; } // DB o CR
}

public class CreateTipoCuentaDto
{
    [Required]
    public string Descripcion { get; set; } = null!;
    [Required]
    [RegularExpression("^(DB|CR)$", ErrorMessage = "Origen debe ser 'DB' o 'CR'")]
    public string Origen { get; set; } = null!;
}

public class UpdateTipoCuentaDto
{
    [Required]
    public string Descripcion { get; set; } = null!;
    [Required]
    [RegularExpression("^(DB|CR)$", ErrorMessage = "Origen debe ser 'DB' o 'CR'")]
    public string Origen { get; set; } = null!;
}

// --- CUENTAS CONTABLES ---
public class CuentaContableDto
{
    public int IdCuentaContable { get; set; }
    public string? Codigo { get; set; }
    public string Descripcion { get; set; } = null!;
    public bool PermiteMovimiento { get; set; }
    public int Nivel { get; set; }
    public int? IdTipoCuenta { get; set; }
    public int? IdCuentaPadre { get; set; }
    public decimal Balance { get; set; }
    public string? TipoCuentaDescripcion { get; set; }
    public string? OrigenCuenta { get; set; } // DB o CR
}

public class CreateCuentaContableDto
{
    public string? Codigo { get; set; }
    [Required]
    public string Descripcion { get; set; } = null!;
    public bool PermiteMovimiento { get; set; }
    public int? IdTipoCuenta { get; set; }
    public int? IdCuentaPadre { get; set; }
}

public class UpdateCuentaContableDto
{
    public string? Codigo { get; set; }
    [Required]
    public string Descripcion { get; set; } = null!;
    public bool PermiteMovimiento { get; set; }
    public int? IdTipoCuenta { get; set; }
    public int? IdCuentaPadre { get; set; }
}
