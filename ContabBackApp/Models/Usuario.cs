using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int IdAuxiliar { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public bool? Activo { get; set; }

    public virtual Auxiliare? Auxiliar { get; set; }
}
