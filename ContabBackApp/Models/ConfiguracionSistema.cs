using System;

namespace ContabBackApp.Models;

/// <summary>
/// Tabla de configuración global del sistema contable
/// </summary>
public partial class ConfiguracionSistema
{
    public int IdConfiguracion { get; set; }

    /// <summary>
    /// Clave única de configuración (ej: "CUENTA_CAJA_GENERAL")
    /// </summary>
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Valor de la configuración
    /// </summary>
    public string Valor { get; set; } = null!;

    /// <summary>
    /// Descripción de la configuración
    /// </summary>
    public string? Descripcion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
