using System.ComponentModel.DataAnnotations;

namespace Ev3Api.Models
{
    public class Planta
    {
        public int IdPlanta { get; set; }

        [Required]
        [StringLength(50)]
        public string NombrePlanta { get; set; } = string.Empty;

        public bool TieneNombreValido()
        {
            return !string.IsNullOrWhiteSpace(NombrePlanta)
                && NombrePlanta.Trim().Length <= 50;
        }
    }
}