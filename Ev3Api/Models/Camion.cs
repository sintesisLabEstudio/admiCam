using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ev3Api.Models;

public class Camion
{
    [Required(ErrorMessage = "El campo COD_CAMION es obligatorio.")]
    [StringLength(10, ErrorMessage = "COD_CAMION no puede superar los 10 caracteres.")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "COD_CAMION solo permite letras y números sin espacios.")]
    [JsonPropertyName("COD_CAMION")]
    public string CodigoCamion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una planta válida.")]
    [JsonPropertyName("ID_PLANTA")]
    public int IdPlanta { get; set; }

    [Range(Estado.Activo, Estado.Inactivo, ErrorMessage = "Estado no válido.")]
    [JsonPropertyName("ID_ESTADO")]
    public int IdEstado { get; set; } = Estado.Activo;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_PLANTA")]
    public string? NombrePlanta { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_ESTADO")]
    public string? NombreEstado { get; set; }
}