namespace Dsw2026Tpi.Application.Interfaces;

public interface IHolidayService
{
    Task<bool> IsHolidayAsync(DateTime date);
}