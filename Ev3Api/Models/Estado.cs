
using System.ComponentModel.DataAnnotations;

namespace Ev3Api.Models
{
    public class Estado
    {
        public const int Activo = 1;
        public const int Inactivo = 2;
        public const int EnRuta = 1;
        public const int Retorno = 2;

        public int IdEstado { get; set; }

        [StringLength(30)]
        public string NombreEstado { get; set; } = string.Empty;

        public bool EsEstadoValido()
        {
            return IdEstado == Activo || IdEstado == Inactivo;
        }

        public bool EsActivo()
        {
            return IdEstado == Activo;
        }
    }
}
