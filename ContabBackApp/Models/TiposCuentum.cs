using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class TiposCuentum
{
    public int IdTipoCuenta { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? Origen { get; set; }

    public virtual ICollection<CuentasContable> CuentasContables { get; set; } = new List<CuentasContable>();
}
