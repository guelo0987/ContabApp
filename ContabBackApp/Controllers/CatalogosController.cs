using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContabBackApp.Controllers;

[Route("api/v1")]
[ApiController]
[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogosService _service;

    public CatalogosController(ICatalogosService service)
    {
        _service = service;
    }

    // --- AUXILIARES ---
    [HttpGet("auxiliares")]
    public async Task<ActionResult<List<AuxiliarDto>>> GetAuxiliares() => Ok(await _service.GetAuxiliaresAsync());

    [HttpPost("auxiliares")]
    public async Task<ActionResult<AuxiliarDto>> CreateAuxiliar(CreateAuxiliarDto dto)
    {
        var result = await _service.CreateAuxiliarAsync(dto);
        return Ok(result);
    }

    // --- MONEDAS ---
    [HttpGet("monedas")]
    public async Task<ActionResult<List<MonedaDto>>> GetMonedas() => Ok(await _service.GetMonedasAsync());

    [HttpPost("monedas")]
    public async Task<ActionResult<MonedaDto>> CreateMoneda(CreateMonedaDto dto)
    {
        var result = await _service.CreateMonedaAsync(dto);
        return Ok(result);
    }

    // --- TIPOS CUENTA ---
    [HttpGet("tipos-cuenta")]
    public async Task<ActionResult<List<TipoCuentaDto>>> GetTiposCuenta() => Ok(await _service.GetTiposCuentaAsync());

    [HttpPost("tipos-cuenta")]
    public async Task<ActionResult<TipoCuentaDto>> CreateTipoCuenta(CreateTipoCuentaDto dto)
    {
        var result = await _service.CreateTipoCuentaAsync(dto);
        return Ok(result);
    }

    // --- CUENTAS CONTABLES ---
    [HttpGet("cuentas-contables")]
    public async Task<ActionResult<List<CuentaContableDto>>> GetCuentas() => Ok(await _service.GetCuentasContablesAsync());

    [HttpPost("cuentas-contables")]
    public async Task<ActionResult<CuentaContableDto>> CreateCuenta(CreateCuentaContableDto dto)
    {
        try
        {
            var result = await _service.CreateCuentaContableAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
