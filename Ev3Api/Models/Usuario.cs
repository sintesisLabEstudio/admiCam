using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Ev3Api.Models;
public class Usuario
{
    [Required(ErrorMessage = "El campo USUARIO es obligatorio.")]
    [StringLength(20, ErrorMessage = "USUARIO no puede superar los 20 caracteres.")]
    [JsonPropertyName("USUARIO")]
    public string UsuarioId { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo NOM_PERSONA es obligatorio.")]
    [StringLength(50, ErrorMessage = "NOM_PERSONA no puede superar los 50 caracteres.")]
    [JsonPropertyName("NOM_PERSONA")]
    public string NombreCompleto { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo PASSWORD es obligatorio.")]
    [StringLength(20, ErrorMessage = "PASSWORD no puede superar los 20 caracteres.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("PASSWORD")]
    public string? Password { get; set; }
    [Range(Estado.Activo, Estado.Activo, ErrorMessage = "Todos los usuarios nuevos deben registrarse con estado ACTIVO.")]
    [JsonPropertyName("ID_ESTADO")]
    public int IdEstado { get; set; } = Estado.Activo;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_ESTADO")]
    public string? NombreEstado { get; set; }
}