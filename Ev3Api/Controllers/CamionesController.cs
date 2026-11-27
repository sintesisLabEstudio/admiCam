using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace Ev3Api.Controllers;
[ApiController]
[Route("api/Camiones")]
public class CamionesController : ControllerBase
{
    private readonly ConexionBD _conexion;
    public CamionesController(ConexionBD conexion)
    {
        _conexion = conexion;
    }
    [HttpGet("listarCamiones")]
    public async Task<IActionResult> ListarCamiones()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sql = @"
                SELECT c.COD_CAMION, c.ID_PLANTA, p.NOM_PLANTA, c.ID_ESTADO, e.NOM_ESTADO
                FROM CAMION c
                INNER JOIN PLANTA p ON c.ID_PLANTA = p.ID_PLANTA
                INNER JOIN ESTADO e ON c.ID_ESTADO = e.ID_ESTADO
                ORDER BY c.COD_CAMION;";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<Camion>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Camion
                {
                    CodigoCamion = reader["COD_CAMION"].ToString()?.Trim() ?? string.Empty,
                    IdPlanta = Convert.ToInt32(reader["ID_PLANTA"]),
                    NombrePlanta = reader["NOM_PLANTA"].ToString()?.Trim(),
                    IdEstado = Convert.ToInt32(reader["ID_ESTADO"]),
                    NombreEstado = reader["NOM_ESTADO"].ToString()?.Trim()
                });
            }
            return Ok(RespuestaApi<List<Camion>>.Exito(lista, "Camiones obtenidos correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar camiones: {ex.Message}"));
        }
    }
    [HttpGet("camionesDisponibles/{idPlanta:int}")]
    public async Task<IActionResult> ListarCamionesDisponibles(int idPlanta)
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sql = @"
                SELECT c.COD_CAMION, c.ID_PLANTA, p.NOM_PLANTA, c.ID_ESTADO, e.NOM_ESTADO
                FROM CAMION c
                INNER JOIN PLANTA p ON c.ID_PLANTA = p.ID_PLANTA
                INNER JOIN ESTADO e ON c.ID_ESTADO = e.ID_ESTADO
                WHERE c.ID_PLANTA = @IdPlanta
                  AND c.ID_ESTADO = @Activo
                  AND NOT EXISTS (
                      SELECT 1 FROM SALIDA s
                      WHERE UPPER(LTRIM(RTRIM(s.COD_CAMION))) = UPPER(LTRIM(RTRIM(c.COD_CAMION)))
                        AND s.ID_ESTADO_SALIDA = 1
                  )
                ORDER BY c.COD_CAMION;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IdPlanta", idPlanta);
            cmd.Parameters.AddWithValue("@Activo", Estado.Activo);
            await using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<Camion>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Camion
                {
                    CodigoCamion = reader["COD_CAMION"].ToString()?.Trim() ?? string.Empty,
                    IdPlanta = Convert.ToInt32(reader["ID_PLANTA"]),
                    NombrePlanta = reader["NOM_PLANTA"].ToString()?.Trim(),
                    IdEstado = Convert.ToInt32(reader["ID_ESTADO"]),
                    NombreEstado = reader["NOM_ESTADO"].ToString()?.Trim()
                });
            }
            if (lista.Count == 0)
                return Ok(RespuestaApi<List<Camion>>.Exito(lista, "No hay camiones disponibles."));
            return Ok(RespuestaApi<List<Camion>>.Exito(lista, "Camiones disponibles obtenidos."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar camiones disponibles: {ex.Message}"));
        }
    }
    [HttpPost("agregarNuevoCamion")]
    public async Task<IActionResult> AgregarNuevoCamion([FromBody] Camion nuevo)
    {
        var codigo = nuevo.CodigoCamion.Trim().ToUpperInvariant();
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sqlDup = "SELECT COUNT(1) FROM CAMION WHERE UPPER(LTRIM(RTRIM(COD_CAMION))) = @Codigo;";
            await using (var cmdDup = new SqlCommand(sqlDup, conn))
            {
                cmdDup.Parameters.AddWithValue("@Codigo", codigo);
                if (Convert.ToInt32(await cmdDup.ExecuteScalarAsync()) > 0)
                    return Conflict(RespuestaApi<object>.Error("El código del camión ya existe."));
            }
            string? nombrePlanta;
            const string sqlPlanta = "SELECT NOM_PLANTA FROM PLANTA WHERE ID_PLANTA = @IdPlanta;";
            await using (var cmdPlanta = new SqlCommand(sqlPlanta, conn))
            {
                cmdPlanta.Parameters.AddWithValue("@IdPlanta", nuevo.IdPlanta);
                nombrePlanta = (await cmdPlanta.ExecuteScalarAsync())?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nombrePlanta))
                    return BadRequest(RespuestaApi<object>.Error("La planta no existe."));
            }
            const string sqlInsert = @"
                INSERT INTO CAMION (COD_CAMION, ID_PLANTA, ID_ESTADO)
                VALUES (@Codigo, @IdPlanta, @IdEstado);";
            await using (var cmdInsert = new SqlCommand(sqlInsert, conn))
            {
                cmdInsert.Parameters.AddWithValue("@Codigo", codigo);
                cmdInsert.Parameters.AddWithValue("@IdPlanta", nuevo.IdPlanta);
                cmdInsert.Parameters.AddWithValue("@IdEstado", nuevo.IdEstado);
                await cmdInsert.ExecuteNonQueryAsync();
            }
            var creado = new Camion
            {
                CodigoCamion = codigo,
                IdPlanta = nuevo.IdPlanta,
                NombrePlanta = nombrePlanta,
                IdEstado = nuevo.IdEstado,
                NombreEstado = nuevo.IdEstado == Estado.Activo ? "HABILITADO" : "DESHABILITADO"
            };
            return Ok(RespuestaApi<Camion>.Exito(creado, "Camión registrado correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al registrar camión: {ex.Message}"));
        }
    }
    [HttpPut("estadoCamion/{codigo}")]
    public async Task<IActionResult> CambiarEstadoCamion(string codigo, [FromBody] Estado estado)
    {
        var codNorm = codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (estado.IdEstado != Estado.Activo && estado.IdEstado != Estado.Inactivo)
            return BadRequest(RespuestaApi<object>.Error("Estado no válido."));
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sql = "UPDATE CAMION SET ID_ESTADO = @IdEstado WHERE UPPER(LTRIM(RTRIM(COD_CAMION))) = @Codigo;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IdEstado", estado.IdEstado);
            cmd.Parameters.AddWithValue("@Codigo", codNorm);
            if (await cmd.ExecuteNonQueryAsync() == 0)
                return NotFound(RespuestaApi<object>.Error("El camión no existe."));
            var dato = new Camion
            {
                CodigoCamion = codNorm,
                IdEstado = estado.IdEstado,
                NombreEstado = estado.IdEstado == Estado.Activo ? "HABILITADO" : "DESHABILITADO"
            };
            return Ok(RespuestaApi<Camion>.Exito(dato, "Estado del camión actualizado."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al actualizar estado del camión: {ex.Message}"));
        }
    }
}