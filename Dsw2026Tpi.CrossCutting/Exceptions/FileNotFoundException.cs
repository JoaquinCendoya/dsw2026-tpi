namespace Dsw2026Tpi.CrossCutting.Exceptions;

/// <summary>
/// Excepción que se lanza cuando no se encuentra un archivo o recurso de configuración requerido.
/// </summary>
public class FileNotFoundException : AppException
{
    public FileNotFoundException(string message, string errorCode)
        : base(message, errorCode)
    {
    }
}
