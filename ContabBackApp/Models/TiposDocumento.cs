using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class TiposDocumento
{
    public int IdTipoDocumento { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? IdCuentaContable { get; set; }

    public string? Estado { get; set; }

    /// <summary>
    /// Tipo de movimiento esperado: "DB" (Débito/Venta) o "CR" (Crédito/Cobro)
    /// </summary>
    public string? TipoMovimientoEsperado { get; set; }

    /// <summary>
    /// Indica si este documento aplica ITBIS
    /// </summary>
    public bool AplicaItbis { get; set; } = true;

    /// <summary>
    /// Tasa de ITBIS a aplicar (ej: 18.00)
    /// </summary>
    public decimal TasaItbis { get; set; } = 18.00m;

    public virtual CuentasContable? IdCuentaContableNavigation { get; set; }

    public virtual ICollection<TransaccionesCxc> TransaccionesCxcs { get; set; } = new List<TransaccionesCxc>();
}
