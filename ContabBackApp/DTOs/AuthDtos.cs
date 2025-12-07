// ===================================================================================
// DTOs (Objetos de Transferencia de Datos)
// Ubicación: /DTOs/AuthDtos.cs
// Cumple con el formato JSON del PDF "How To"
// ===================================================================================

namespace ContabBackApp.DTOs;

public class LoginRequestDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequestDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int IdAuxiliar { get; set; } // Ej: 5 para CxC
}

// Respuesta estándar basada en "How To.pdf"
public class AuthResponseDto
{
    public bool IsOk { get; set; }
    public string Message { get; set; } = null!;
    public AuthDataDto? Data { get; set; }
}

public class AuthDataDto
{
    public string Token { get; set; } = null!;
    public AuthUserDto Auth { get; set; } = null!;
    public AuxiliarySystemDto AuxiliarySystem { get; set; } = null!;
}

public class AuthUserDto
{
    public string Username { get; set; } = null!;
}

public class AuxiliarySystemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
