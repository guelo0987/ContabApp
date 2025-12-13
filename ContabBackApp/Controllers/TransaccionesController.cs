using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace ContabBackApp.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize] // <--- Obligatorio: Requiere Token
public class TransaccionesController : ControllerBase
{
    private readonly ITransaccionService _service;

    public TransaccionesController(ITransaccionService service)
    {
        _service = service;
    }
    private HttpClient client = new HttpClient();

    private async Task <string> LoginToContabilidad ()
    {
        
        client.BaseAddress = new Uri("https://isofinal815-810-backend.onrender.com");
        var body = JsonSerializer.Serialize(new
        {
            username = "cxp_user",
            password = "ISO815810"
        });

        var response = await client.PostAsync(
            "/api/v1/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // grab token directly
        var token = doc.RootElement
            .GetProperty("data")
            .GetProperty("token")
            .GetString();
        return token;
    }

    private async Task <bool> RegistrarToContabilidad (string description, decimal amount, string type)
    {
        client.BaseAddress = new Uri("https://isofinal815-810-backend.onrender.com");
        var token = await LoginToContabilidad();
        client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
        var body = JsonSerializer.Serialize(new
        {

            description = description,
            amount = amount,
            movementType = type,
            accountId = 8
        });

        var response = await client.PostAsync(
            "/api/v1/accounting-entries",
            new StringContent(body, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var identrada = doc.RootElement
            .GetProperty("id").GetInt64();

        return Int64.IsPositive(identrada);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarTransaccionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 1. Extraer ID del Auxiliar del Token (Seguridad)
            // El claim "id_auxiliar" fue guardado al hacer Login.
            var idAuxiliarClaim = User.Claims.FirstOrDefault(c => c.Type == "id_auxiliar");

            if (idAuxiliarClaim == null)
                return Unauthorized(new { error = "El token es inválido o no tiene permisos de auxiliar." });

            int idAuxiliar = int.Parse(idAuxiliarClaim.Value);

            // 2. Llamar al servicio
            var resultado = await _service.RegistrarTransaccionAsync(dto, idAuxiliar);
            await RegistrarToContabilidad(dto.Concepto??"" ,dto.Monto , dto.TipoMovimiento);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) // Para errores de negocio (Ej: Límite de crédito)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            // Loguear el error real en servidor y no mostrar detalles técnicos al cliente
            return StatusCode(500, new { error = "Ocurrió un error interno procesando la transacción." });
        }
    }

    /// <summary>
    /// Obtiene el saldo actual del cliente (cuánto debe)
    /// </summary>
    [HttpGet("saldo/{idCliente}")]
    public async Task<IActionResult> ObtenerSaldo(int idCliente)
    {
        try
        {
            var saldo = await _service.ObtenerSaldoClienteAsync(idCliente);
            return Ok(saldo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de transacciones de un cliente
    /// </summary>
    [HttpGet("historial/{idCliente}")]
    public async Task<IActionResult> ObtenerHistorial(int idCliente)
    {
        try
        {
            var historial = await _service.ObtenerHistorialClienteAsync(idCliente);
            return Ok(historial);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}