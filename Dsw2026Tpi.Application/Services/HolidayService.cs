using Dsw2026Tpi.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Dsw2026Tpi.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly string _filePath;

    public HolidayService(IConfiguration configuration)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        _filePath = configuration["HolidaySettings:FilePath"]
                    ?? Path.Combine(AppContext.BaseDirectory, "Sources", "feriados.json");

        if (!File.Exists(_filePath))
        {
            var projectRootPath = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\..\Dsw2026Tpi.Data\Sources\feriados.json"));
            if (File.Exists(projectRootPath))
            {
                _filePath = projectRootPath;
            }
        }
    }

    public async Task<bool> IsHolidayAsync(DateTime date)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"No se encontró el archivo de feriados requerido en la ruta: {_filePath}");
        }

        var jsonContent = await File.ReadAllTextAsync(_filePath);
        var holidayData = JsonSerializer.Deserialize<HolidayConfigDto>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (holidayData?.Feriados == null) return false;

        string targetDateString = date.ToString("dd/MM");

        return holidayData.Feriados.Contains(targetDateString);
    }
}

public class HolidayConfigDto
{
    public List<string> Feriados { get; set; } = new();
}