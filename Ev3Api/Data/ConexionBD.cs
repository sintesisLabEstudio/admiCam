using Microsoft.Data.SqlClient;
namespace Ev3Api.Data;

public class ConexionBD
{
    private readonly string _cadenaConexion;
    public ConexionBD(IConfiguration configuration)
    {
        _cadenaConexion = configuration.GetConnectionString("ConexionSQL")
            ?? throw new InvalidOperationException("No se encontro la cadena ConexionSQL.");
    }
    public SqlConnection ObtenerConexion() => new(_cadenaConexion);
}