using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace Ev3Api.Controllers;
[ApiController]
[Route("api/Camiones")]
public class ListaBaseCompletaController : ControllerBase
{
    private readonly ConexionBD _conexion;
    public ListaBaseCompletaController(ConexionBD conexion)
    {
        _conexion = conexion;
    }
    [HttpGet("listaBaseCompleta")]
    public async Task<IActionResult> ObtenerBaseCompleta()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            var tablas = new Dictionary<string, List<Dictionary<string, object?>>>
            {
                ["ESTADO"] = await LeerTablaAsync(conn, "SELECT ID_ESTADO, NOM_ESTADO FROM ESTADO ORDER BY ID_ESTADO"),
                ["ESTADO_SALIDA"] = await LeerTablaAsync(conn, "SELECT ID_ESTADO_SALIDA, NOM_ESTADO_SALIDA FROM ESTADO_SALIDA ORDER BY ID_ESTADO_SALIDA"),
                ["PLANTA"] = await LeerTablaAsync(conn, "SELECT ID_PLANTA, NOM_PLANTA FROM PLANTA ORDER BY ID_PLANTA"),
                ["USUARIOS"] = await LeerTablaAsync(conn, "SELECT USUARIO, NOM_PERSONA, PASSWORD, ID_ESTADO FROM USUARIOS ORDER BY USUARIO"),
                ["CHOFER"] = await LeerTablaAsync(conn, "SELECT ID_CHOFER, NOM_CHOFER, ID_ESTADO FROM CHOFER ORDER BY ID_CHOFER"),
                ["CAMION"] = await LeerTablaAsync(conn, "SELECT COD_CAMION, ID_PLANTA, ID_ESTADO FROM CAMION ORDER BY COD_CAMION"),
                ["SALIDA"] = await LeerTablaAsync(conn, "SELECT ID_SALIDA, FECHA_SALIDA, ID_PLANTA, COD_CAMION, ID_CHOFER, ID_ESTADO_SALIDA, USUARIO FROM SALIDA ORDER BY ID_SALIDA")
            };
            var dato = new
            {
                baseDatos = conn.Database,
                tablas
            };
            return Ok(RespuestaApi<object>.Exito(dato, "Datos de la base de datos obtenidos."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al obtener la base de datos: {ex.Message}"));
        }
    }
    private static async Task<List<Dictionary<string, object?>>> LeerTablaAsync(SqlConnection conn, string sql)
    {
        var filas = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var fila = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                fila[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            filas.Add(fila);
        }
        return filas;
    }
}