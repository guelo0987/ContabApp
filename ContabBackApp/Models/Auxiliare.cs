using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class Auxiliare
{
    public int IdAuxiliar { get; set; }

    public string Descripcion { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<AsientosCabecera> AsientosCabeceras { get; set; } = new List<AsientosCabecera>();
}
