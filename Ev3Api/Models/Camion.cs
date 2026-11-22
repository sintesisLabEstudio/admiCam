
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ev3Api.Models
{
    public class Camion
    {
        [Required(ErrorMessage = "El código del camión es obligatorio.")]
        [StringLength(10, MinimumLength = 1,
            ErrorMessage = "El código del camión debe tener máximo 10 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$",
            ErrorMessage = "El código del camión solo puede contener letras y números.")]
        public string CodigoCamion { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una planta válida.")]
        public int IdPlanta { get; set; }

        public string? NombrePlanta { get; set; }

        [Range(1, 2,
            ErrorMessage = "El estado del camión debe ser válido.")]
        public int IdEstado { get; set; } = Estado.Activo;

        public string? NombreEstado { get; set; }

        public string NormalizarCodigo()
        {
            return CodigoCamion.Trim().ToUpperInvariant();
        }

        public bool EstaActivo()
        {
            return IdEstado == Estado.Activo;
        }

        public bool TieneCodigoValido()
        {
            return !string.IsNullOrWhiteSpace(CodigoCamion)
                && CodigoCamion.Trim().Length <= 10
                && Regex.IsMatch(CodigoCamion.Trim(), @"^[a-zA-Z0-9]+$");
        }

        public bool EsEstadoValido()
        {
            return IdEstado == Estado.Activo
                || IdEstado == Estado.Inactivo;
        }
    }
}
