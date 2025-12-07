using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContabBackApp.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize] // Protegido
public class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service)
    {
        _service = service;
    }

    // GET: api/v1/Reportes/estado-cuenta/1
    [HttpGet("estado-cuenta/{idCliente}")]
    public async Task<ActionResult<EstadoCuentaDto>> GetEstadoCuenta(int idCliente)
    {
        try
        {
            var reporte = await _service.ObtenerEstadoCuentaAsync(idCliente);
            return Ok(reporte);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Cliente no encontrado");
        }
    }

    // GET: api/v1/Reportes/diario?desde=2023-01-01&hasta=2023-12-31
    [HttpGet("diario")]
    public async Task<ActionResult<List<AsientoReporteDto>>> GetDiario([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        var reporte = await _service.ObtenerDiarioGeneralAsync(desde, hasta);
        return Ok(reporte);
    }
}
