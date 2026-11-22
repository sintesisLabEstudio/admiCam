
using Microsoft.AspNetCore.Mvc;
using Ev3Api.Data;
namespace Ev3Api.Controllers
{
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
                return Ok(new
                {
                    mensaje = "Conexión exitosa con SQL Server",
                    servidor = conexion.DataSource,
                    baseDatos = conexion.Database,
                    estado = conexion.State.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al conectar con SQL Server",
                    detalle = ex.Message
                });
            }
        }
    }
}
