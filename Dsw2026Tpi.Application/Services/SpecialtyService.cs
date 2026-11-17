using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.Domain.Enums;

namespace Dsw2026Tpi.Application.Services;

public class SpecialtyService : ISpecialtyService
{
    private readonly IUnitOfWork _unitOfWork;

    public SpecialtyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Pagination<SpecialtyModel.Response>> GetAll(SpecialtyModel.SearchRequest request)
    {
        var specialties = await _unitOfWork.Repository<Specialty>().PaginateAsync(
            request.PageSize,
            request.PageIndex,
            s => string.IsNullOrWhiteSpace(request.Name) || s.Name.Contains(request.Name),
            s => s.Name,
            request.Descending
        );

        return specialties.Map(s => s.ToResponse());
    }

    public async Task<SpecialtyModel.Response> Create(SpecialtyModel.Request request)
    {
        var specialty = new Specialty(request.Name, request.Description);

        await _unitOfWork.Repository<Specialty>().AddAsync(specialty);
        await _unitOfWork.SaveChangesAsync();

        return specialty.ToResponse();
    }

    public async Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        specialty.UpdateDetails(request.Name, request.Description);

        _unitOfWork.Repository<Specialty>().Update(specialty);
        await _unitOfWork.SaveChangesAsync();

        return specialty.ToResponse();
    }

    public async Task Delete(Guid id)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        _unitOfWork.Repository<Specialty>().Delete(specialty);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<SpecialtyModel.MetricsResponse> GetMetrics()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        int currentQuarter = (now.Month - 1) / 3 + 1;
        int quarterStartMonth = (currentQuarter - 1) * 3 + 1;
        var startOfQuarter = new DateTime(now.Year, quarterStartMonth, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeSpecialties = (await _unitOfWork.Repository<Specialty>()
            .FindAsync(s => !s.Deleted))
            .ToList();

        int totalSpecialties = activeSpecialties.Count;
        int quarterlyGrowthCount = activeSpecialties.Count(s => s.CreatedAt >= startOfQuarter);

        var newThisMonthNames = activeSpecialties
            .Where(s => s.CreatedAt >= startOfMonth)
            .OrderBy(s => s.Name)
            .Select(s => s.Name)
            .ToList();

        var appointments = (await _unitOfWork.Repository<Appointment>()
            .FindAsync(
                a => !a.Deleted && a.Status != AppointmentStatus.Cancelled,
                "AvailabilitySlot.AvailabilityRule.Doctor.Specialty"))
            .ToList();

        var validAppointments = appointments
            .Where(a => a.AvailabilitySlot?.AvailabilityRule?.Doctor?.Specialty != null
                        && !a.AvailabilitySlot.AvailabilityRule.Doctor.Specialty.Deleted)
            .ToList();

        int totalValidAppointments = validAppointments.Count;

        var topSpecialty = validAppointments
            .GroupBy(a => new
            {
                a.AvailabilitySlot!.AvailabilityRule!.Doctor!.Specialty!.Id,
                a.AvailabilitySlot.AvailabilityRule.Doctor.Specialty.Name
            })
            .Select(group => new
            {
                group.Key.Name,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name)
            .FirstOrDefault();

        string? mostDemandedName = topSpecialty?.Name;
        int mostDemandedCount = topSpecialty?.Count ?? 0;
        double mostDemandedPercentage = totalValidAppointments > 0
            ? Math.Round((double)mostDemandedCount * 100 / totalValidAppointments, 1)
            : 0;

        return new SpecialtyModel.MetricsResponse(
            totalSpecialties,
            quarterlyGrowthCount,
            mostDemandedName,
            mostDemandedCount,
            mostDemandedPercentage,
            newThisMonthNames.Count,
            newThisMonthNames);
    }
}