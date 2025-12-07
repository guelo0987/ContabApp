using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class Moneda
{
    public int IdMoneda { get; set; }

    public string CodigoIso { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal TasaCambio { get; set; }

    public virtual ICollection<AsientosCabecera> AsientosCabeceras { get; set; } = new List<AsientosCabecera>();
}
