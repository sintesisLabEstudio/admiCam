using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ev3Api.Models;

public class Salida
{
    [JsonPropertyName("ID_SALIDA")]
    public int IdSalida { get; set; }

    [Required(ErrorMessage = "El campo FECHA_SALIDA es obligatorio.")]
    [JsonPropertyName("FECHA_SALIDA")]
    public DateTime FechaSalida { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar una planta válida.")]
    [JsonPropertyName("ID_PLANTA")]
    public int IdPlanta { get; set; }

    [Required(ErrorMessage = "El campo COD_CAMION es obligatorio.")]
    [StringLength(10, ErrorMessage = "COD_CAMION no puede superar los 10 caracteres.")]
    [JsonPropertyName("COD_CAMION")]
    public string CodigoCamion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un chofer válido.")]
    [JsonPropertyName("ID_CHOFER")]
    public int IdChofer { get; set; }

    [JsonPropertyName("ID_ESTADO_SALIDA")]
    public int IdEstadoSalida { get; set; } = 1;

    [Required(ErrorMessage = "El campo USUARIO es obligatorio.")]
    [StringLength(20, ErrorMessage = "USUARIO no puede superar los 20 caracteres.")]
    [JsonPropertyName("USUARIO")]
    public string Usuario { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_PLANTA")]
    public string? NombrePlanta { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_CHOFER")]
    public string? NombreChofer { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_ESTADO_SALIDA")]
    public string? NombreEstadoSalida { get; set; }
}