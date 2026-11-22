using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Ev3Api.Controllers;

[ApiController]
[Route("api/Camiones")]
public class ChoferesController : ControllerBase
{
    private readonly ConexionBD _conexion;

    public ChoferesController(ConexionBD conexion)
    {
        _conexion = conexion;
    }

    [HttpGet("choferes")]
    public async Task<IActionResult> ListarChoferes()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sql = @"
                SELECT c.ID_CHOFER, c.NOM_CHOFER, c.ID_ESTADO, e.NOM_ESTADO
                FROM CHOFER c
                INNER JOIN ESTADO e ON c.ID_ESTADO = e.ID_ESTADO
                ORDER BY c.ID_CHOFER;";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var lista = new List<Chofer>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Chofer
                {
                    IdChofer = Convert.ToInt32(reader["ID_CHOFER"]),
                    NombreChofer = reader["NOM_CHOFER"].ToString()?.Trim() ?? string.Empty,
                    IdEstado = Convert.ToInt32(reader["ID_ESTADO"]),
                    NombreEstado = reader["NOM_ESTADO"].ToString()?.Trim()
                });
            }

            return Ok(RespuestaApi<List<Chofer>>.Exito(lista, "Choferes obtenidos correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar choferes: {ex.Message}"));
        }
    }

    [HttpGet("choferesDisponibles")]
    public async Task<IActionResult> ListarChoferesDisponibles()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sql = @"
                SELECT c.ID_CHOFER, c.NOM_CHOFER, c.ID_ESTADO, e.NOM_ESTADO
                FROM CHOFER c
                INNER JOIN ESTADO e ON c.ID_ESTADO = e.ID_ESTADO
                WHERE c.ID_ESTADO = @Activo
                  AND NOT EXISTS (
                      SELECT 1 FROM SALIDA s
                      WHERE s.ID_CHOFER = c.ID_CHOFER
                        AND s.ID_ESTADO_SALIDA = 1
                  )
                ORDER BY c.NOM_CHOFER;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Activo", Estado.Activo);
            await using var reader = await cmd.ExecuteReaderAsync();

            var lista = new List<Chofer>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Chofer
                {
                    IdChofer = Convert.ToInt32(reader["ID_CHOFER"]),
                    NombreChofer = reader["NOM_CHOFER"].ToString()?.Trim() ?? string.Empty,
                    IdEstado = Convert.ToInt32(reader["ID_ESTADO"]),
                    NombreEstado = reader["NOM_ESTADO"].ToString()?.Trim()
                });
            }

            return Ok(RespuestaApi<List<Chofer>>.Exito(lista, "Choferes disponibles obtenidos."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar choferes disponibles: {ex.Message}"));
        }
    }

    [HttpPost("agregarNuevoChofer")]
    public async Task<IActionResult> AgregarNuevoChofer([FromBody] Chofer nuevo)
    {
        var nombre = nuevo.NombreChofer.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 50)
            return BadRequest(RespuestaApi<object>.Error("El nombre del chofer es obligatorio y debe tener máximo 50 caracteres."));

        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sqlDup = "SELECT COUNT(1) FROM CHOFER WHERE UPPER(LTRIM(RTRIM(NOM_CHOFER))) = UPPER(@Nombre);";
            await using (var cmdDup = new SqlCommand(sqlDup, conn))
            {
                cmdDup.Parameters.AddWithValue("@Nombre", nombre);
                if (Convert.ToInt32(await cmdDup.ExecuteScalarAsync()) > 0)
                    return Conflict(RespuestaApi<object>.Error("Ya existe un chofer registrado con ese nombre."));
            }

            const string sqlInsert = @"
                INSERT INTO CHOFER (NOM_CHOFER, ID_ESTADO)
                OUTPUT INSERTED.ID_CHOFER
                VALUES (@Nombre, @IdEstado);";

            int nuevoId;
            await using (var cmdInsert = new SqlCommand(sqlInsert, conn))
            {
                cmdInsert.Parameters.AddWithValue("@Nombre", nombre);
                cmdInsert.Parameters.AddWithValue("@IdEstado", nuevo.IdEstado);
                nuevoId = Convert.ToInt32(await cmdInsert.ExecuteScalarAsync());
            }

            var creado = new Chofer
            {
                IdChofer = nuevoId,
                NombreChofer = nombre,
                IdEstado = nuevo.IdEstado,
                NombreEstado = nuevo.IdEstado == Estado.Activo ? "HABILITADO" : "DESHABILITADO"
            };

            return Ok(RespuestaApi<Chofer>.Exito(creado, "Chofer registrado correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al registrar chofer: {ex.Message}"));
        }
    }

    [HttpPut("estadoChofer/{idChofer:int}")]
    public async Task<IActionResult> CambiarEstadoChofer(int idChofer, [FromBody] Estado estado)
    {
        if (estado.IdEstado != Estado.Activo && estado.IdEstado != Estado.Inactivo)
            return BadRequest(RespuestaApi<object>.Error("Estado no válido."));

        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sql = "UPDATE CHOFER SET ID_ESTADO = @IdEstado WHERE ID_CHOFER = @IdChofer;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IdEstado", estado.IdEstado);
            cmd.Parameters.AddWithValue("@IdChofer", idChofer);

            if (await cmd.ExecuteNonQueryAsync() == 0)
                return NotFound(RespuestaApi<object>.Error("El chofer no existe."));

            var dato = new Chofer
            {
                IdChofer = idChofer,
                IdEstado = estado.IdEstado,
                NombreEstado = estado.IdEstado == Estado.Activo ? "HABILITADO" : "DESHABILITADO"
            };

            return Ok(RespuestaApi<Chofer>.Exito(dato, "Estado del chofer actualizado."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al actualizar estado del chofer: {ex.Message}"));
        }
    }
}