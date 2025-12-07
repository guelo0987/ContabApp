// ===================================================================================
// CONTROLLER (Endpoints API)
// Ubicación: /Controllers/AuthController.cs
// ===================================================================================

using ContabBackApp.DTOs;
using ContabBackApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContabBackApp.Controllers;

[Route("api/v1/[controller]")] // Versionado v1 según PDF
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var resultado = await _authService.Login(request);
        if (!resultado.IsOk)
        {
            return Unauthorized(resultado);
        }
        return Ok(resultado);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var resultado = await _authService.Register(request);
        if (!resultado.IsOk)
        {
            return BadRequest(resultado);
        }
        return Ok(resultado);
    }
}
