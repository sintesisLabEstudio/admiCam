
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace Ev3Api.Data
{
    public class ConexionBD
    {
        private readonly string _cadenaConexion;
        public ConexionBD(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
                ?? throw new InvalidOperationException("No se encontró la cadena ConexionSQL.");
        }
        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }
    }
}
