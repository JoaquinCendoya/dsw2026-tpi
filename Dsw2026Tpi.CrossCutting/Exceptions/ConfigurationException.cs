using Dsw2026Tpi.CrossCutting.Resources;

namespace Dsw2026Tpi.CrossCutting.Exceptions;

/// <summary>
/// Excepción que se lanza cuando no se encuentra un archivo de configuración.
/// </summary>
public class ConfigurationException : AppException
{
    public ConfigurationException(string filePath)
        : base(string.Format(ErrorCodes.CONFIGURATION_FILE_NOT_FOUND, filePath),
               nameof(ErrorCodes.CONFIGURATION_FILE_NOT_FOUND))
    {
    }
}