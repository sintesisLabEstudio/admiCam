using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
namespace Ev3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticoController : ControllerBase
{
    private readonly ConexionBD _conexionBD;
    public DiagnosticoController(ConexionBD conexionBD)
    {
        _conexionBD = conexionBD;
    }
    [HttpGet("conexion")]
    public async Task<IActionResult> ProbarConexion()
    {
        try
        {
            await using var conexion = _conexionBD.ObtenerConexion();
            await conexion.OpenAsync();
            var resultado = new
            {
                servidor = conexion.DataSource,
                baseDatos = conexion.Database,
                estado = conexion.State.ToString()
            };
            return Ok(RespuestaApi<object>.Exito(resultado, "Conexion exitosa con SQL Server"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al conectar con SQL Server: {ex.Message}"));
        }
    }
}