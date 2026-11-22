
using System.ComponentModel.DataAnnotations;

namespace Ev3Api.Models
{
    public class Salida
    {
        public int IdSalida { get; set; }
        [Required]
        public DateTime FechaSalida { get; set; }
        [Range(1, int.MaxValue)]
        public int IdPlanta { get; set; }
        public string? NombrePlanta { get; set; }
        [Required]
        [StringLength(10)]
        public string CodigoCamion { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int IdChofer { get; set; }
        public string? NombreChofer { get; set; }
        public int IdEstadoSalida { get; set; }
        public string? NombreEstadoSalida { get; set; }
        [Required]
        [StringLength(20)]
        public string Usuario { get; set; } = string.Empty;
        public string? NombreUsuario { get; set; }
        public bool TieneFechaValida()
        {
            return FechaSalida != default;
        }
        public DateTime ObtenerFechaSoloFecha()
        {
            return FechaSalida.Date;
        }
        public bool EstaEnRuta()
        {
            return IdEstadoSalida == Estado.EnRuta;
        }
        public bool PuedeRegistrarRetorno()
        {
            return EstaEnRuta();
        }
    }
}
