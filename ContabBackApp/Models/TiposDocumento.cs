using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class TiposDocumento
{
    public int IdTipoDocumento { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? IdCuentaContable { get; set; }

    public string? Estado { get; set; }

    public virtual CuentasContable? IdCuentaContableNavigation { get; set; }

    public virtual ICollection<TransaccionesCxc> TransaccionesCxcs { get; set; } = new List<TransaccionesCxc>();
}
