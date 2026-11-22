using System.Text.Json.Serialization;

namespace Ev3Api.Models;

public class Estado
{
    public const int Activo = 1;
    public const int Inactivo = 2;

    [JsonPropertyName("ID_ESTADO")]
    public int IdEstado { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("NOM_ESTADO")]
    public string? NombreEstado { get; set; }
}