using Dsw2026Tpi.Application.Models;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAvailabilityService
{
    Task<List<AvailabilityModel.Response>> GenerateMonthlyAvailabilityAsync(AvailabilityModel.Request request);
}
