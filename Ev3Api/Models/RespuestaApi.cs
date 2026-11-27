namespace Ev3Api.Models;
public class RespuestaApi<T>
{
    public bool Resultado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Dato { get; set; }
    public static RespuestaApi<T> Exito(T? dato, string mensaje = "Operacion exitosa.") =>
        new() { Resultado = true, Mensaje = mensaje, Dato = dato };
    public static RespuestaApi<T> Error(string mensaje, T? dato = default) =>
        new() { Resultado = false, Mensaje = mensaje, Dato = dato };
}