using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Registro de ConexionBD (mantiene tu inyección de dependencias)
builder.Services.AddSingleton<ConexionBD>();

// Controladores + manejo unificado de errores de validación y deserialización JSON
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // 1. Detectar error de formato de fecha en FECHA_SALIDA (ej. "no-es-fecha")
            if (context.ModelState.ContainsKey("$.FECHA_SALIDA") ||
                context.ModelState.Values.Any(v => v.Errors.Any(e =>
                    e.ErrorMessage.Contains("DateTime", StringComparison.OrdinalIgnoreCase) ||
                    e.ErrorMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase))))
            {
                return new BadRequestObjectResult(
                    RespuestaApi<object>.Error("Formato de fecha inválido en FECHA_SALIDA. Use YYYY-MM-DD."));
            }

            // 2. Detectar error de parámetros de ruta no numéricos (ej. /camionesDisponibles/abc o /retornarCamion/abc)
            if (context.ModelState.Any(x =>
                (x.Key == "idPlanta" || x.Key == "idChofer" || x.Key == "idSalida") &&
                x.Value?.Errors.Count > 0))
            {
                return new BadRequestObjectResult(
                    RespuestaApi<object>.Error("El identificador enviado en la ruta debe ser un número entero válido."));
            }

            // 3. Obtener el primer error de validación en español (DataAnnotations)
            var primerError = context.ModelState
                .Where(x => x.Key != "salida" && x.Key != "$" && x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(msg => !string.IsNullOrWhiteSpace(msg) &&
                                       !msg.StartsWith("The ", StringComparison.OrdinalIgnoreCase));

            var mensaje = string.IsNullOrWhiteSpace(primerError)
                ? "Los datos enviados en la solicitud no son válidos."
                : primerError;

            return new BadRequestObjectResult(RespuestaApi<object>.Error(mensaje));
        };
    });

// Configuración de Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Ev3Api - Control de Camiones",
        Version = "v1",
        Description = "API REST estandarizada con SQL Server (EVALUACION_3)"
    });
});

// CORS abierto para Swagger y Android / Kotlin
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Habilitar Swagger en desarrollo (y opcionalmente siempre)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ============================================================================
// CAMBIO (24-11-2026): Se condiciona la redirección HTTP -> HTTPS a que NO sea
// entorno de Development.
// Motivo: la app Kotlin llama a http://10.0.2.2:5000. Con UseHttpsRedirection
// activo en Development, Kestrel responde 307 -> https://10.0.2.2:7001 y Android
// no confía en el certificado de desarrollo de ASP.NET Core, dando un
// SSLHandshakeException que PantallaBase mostraba como "No se pudo conectar
// con la API". En el PC Swagger sí funciona porque el navegador confía en el
// certificado. En Producción se conserva la redirección original.
// CÓDIGO ORIGINAL (no eliminado, solo comentado):
// app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// ============================================================================

app.UseCors("PermitirTodo");

app.UseAuthorization();

app.MapControllers();

app.Run();