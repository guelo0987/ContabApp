using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Cedula { get; set; }

    public decimal LimiteCredito { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<AsientosCabecera> AsientosCabeceras { get; set; } = new List<AsientosCabecera>();

    public virtual ICollection<TransaccionesCxc> TransaccionesCxcs { get; set; } = new List<TransaccionesCxc>();
}
