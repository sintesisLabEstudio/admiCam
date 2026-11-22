
using Ev3Api.Data;
var builder = WebApplication.CreateBuilder(args);
// 1.LEER LA CADENA DE CONEXIÓN DESDE appsettings.json
var conexionSQL = builder.Configuration.GetConnectionString("ConexionSQL");
// 2.SERVICIOS DE LA API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 3.REGISTRAR ACCESO A DATOS
builder.Services.AddScoped<ConexionBD>();
// 4.CONSTRUIR LA APLICACIÓN
var app = builder.Build();
// 5.CONFIGURAR SWAGGER
app.UseSwagger();
app.UseSwaggerUI();
// 6.AUTORIZACIÓN Y CONTROLADORES
app.UseAuthorization();
app.MapControllers();
app.Run();
