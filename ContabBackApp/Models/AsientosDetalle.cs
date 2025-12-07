using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class AsientosDetalle
{
    public int IdAsientoDetalle { get; set; }

    public int? IdAsiento { get; set; }

    public int? IdCuentaContable { get; set; }

    public string? TipoMovimiento { get; set; }

    public decimal Monto { get; set; }

    public virtual AsientosCabecera? IdAsientoNavigation { get; set; }

    public virtual CuentasContable? IdCuentaContableNavigation { get; set; }
}
