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

    [HttpGet("auxiliares/{id}")]
    public async Task<ActionResult<AuxiliarDto>> GetAuxiliar(int id)
    {
        try
        {
            var result = await _service.GetAuxiliarByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("auxiliares")]
    public async Task<ActionResult<AuxiliarDto>> CreateAuxiliar(CreateAuxiliarDto dto)
    {
        var result = await _service.CreateAuxiliarAsync(dto);
        return Ok(result);
    }

    [HttpPut("auxiliares/{id}")]
    public async Task<ActionResult<AuxiliarDto>> UpdateAuxiliar(int id, UpdateAuxiliarDto dto)
    {
        try
        {
            var result = await _service.UpdateAuxiliarAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("auxiliares/{id}")]
    public async Task<ActionResult> DeleteAuxiliar(int id)
    {
        try
        {
            await _service.DeleteAuxiliarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // --- MONEDAS ---
    [HttpGet("monedas")]
    public async Task<ActionResult<List<MonedaDto>>> GetMonedas() => Ok(await _service.GetMonedasAsync());

    [HttpGet("monedas/{id}")]
    public async Task<ActionResult<MonedaDto>> GetMoneda(int id)
    {
        try
        {
            var result = await _service.GetMonedaByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("monedas")]
    public async Task<ActionResult<MonedaDto>> CreateMoneda(CreateMonedaDto dto)
    {
        var result = await _service.CreateMonedaAsync(dto);
        return Ok(result);
    }

    [HttpPut("monedas/{id}")]
    public async Task<ActionResult<MonedaDto>> UpdateMoneda(int id, UpdateMonedaDto dto)
    {
        try
        {
            var result = await _service.UpdateMonedaAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("monedas/{id}")]
    public async Task<ActionResult> DeleteMoneda(int id)
    {
        try
        {
            await _service.DeleteMonedaAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // --- TIPOS CUENTA ---
    [HttpGet("tipos-cuenta")]
    public async Task<ActionResult<List<TipoCuentaDto>>> GetTiposCuenta() => Ok(await _service.GetTiposCuentaAsync());

    [HttpGet("tipos-cuenta/{id}")]
    public async Task<ActionResult<TipoCuentaDto>> GetTipoCuenta(int id)
    {
        try
        {
            var result = await _service.GetTipoCuentaByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("tipos-cuenta")]
    public async Task<ActionResult<TipoCuentaDto>> CreateTipoCuenta(CreateTipoCuentaDto dto)
    {
        var result = await _service.CreateTipoCuentaAsync(dto);
        return Ok(result);
    }

    [HttpPut("tipos-cuenta/{id}")]
    public async Task<ActionResult<TipoCuentaDto>> UpdateTipoCuenta(int id, UpdateTipoCuentaDto dto)
    {
        try
        {
            var result = await _service.UpdateTipoCuentaAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("tipos-cuenta/{id}")]
    public async Task<ActionResult> DeleteTipoCuenta(int id)
    {
        try
        {
            await _service.DeleteTipoCuentaAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // --- CUENTAS CONTABLES ---
    [HttpGet("cuentas-contables")]
    public async Task<ActionResult<List<CuentaContableDto>>> GetCuentas() => Ok(await _service.GetCuentasContablesAsync());

    [HttpGet("cuentas-contables/{id}")]
    public async Task<ActionResult<CuentaContableDto>> GetCuenta(int id)
    {
        try
        {
            var result = await _service.GetCuentaContableByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

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

    [HttpPut("cuentas-contables/{id}")]
    public async Task<ActionResult<CuentaContableDto>> UpdateCuenta(int id, UpdateCuentaContableDto dto)
    {
        try
        {
            var result = await _service.UpdateCuentaContableAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("cuentas-contables/{id}")]
    public async Task<ActionResult> DeleteCuenta(int id)
    {
        try
        {
            await _service.DeleteCuentaContableAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
