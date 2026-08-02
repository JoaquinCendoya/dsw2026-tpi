using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHolidayService _holidayService;

    public AvailabilityService(IUnitOfWork unitOfWork, IHolidayService holidayService)
    {
        _unitOfWork = unitOfWork;
        _holidayService = holidayService;
    }

    public async Task<List<AvailabilityModel.Response>> GenerateMonthlyAvailabilityAsync(AvailabilityModel.Request request)
    {
        var resultList = new List<AvailabilityModel.Response>();
        var today = DateTime.Today;
        var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId);
        if (doctor == null)
        {
            throw new EntityNotFoundException("Doctor");
        }

        foreach (var dayConfig in request.Days)
        {
            if (!Enum.TryParse<DayOfWeek>(dayConfig.Day, true, out var dayOfWeek))
                continue;

            var ruleStartTime = TimeOnly.FromTimeSpan(dayConfig.StartTime);
            var ruleEndTime = TimeOnly.FromTimeSpan(dayConfig.EndTime);

            if (ruleStartTime >= ruleEndTime)
                throw new ArgumentException("El horario de inicio debe ser menor al de fin.");

            var rule = new AvailabilityRule(
                doctor,                 
                today.Month,
                today.Year,
                dayOfWeek,
                ruleStartTime,
                ruleEndTime
            );

            await _unitOfWork.Repository<AvailabilityRule>().AddAsync(rule);

            for (var date = today; date <= endOfMonth; date = date.AddDays(1))
            {
                if (date.DayOfWeek != dayOfWeek) continue;
                if (await _holidayService.IsHolidayAsync(date)) continue;

                var slotStartTime = ruleStartTime;

                while (slotStartTime < ruleEndTime)
                {
                    var slotEndTime = slotStartTime.AddMinutes(30);

                    var slotDateOnly = DateOnly.FromDateTime(date);

                    var slot = new AvailabilitySlot(
                        rule,          
                        slotDateOnly,
                        slotStartTime,
                        slotEndTime
                    );

                    await _unitOfWork.Repository<AvailabilitySlot>().AddAsync(slot);

                    resultList.Add(new AvailabilityModel.Response(date, slotStartTime.ToTimeSpan(), slotEndTime.ToTimeSpan()));

                    slotStartTime = slotEndTime;
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return resultList;
    }
}
