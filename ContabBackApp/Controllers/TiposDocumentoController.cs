using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContabBackApp.Controllers;

// ==========================================
// CONTROLADOR DE TIPOS DE DOCUMENTO
// ==========================================
[Route("api/v1/TiposDocumento")] // Ajuste de ruta estándar
[ApiController]
[Authorize]
public class TiposDocumentoController : ControllerBase
{
    private readonly ITipoDocumentoService _service;

    public TiposDocumentoController(ITipoDocumentoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoDocumentoDto>>> Get()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>
    /// Obtiene los tipos de documento filtrados por tipo de movimiento.
    /// DB = Facturas/Notas de Débito (para Ventas)
    /// CR = Recibos/Notas de Crédito (para Cobros)
    /// </summary>
    [HttpGet("por-movimiento/{tipoMovimiento}")]
    public async Task<ActionResult<List<TipoDocumentoDto>>> GetByTipoMovimiento(string tipoMovimiento)
    {
        if (tipoMovimiento != "DB" && tipoMovimiento != "CR")
            return BadRequest(new { error = "El tipo de movimiento debe ser 'DB' o 'CR'" });

        return Ok(await _service.GetByTipoMovimientoAsync(tipoMovimiento));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTipoDocumentoDto dto)
    {
        try
        {
            var resultado = await _service.CreateAsync(dto);
            return Ok(new { message = "Tipo de documento creado correctamente", data = resultado });
        }
        catch (ArgumentException ex) // Captura error de cuenta contable inválida
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno", detalle = ex.Message });
        }
    }
}
