using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class CuentasContable
{
    public int IdCuentaContable { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool? PermiteMovimiento { get; set; }

    public int? IdTipoCuenta { get; set; }

    public int Nivel { get; set; }

    public int? IdCuentaPadre { get; set; }

    public decimal? Balance { get; set; }

    public virtual ICollection<AsientosDetalle> AsientosDetalles { get; set; } = new List<AsientosDetalle>();

    public virtual CuentasContable? IdCuentaPadreNavigation { get; set; }

    public virtual TiposCuentum? IdTipoCuentaNavigation { get; set; }

    public virtual ICollection<CuentasContable> InverseIdCuentaPadreNavigation { get; set; } = new List<CuentasContable>();

    public virtual ICollection<TiposDocumento> TiposDocumentos { get; set; } = new List<TiposDocumento>();
}
