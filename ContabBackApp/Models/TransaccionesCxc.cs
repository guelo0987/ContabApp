using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class TransaccionesCxc
{
    public int IdTransaccion { get; set; }

    public string? TipoMovimiento { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string NumeroDocumento { get; set; } = null!;

    public DateOnly FechaTransaccion { get; set; }

    public int? IdCliente { get; set; }

    public decimal Monto { get; set; }

    public int? IdAsientoGenerado { get; set; }

    public virtual AsientosCabecera? IdAsientoGeneradoNavigation { get; set; }

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual TiposDocumento? IdTipoDocumentoNavigation { get; set; }
}
