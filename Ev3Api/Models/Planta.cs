using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Ev3Api.Models;
public class Planta
{
    [JsonPropertyName("ID_PLANTA")]
    public int IdPlanta { get; set; }
    [Required(ErrorMessage = "El campo NOM_PLANTA es obligatorio.")]
    [StringLength(50, ErrorMessage = "NOM_PLANTA no puede superar los 50 caracteres.")]
    [JsonPropertyName("NOM_PLANTA")]
    public string NombrePlanta { get; set; } = string.Empty;
}