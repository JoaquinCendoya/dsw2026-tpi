using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHolidayService _holidayService;

    public DoctorService(IUnitOfWork unitOfWork, IHolidayService holidayService)
    {
        _unitOfWork = unitOfWork;
        _holidayService = holidayService;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _unitOfWork.Repository<Doctor>().PaginateAsync(pageSize, pageIndex, d => string.IsNullOrWhiteSpace(name) ||
                                                   d.Name.Contains(name), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }
    public async Task<List<AvailabilityModel.Response>> GenerateMonthlyAvailabilityAsync(Guid doctorId, List<AvailabilityModel.Request> daysConfig)
    {
        var resultList = new List<AvailabilityModel.Response>();
        var today = DateTime.Today;
        var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        for (var date = today; date <= endOfMonth; date = date.AddDays(1))
        {
            if (await _holidayService.IsHolidayAsync(date))
            {
                continue;
            }

            string dayOfWeek = date.DayOfWeek.ToString();
            var configForDay = daysConfig.FirstOrDefault(d => d.Day.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase));

            if (configForDay != null)
            {
                var startTime = configForDay.StartTime;
                var endTime = configForDay.EndTime;

                while (startTime < endTime)
                {
                    var slotEndTime = startTime.Add(TimeSpan.FromMinutes(30));

                    resultList.Add(new AvailabilityModel.Response(date, startTime, slotEndTime));

                    startTime = slotEndTime;
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return resultList;
    }
}

