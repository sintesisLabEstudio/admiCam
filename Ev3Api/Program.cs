using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ConexionBD>();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            if (context.ModelState.ContainsKey("$.FECHA_SALIDA") ||
                context.ModelState.Values.Any(v => v.Errors.Any(e =>
                    e.ErrorMessage.Contains("DateTime", StringComparison.OrdinalIgnoreCase) ||
                    e.ErrorMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase))))
            {
                return new BadRequestObjectResult(
                    RespuestaApi<object>.Error("Formato de fecha inválido en FECHA_SALIDA. Use YYYY-MM-DD."));
            }
            if (context.ModelState.Any(x =>
                (x.Key == "idPlanta" || x.Key == "idChofer" || x.Key == "idSalida") &&
                x.Value?.Errors.Count > 0))
            {
                return new BadRequestObjectResult(
                    RespuestaApi<object>.Error("El identificador enviado en la ruta debe ser un número entero válido."));
            }
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("PermitirTodo");
app.UseAuthorization();
app.MapControllers();
app.Run();