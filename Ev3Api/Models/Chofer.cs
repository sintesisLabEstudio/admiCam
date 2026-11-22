using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ev3Api.Models;

public class Chofer
{
    [JsonPropertyName("ID_CHOFER")]
    public int IdChofer { get; set; }

    [Required(ErrorMessage = "El campo NOM_CHOFER es obligatorio.")]
    [StringLength(50, ErrorMessage = "NOM_CHOFER no puede superar los 50 caracteres.")]
    [JsonPropertyName("NOM_CHOFER")]
    public string NombreChofer { get; set; } = string.Empty;

    [Range(Estado.Activo, Estado.Inactivo, ErrorMessage = "Estado no válido.")]
    [JsonPropertyName("ID_ESTADO")]
    public int IdEstado { get; set; } = Estado.Activo;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_ESTADO")]
    public string? NombreEstado { get; set; }
}