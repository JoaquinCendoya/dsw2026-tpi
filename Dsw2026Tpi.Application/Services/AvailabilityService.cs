using Dsw2026Tpi.Application.Common;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHolidayService _holidayService;
    private readonly ILogger<AvailabilityService> _logger;

    public AvailabilityService(IUnitOfWork unitOfWork, IHolidayService holidayService, ILogger<AvailabilityService> logger)
    {
        _unitOfWork = unitOfWork;
        _holidayService = holidayService;
        _logger = logger;
    }

    public async Task<List<AvailabilityModel.Response>> GenerateMonthlyAvailabilityAsync(AvailabilityModel.Request request, bool overwrite = false)
    {
        var resultList = new List<AvailabilityModel.Response>();
        var today = DateTime.Today;
        var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId);
        if (doctor == null)
        {
            throw new EntityNotFoundException("Doctor");
        }

        var existingRules = await _unitOfWork.Repository<AvailabilityRule>()
            .FindAsync(r => r.DoctorId == request.DoctorId && r.Month == today.Month && r.Year == today.Year);

        var protectedRules = new List<AvailabilityRule>();

        foreach (var rule in existingRules)
        {
            var slots = await _unitOfWork.Repository<AvailabilitySlot>().FindAsync(s => s.AvailabilityRuleId == rule.Id);
            var hasBookedSlots = slots.Any(s => s.Status != SlotStatus.Available);

            if (!overwrite || hasBookedSlots)
            {
                protectedRules.Add(rule);
                continue;
            }

            foreach (var slot in slots)
            {
                _unitOfWork.Repository<AvailabilitySlot>().Delete(slot);
            }
            _unitOfWork.Repository<AvailabilityRule>().Delete(rule);
        }

        foreach (var dayConfig in request.Days)
        {
            if (!SpanishDayOfWeek.TryParse(dayConfig.Day, out var dayOfWeek))
                continue;

            var ruleStartTime = TimeOnly.FromTimeSpan(dayConfig.StartTime);
            var ruleEndTime = TimeOnly.FromTimeSpan(dayConfig.EndTime);

            if (ruleStartTime >= ruleEndTime)
                throw new ArgumentException("El horario de inicio debe ser menor al de fin.");

            var conflictingRule = protectedRules.FirstOrDefault(r =>
                r.DayOfWeek == dayOfWeek && ruleStartTime < r.EndTime && r.StartTime < ruleEndTime);

            if (conflictingRule is not null)
            {
                throw new BusinessRuleException(
                    $"El médico ya tiene disponibilidad configurada el día {dayOfWeek} entre {conflictingRule.StartTime.ToString("HH:mm")} y {conflictingRule.EndTime.ToString("HH:mm")}.",
                    "AVAILABILITY_OVERLAP");
            }

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

        _logger.LogInformation("Disponibilidad mensual generada. DoctorId: {DoctorId}, Turnos creados: {SlotCount}",
            request.DoctorId, resultList.Count);

        return resultList;
    }
}
