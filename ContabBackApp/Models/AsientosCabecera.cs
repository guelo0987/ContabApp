using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class AsientosCabecera
{
    public int IdAsiento { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? IdAuxiliar { get; set; }

    public DateOnly FechaAsiento { get; set; }

    public int? IdMoneda { get; set; }

    public decimal? TasaCambio { get; set; }

    public int? IdCliente { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<AsientosDetalle> AsientosDetalles { get; set; } = new List<AsientosDetalle>();

    public virtual Auxiliare? IdAuxiliarNavigation { get; set; }

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Moneda? IdMonedaNavigation { get; set; }

    public virtual ICollection<TransaccionesCxc> TransaccionesCxcs { get; set; } = new List<TransaccionesCxc>();
}
