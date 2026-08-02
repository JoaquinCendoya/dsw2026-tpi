using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Dsw2026Tpi.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly string _filePath;

    public HolidayService(IConfiguration configuration)
    {
        var relativePath = configuration["HolidaySettings:FilePath"] ?? Path.Combine("Sources", "feriados.json");
        _filePath = Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    public async Task<bool> IsHolidayAsync(DateTime date)
    {
        if (!File.Exists(_filePath))
        {
            throw new ConfigurationException(_filePath);
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