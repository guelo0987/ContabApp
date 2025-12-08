using System;
using System.Collections.Generic;

namespace ContabBackApp.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Cedula { get; set; }

    /// <summary>
    /// RNC para empresas
    /// </summary>
    public string? Rnc { get; set; }

    /// <summary>
    /// Tipo: "Persona" o "Empresa"
    /// </summary>
    public string TipoCliente { get; set; } = "Persona";

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public decimal LimiteCredito { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<AsientosCabecera> AsientosCabeceras { get; set; } = new List<AsientosCabecera>();

    public virtual ICollection<TransaccionesCxc> TransaccionesCxcs { get; set; } = new List<TransaccionesCxc>();
}
