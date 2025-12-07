// ===================================================================================
// SERVICE (Lógica de Negocio y Generación de JWT)
// Ubicación: /Services/AuthService.cs
// ===================================================================================

using ContabBackApp.Context;
using ContabBackApp.DTOs;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContabBackApp.Services;

public interface IAuthService
{
    Task<AuthResponseDto> Login(LoginRequestDto request);
    Task<AuthResponseDto> Register(RegisterRequestDto request);
}

public class AuthService : IAuthService
{
    private readonly MyDbContext _context;

    public AuthService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponseDto> Register(RegisterRequestDto request)
    {
        // 1. Validar si usuario existe
        if (await _context.Usuarios.AnyAsync(u => u.Username == request.Username))
        {
            return new AuthResponseDto { IsOk = false, Message = "El usuario ya existe." };
        }

        // 2. Validar si el auxiliar existe
        var auxiliar = await _context.Auxiliares.FindAsync(request.IdAuxiliar);
        if (auxiliar == null)
        {
            return new AuthResponseDto { IsOk = false, Message = "El ID de Auxiliar no es válido." };
        }

        // 3. Crear usuario
        var usuario = new Usuario
        {
            Username = request.Username,
            Password = request.Password, // Idealmente encriptar aquí
            IdAuxiliar = request.IdAuxiliar,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return new AuthResponseDto { IsOk = true, Message = "Usuario registrado exitosamente." };
    }

    public async Task<AuthResponseDto> Login(LoginRequestDto request)
    {
        // 1. Buscar usuario y traer su Auxiliar
        var usuario = await _context.Usuarios
            .Include(u => u.Auxiliar)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

        if (usuario == null)
        {
            return new AuthResponseDto { IsOk = false, Message = "Credenciales incorrectas." };
        }

        // Validar que el usuario esté activo
        if (usuario.Activo == false)
        {
            return new AuthResponseDto { IsOk = false, Message = "El usuario está inactivo." };
        }

        // 2. Generar Token
        var token = GenerarTokenJwt(usuario);

        // 3. Armar respuesta según formato PDF
        return new AuthResponseDto
        {
            IsOk = true,
            Message = "Authentication successful",
            Data = new AuthDataDto
            {
                Token = token,
                Auth = new AuthUserDto { Username = usuario.Username },
                AuxiliarySystem = new AuxiliarySystemDto
                {
                    Id = usuario.IdAuxiliar,
                    Name = usuario.Auxiliar?.Descripcion ?? "Desconocido"
                }
            }
        };
    }

    private string GenerarTokenJwt(Usuario usuario)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY no configurada en dev.env");
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER no configurada en dev.env");
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE no configurada en dev.env");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Username),
            new Claim("id_usuario", usuario.IdUsuario.ToString()),
            new Claim("id_auxiliar", usuario.IdAuxiliar.ToString()) // DATO CRÍTICO PARA TU LÓGICA
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
