using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace Ev3Api.Controllers;
[ApiController]
[Route("api/Camiones")]
public class SalidasController : ControllerBase
{
    private readonly ConexionBD _conexion;
    public SalidasController(ConexionBD conexion)
    {
        _conexion = conexion;
    }
    [HttpGet("salidas")]
    public async Task<IActionResult> ListarSalidas()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sql = @"
                SELECT s.ID_SALIDA, s.FECHA_SALIDA, s.ID_PLANTA, p.NOM_PLANTA,
                       s.COD_CAMION, s.ID_CHOFER, c.NOM_CHOFER,
                       s.ID_ESTADO_SALIDA, es.NOM_ESTADO_SALIDA, s.USUARIO
                FROM SALIDA s
                INNER JOIN PLANTA p ON s.ID_PLANTA = p.ID_PLANTA
                INNER JOIN CHOFER c ON s.ID_CHOFER = c.ID_CHOFER
                INNER JOIN ESTADO_SALIDA es ON s.ID_ESTADO_SALIDA = es.ID_ESTADO_SALIDA
                ORDER BY s.ID_SALIDA DESC;";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<Salida>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Salida
                {
                    IdSalida = Convert.ToInt32(reader["ID_SALIDA"]),
                    FechaSalida = Convert.ToDateTime(reader["FECHA_SALIDA"]),
                    IdPlanta = Convert.ToInt32(reader["ID_PLANTA"]),
                    NombrePlanta = reader["NOM_PLANTA"].ToString()?.Trim(),
                    CodigoCamion = reader["COD_CAMION"].ToString()?.Trim() ?? string.Empty,
                    IdChofer = Convert.ToInt32(reader["ID_CHOFER"]),
                    NombreChofer = reader["NOM_CHOFER"].ToString()?.Trim(),
                    IdEstadoSalida = Convert.ToInt32(reader["ID_ESTADO_SALIDA"]),
                    NombreEstadoSalida = reader["NOM_ESTADO_SALIDA"].ToString()?.Trim(),
                    Usuario = reader["USUARIO"].ToString()?.Trim() ?? string.Empty
                });
            }
            return Ok(RespuestaApi<List<Salida>>.Exito(lista, "Salidas obtenidas correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar salidas: {ex.Message}"));
        }
    }
    [HttpPost("SalidaCamion")]
    public async Task<IActionResult> RegistrarSalida([FromBody] Salida salida)
    {
        if (salida.FechaSalida == default)
            return BadRequest(RespuestaApi<object>.Error("Debe indicar datos válidos para la salida."));
        var codCamion = salida.CodigoCamion.Trim().ToUpperInvariant();
        var usuario = salida.Usuario.Trim().ToUpperInvariant();
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sqlUser = "SELECT COUNT(1) FROM USUARIOS WHERE UPPER(LTRIM(RTRIM(USUARIO))) = @Usuario AND ID_ESTADO = @Activo;";
            await using (var cmdU = new SqlCommand(sqlUser, conn))
            {
                cmdU.Parameters.AddWithValue("@Usuario", usuario);
                cmdU.Parameters.AddWithValue("@Activo", Estado.Activo);
                if (Convert.ToInt32(await cmdU.ExecuteScalarAsync()) == 0)
                    return BadRequest(RespuestaApi<object>.Error("El usuario no existe o está INACTIVO."));
            }
            const string sqlCam = "SELECT COUNT(1) FROM CAMION WHERE UPPER(LTRIM(RTRIM(COD_CAMION))) = @Cod AND ID_PLANTA = @IdPlanta AND ID_ESTADO = @Activo;";
            await using (var cmdC = new SqlCommand(sqlCam, conn))
            {
                cmdC.Parameters.AddWithValue("@Cod", codCamion);
                cmdC.Parameters.AddWithValue("@IdPlanta", salida.IdPlanta);
                cmdC.Parameters.AddWithValue("@Activo", Estado.Activo);
                if (Convert.ToInt32(await cmdC.ExecuteScalarAsync()) == 0)
                    return BadRequest(RespuestaApi<object>.Error("El camión no existe, está INACTIVO o no pertenece a la planta."));
            }
            const string sqlChof = "SELECT COUNT(1) FROM CHOFER WHERE ID_CHOFER = @IdChofer AND ID_ESTADO = @Activo;";
            await using (var cmdCh = new SqlCommand(sqlChof, conn))
            {
                cmdCh.Parameters.AddWithValue("@IdChofer", salida.IdChofer);
                cmdCh.Parameters.AddWithValue("@Activo", Estado.Activo);
                if (Convert.ToInt32(await cmdCh.ExecuteScalarAsync()) == 0)
                    return BadRequest(RespuestaApi<object>.Error("El chofer no existe o está INACTIVO."));
            }
            const string sqlCamRuta = "SELECT COUNT(1) FROM SALIDA WHERE UPPER(LTRIM(RTRIM(COD_CAMION))) = @Cod AND ID_ESTADO_SALIDA = 1;";
            await using (var cmdCR = new SqlCommand(sqlCamRuta, conn))
            {
                cmdCR.Parameters.AddWithValue("@Cod", codCamion);
                if (Convert.ToInt32(await cmdCR.ExecuteScalarAsync()) > 0)
                    return Conflict(RespuestaApi<object>.Error("El camión ya tiene una salida EN RUTA."));
            }
            const string sqlChofRuta = "SELECT COUNT(1) FROM SALIDA WHERE ID_CHOFER = @IdChofer AND ID_ESTADO_SALIDA = 1;";
            await using (var cmdChR = new SqlCommand(sqlChofRuta, conn))
            {
                cmdChR.Parameters.AddWithValue("@IdChofer", salida.IdChofer);
                if (Convert.ToInt32(await cmdChR.ExecuteScalarAsync()) > 0)
                    return Conflict(RespuestaApi<object>.Error("El chofer ya tiene una salida EN RUTA."));
            }
            const string sqlInsert = @"
                INSERT INTO SALIDA (FECHA_SALIDA, ID_PLANTA, COD_CAMION, ID_CHOFER, ID_ESTADO_SALIDA, USUARIO)
                OUTPUT INSERTED.ID_SALIDA
                VALUES (@Fecha, @IdPlanta, @CodCamion, @IdChofer, 1, @Usuario);";
            int nuevoId;
            await using (var cmdIns = new SqlCommand(sqlInsert, conn))
            {
                cmdIns.Parameters.AddWithValue("@Fecha", salida.FechaSalida.Date);
                cmdIns.Parameters.AddWithValue("@IdPlanta", salida.IdPlanta);
                cmdIns.Parameters.AddWithValue("@CodCamion", codCamion);
                cmdIns.Parameters.AddWithValue("@IdChofer", salida.IdChofer);
                cmdIns.Parameters.AddWithValue("@Usuario", usuario);
                nuevoId = Convert.ToInt32(await cmdIns.ExecuteScalarAsync());
            }
            var creada = new Salida
            {
                IdSalida = nuevoId,
                FechaSalida = salida.FechaSalida.Date,
                IdPlanta = salida.IdPlanta,
                CodigoCamion = codCamion,
                IdChofer = salida.IdChofer,
                IdEstadoSalida = 1,
                NombreEstadoSalida = "EN RUTA",
                Usuario = usuario
            };
            return Ok(RespuestaApi<Salida>.Exito(creada, "Salida registrada correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al registrar salida: {ex.Message}"));
        }
    }
    [HttpPut("retornarCamion/{idSalida:int}")]
    public async Task<IActionResult> RetornarCamion(int idSalida)
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();
            const string sql = @"
                UPDATE SALIDA
                SET ID_ESTADO_SALIDA = 2
                WHERE ID_SALIDA = @IdSalida AND ID_ESTADO_SALIDA = 1;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IdSalida", idSalida);
            if (await cmd.ExecuteNonQueryAsync() == 0)
                return BadRequest(RespuestaApi<object>.Error("La salida no existe o no está EN RUTA."));
            var dato = new Salida
            {
                IdSalida = idSalida,
                IdEstadoSalida = 2,
                NombreEstadoSalida = "RETORNADO"
            };
            return Ok(RespuestaApi<Salida>.Exito(dato, "Retorno registrado correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al registrar retorno: {ex.Message}"));
        }
    }
}