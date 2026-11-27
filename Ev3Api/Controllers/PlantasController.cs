using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace Ev3Api.Controllers;
[ApiController]
[Route("api/Camiones")]
public class PlantaController : ControllerBase
{
    private readonly ConexionBD _conexion;
    public PlantaController(ConexionBD conexion)
    {
        _conexion = conexion;
    }
    [HttpGet("listarPlantas")]
    public async Task<IActionResult> ListarPlantas()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sql = "SELECT ID_PLANTA, NOM_PLANTA FROM PLANTA ORDER BY NOM_PLANTA;";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<Planta>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Planta
                {
                    IdPlanta = Convert.ToInt32(reader["ID_PLANTA"]),
                    NombrePlanta = reader["NOM_PLANTA"].ToString()?.Trim() ?? string.Empty
                });
            }
            return Ok(RespuestaApi<List<Planta>>.Exito(lista, "Plantas obtenidas correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar plantas: {ex.Message}"));
        }
    }
    [HttpPost("agregarNuevaPlanta")]
    public async Task<IActionResult> AgregarNuevaPlanta([FromBody] Planta nueva)
    {
        var nombre = nueva.NombrePlanta.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return BadRequest(RespuestaApi<object>.Error("Nombre de planta no válido."));
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sqlExiste = "SELECT COUNT(1) FROM PLANTA WHERE UPPER(LTRIM(RTRIM(NOM_PLANTA))) = UPPER(@Nombre);";
            await using (var cmdExiste = new SqlCommand(sqlExiste, conn))
            {
                cmdExiste.Parameters.AddWithValue("@Nombre", nombre);
                if (Convert.ToInt32(await cmdExiste.ExecuteScalarAsync()) > 0)
                    return Conflict(RespuestaApi<object>.Error("Ya existe una planta con ese nombre."));
            }
            const string sqlInsert = @"
                INSERT INTO PLANTA (NOM_PLANTA)
                OUTPUT INSERTED.ID_PLANTA
                VALUES (@Nombre);";
            int nuevoId;
            await using (var cmdInsert = new SqlCommand(sqlInsert, conn))
            {
                cmdInsert.Parameters.AddWithValue("@Nombre", nombre);
                nuevoId = Convert.ToInt32(await cmdInsert.ExecuteScalarAsync());
            }
            var creada = new Planta { IdPlanta = nuevoId, NombrePlanta = nombre };
            return Ok(RespuestaApi<Planta>.Exito(creada, "Planta creada correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al crear planta: {ex.Message}"));
        }
    }
}