using ContabBackApp.Context;
using ContabBackApp.Services;
using DotEnv.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;



var builder = WebApplication.CreateBuilder(args);

// 1. Cargar variables de entorno (Opcional, para local)
try {
    new EnvLoader().AddEnvFile("dev.env").Load();
} catch (Exception) {
    // Si no existe el archivo (ej. en Render), usará las variables del sistema
    Console.WriteLine("No se encontró dev.env, usando variables de entorno del sistema.");
}

// Configurar CORS para el frontend (Lovable y Producción)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin() // Permitir cualquier origen (Frontend en Vercel, etc)
              .AllowAnyMethod()
              .AllowAnyHeader();
              //.AllowCredentials(); // No compatible con AllowAnyOrigin
    });
});

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING")));

// --- INYECCIÓN DE DEPENDENCIAS (SERVICES) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ITipoDocumentoService, TipoDocumentoService>();
builder.Services.AddScoped<ITransaccionService, TransaccionService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<ICatalogosService, CatalogosService>();

// 2. Configurar JWT desde variables de entorno
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY no configurada");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER no configurada");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE no configurada");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Permitir Swagger en producción para que puedas probarlo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ContabBack API v1");
    c.RoutePrefix = string.Empty; // Swagger UI en la raíz
});

app.UseHttpsRedirection();

// Habilitar CORS (DEBE IR ANTES de Authentication/Authorization)
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

