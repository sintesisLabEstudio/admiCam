
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ev3Api.Models
{
    public class Usuario
    {
        [Required]
        [StringLength(20)]
        public string UsuarioId { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Password { get; set; }
        [Range(1, 2)]
        public int IdEstado { get; set; } = Estado.Activo;
        public string? NombreEstado { get; set; }
        public bool EstaActivo()
        {
            return IdEstado == Estado.Activo;
        }
    }
}
