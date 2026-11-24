using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Ev3Api.Controllers;

[ApiController]
[Route("api/Camiones")]
public class UsuariosController : ControllerBase
{
    private readonly ConexionBD _conexion;

    public UsuariosController(ConexionBD conexion)
    {
        _conexion = conexion;
    }

    [HttpGet("listarUsuarios")]
    public async Task<IActionResult> ListarUsuarios()
    {
        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            // CAMBIO (24-11-2026): se documenta por qué ADMIN no aparece en la lista.
            // Motivo: la app usa este listado para resolver USUARIO (ID) -> NOM_PERSONA,
            // pero ADMIN se resuelve con un atajo en LoginActivity, por eso se excluye.
            // No quitar el filtro: es el comportamiento esperado del requerimiento.
            const string sql = @"
                SELECT u.USUARIO, u.NOM_PERSONA, u.ID_ESTADO, e.NOM_ESTADO
                FROM USUARIOS u
                INNER JOIN ESTADO e ON u.ID_ESTADO = e.ID_ESTADO
                WHERE UPPER(LTRIM(RTRIM(u.USUARIO))) <> 'ADMIN'
                ORDER BY u.USUARIO;";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var lista = new List<Usuario>();
            while (await reader.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    UsuarioId = reader["USUARIO"].ToString()?.Trim() ?? string.Empty,
                    NombreCompleto = reader["NOM_PERSONA"].ToString()?.Trim() ?? string.Empty,
                    Password = null,
                    IdEstado = Convert.ToInt32(reader["ID_ESTADO"]),
                    NombreEstado = reader["NOM_ESTADO"].ToString()?.Trim()
                });
            }

            return Ok(RespuestaApi<List<Usuario>>.Exito(lista, "Usuarios obtenidos correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al listar usuarios: {ex.Message}"));
        }
    }

    [HttpPost("crearNuevoUsuario")]
    public async Task<IActionResult> CrearNuevoUsuario([FromBody] Usuario nuevo)
    {
        var usuarioId = nuevo.UsuarioId.Trim().ToUpperInvariant();
        var nombreCompleto = nuevo.NombreCompleto.Trim();
        var password = nuevo.Password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(usuarioId) ||
            string.IsNullOrWhiteSpace(nombreCompleto) ||
            string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(RespuestaApi<object>.Error("Todos los campos son obligatorios."));
        }

        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sqlExiste = @"
                SELECT COUNT(1)
                FROM USUARIOS
                WHERE UPPER(LTRIM(RTRIM(USUARIO))) = @Usuario;";

            await using (var cmdExiste = new SqlCommand(sqlExiste, conn))
            {
                cmdExiste.Parameters.AddWithValue("@Usuario", usuarioId);
                if (Convert.ToInt32(await cmdExiste.ExecuteScalarAsync()) > 0)
                    return Conflict(RespuestaApi<object>.Error("El usuario ya existe."));
            }

            const string sqlInsert = @"
                INSERT INTO USUARIOS (USUARIO, NOM_PERSONA, PASSWORD, ID_ESTADO)
                VALUES (@Usuario, @NombrePersona, @Password, @IdEstado);";

            await using (var cmdInsert = new SqlCommand(sqlInsert, conn))
            {
                cmdInsert.Parameters.AddWithValue("@Usuario", usuarioId);
                cmdInsert.Parameters.AddWithValue("@NombrePersona", nombreCompleto);
                cmdInsert.Parameters.AddWithValue("@Password", password);
                cmdInsert.Parameters.AddWithValue("@IdEstado", Estado.Activo);
                await cmdInsert.ExecuteNonQueryAsync();
            }

            var creado = new Usuario
            {
                UsuarioId = usuarioId,
                NombreCompleto = nombreCompleto,
                Password = null,
                IdEstado = Estado.Activo,
                NombreEstado = "HABILITADO"
            };

            return Ok(RespuestaApi<Usuario>.Exito(creado, "Usuario creado correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al crear usuario: {ex.Message}"));
        }
    }

    [HttpPut("estadoUsuario/{usuario}")]
    public async Task<IActionResult> CambiarEstadoUsuario(string usuario, [FromBody] Estado estado)
    {
        var usuarioNorm = usuario?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(usuarioNorm))
            return BadRequest(RespuestaApi<object>.Error("Debe indicar el usuario."));

        if (usuarioNorm == "ADMIN")
            return BadRequest(RespuestaApi<object>.Error("No se puede modificar el estado de ADMIN."));

        if (estado.IdEstado != Estado.Activo && estado.IdEstado != Estado.Inactivo)
            return BadRequest(RespuestaApi<object>.Error("Estado no válido."));

        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sqlUpdate = @"
                UPDATE USUARIOS
                SET ID_ESTADO = @IdEstado
                WHERE UPPER(LTRIM(RTRIM(USUARIO))) = @Usuario;";

            await using var cmd = new SqlCommand(sqlUpdate, conn);
            cmd.Parameters.AddWithValue("@IdEstado", estado.IdEstado);
            cmd.Parameters.AddWithValue("@Usuario", usuarioNorm);

            if (await cmd.ExecuteNonQueryAsync() == 0)
                return NotFound(RespuestaApi<object>.Error("El usuario no existe."));

            var dato = new Usuario
            {
                UsuarioId = usuarioNorm,
                IdEstado = estado.IdEstado,
                NombreEstado = estado.IdEstado == Estado.Activo ? "HABILITADO" : "DESHABILITADO"
            };

            return Ok(RespuestaApi<Usuario>.Exito(dato, "Estado del usuario actualizado."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al actualizar estado del usuario: {ex.Message}"));
        }
    }
}