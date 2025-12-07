using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContabBackApp.Controllers;

// ==========================================
// CONTROLADOR DE CLIENTES
// ==========================================
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> Get()
    {
        var clientes = await _clienteService.GetAllAsync();
        return Ok(clientes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        try
        {
            var resultado = await _clienteService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = resultado.IdCliente }, resultado);
        }
        catch (InvalidOperationException ex) // Captura errores de negocio (Ej: Cédula duplicada)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }
}
