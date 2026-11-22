using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ev3Api.Data;
using Ev3Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Ev3Api.Controllers;

[ApiController]
[Route("api/Camiones")]
public class AuthController : ControllerBase
{
    private readonly ConexionBD _conexion;

    public AuthController(ConexionBD conexion)
    {
        _conexion = conexion;
    }

    [HttpPost("validaLogin")]
    public async Task<IActionResult> ValidaLogin([FromBody] CredencialesLogin credenciales)
    {
        var nombrePersona = credenciales.NomPersona.Trim();
        var password = credenciales.Password.Trim();

        if (string.IsNullOrWhiteSpace(nombrePersona) || string.IsNullOrWhiteSpace(password))
            return BadRequest(RespuestaApi<object>.Error("Debe ingresar NOM_PERSONA y PASSWORD."));

        try
        {
            await using var conn = _conexion.ObtenerConexion();
            await conn.OpenAsync();

            const string sql = @"
                SELECT TOP 1 USUARIO, NOM_PERSONA, PASSWORD, ID_ESTADO
                FROM USUARIOS
                WHERE UPPER(LTRIM(RTRIM(NOM_PERSONA))) = UPPER(@NomPersona)
                ORDER BY
                    CASE WHEN UPPER(LTRIM(RTRIM(USUARIO))) = 'ADMIN' THEN 0 ELSE 1 END,
                    CASE WHEN ID_ESTADO = 1 THEN 0 ELSE 1 END,
                    USUARIO;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NomPersona", nombrePersona);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return Unauthorized(RespuestaApi<object>.Error("La persona no existe."));

            var usuarioBd = reader["USUARIO"].ToString()?.Trim() ?? string.Empty;
            var nombreBd = reader["NOM_PERSONA"].ToString()?.Trim() ?? string.Empty;
            var passwordBd = reader["PASSWORD"].ToString()?.Trim() ?? string.Empty;
            var idEstado = Convert.ToInt32(reader["ID_ESTADO"]);

            if (!string.Equals(passwordBd, password, StringComparison.Ordinal))
                return Unauthorized(RespuestaApi<object>.Error("La contraseña es incorrecta."));

            if (idEstado != Estado.Activo)
                return Unauthorized(RespuestaApi<object>.Error("La persona se encuentra INACTIVA."));

            var dato = new Usuario
            {
                UsuarioId = usuarioBd,
                NombreCompleto = nombreBd,
                Password = null,
                IdEstado = idEstado,
                NombreEstado = idEstado == Estado.Activo ? "HABILITADO" : "DESHABILITADO"
            };

            return Ok(RespuestaApi<Usuario>.Exito(dato, "Acceso autorizado."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<object>.Error($"Error al validar acceso: {ex.Message}"));
        }
    }

    public class CredencialesLogin
    {
        [Required(ErrorMessage = "El campo NOM_PERSONA es obligatorio.")]
        [StringLength(50, ErrorMessage = "NOM_PERSONA no puede superar los 50 caracteres.")]
        [JsonPropertyName("NOM_PERSONA")]
        public string NomPersona { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo PASSWORD es obligatorio.")]
        [StringLength(20, ErrorMessage = "PASSWORD no puede superar los 20 caracteres.")]
        [JsonPropertyName("PASSWORD")]
        public string Password { get; set; } = string.Empty;
    }
}