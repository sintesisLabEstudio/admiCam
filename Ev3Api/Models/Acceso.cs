
using System.ComponentModel.DataAnnotations;

namespace Ev3Api.Models
{
    public class Acceso
    {
        [Required]
        [StringLength(20)]
        public string Usuario { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string Password { get; set; } = string.Empty;
        public bool TieneCredenciales()
        {
            return !string.IsNullOrWhiteSpace(Usuario)
                && !string.IsNullOrWhiteSpace(Password);
        }
    }
}
