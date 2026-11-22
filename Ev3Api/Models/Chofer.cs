
using System.ComponentModel.DataAnnotations;

namespace Ev3Api.Models
{
    public class Chofer
    {
        public int IdChofer { get; set; }

        [Required(ErrorMessage = "El nombre del chofer es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del chofer no puede superar los 50 caracteres.")]
        public string NombreChofer { get; set; } = string.Empty;

        [Range(1, 2, ErrorMessage = "Debe seleccionar un estado válido para el chofer.")]
        public int IdEstado { get; set; } = Estado.Activo;

        public string? NombreEstado { get; set; }

        public bool EstaActivo()
        {
            return IdEstado == Estado.Activo;
        }

        public bool TieneNombreValido()
        {
            return !string.IsNullOrWhiteSpace(NombreChofer)
                && NombreChofer.Trim().Length <= 50;
        }
    }
}
