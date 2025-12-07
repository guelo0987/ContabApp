using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
