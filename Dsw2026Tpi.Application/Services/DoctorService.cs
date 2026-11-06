using Dsw2026Tpi.Application.Common;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;

    public DoctorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(DoctorModel.SearchRequest request)
    {
        var doctors = await _unitOfWork.Repository<Doctor>().PaginateAsync(
            request.PageSize,
            request.PageIndex,
            d => string.IsNullOrWhiteSpace(request.Name) || d.Name.Contains(request.Name),
            d => d.Name,
            request.Descending,
            request.IncludeInactive,
            nameof(Doctor.Specialty)
        );

        return doctors.Map(d => d.ToResponse());
    }
    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        var license = request.LicenseNumber.Trim();
        var existingWithLicense = (await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.LicenseNumber.ToLower() == license.ToLower()))
            .FirstOrDefault();

        if (existingWithLicense is not null)
        {
            throw new ConflictException("Ya existe un médico registrado con esa matrícula.", "DUPLICATE_LICENSE_NUMBER");
        }

        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        var doctor = new Doctor(request.Name, license, specialty);

        await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
        await _unitOfWork.SaveChangesAsync();

        return doctor.ToResponse();
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var license = request.LicenseNumber.Trim();
        var existingWithLicense = (await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.Id != id && d.LicenseNumber.ToLower() == license.ToLower()))
            .FirstOrDefault();

        if (existingWithLicense is not null)
        {
            throw new ConflictException("Ya existe otro médico registrado con esa matrícula.", "DUPLICATE_LICENSE_NUMBER");
        }

        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        doctor.UpdateProfile(request.Name, license, specialty);

        _unitOfWork.Repository<Doctor>().Update(doctor);
        await _unitOfWork.SaveChangesAsync();

        return doctor.ToResponse();
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id, includeDeleted: true)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var rules = (await _unitOfWork.Repository<AvailabilityRule>()
            .FindAsync(r => r.DoctorId == id, includeDeleted: true)).ToList();
        var ruleIds = rules.Select(r => r.Id).ToList();

        var slots = (await _unitOfWork.Repository<AvailabilitySlot>()
            .FindAsync(s => ruleIds.Contains(s.AvailabilityRuleId), includeDeleted: true)).ToList();
        var slotIds = slots.Select(s => s.Id).ToList();

        var appointments = (await _unitOfWork.Repository<Appointment>()
            .FindAsync(a => slotIds.Contains(a.AvailabilitySlotId), includeDeleted: true)).ToList();

        if (appointments.Any())
        {
            throw new ConflictException(
                "No se puede eliminar físicamente al doctor porque tiene turnos asociados. Puede desactivarlo en su lugar.",
                "DOCTOR_HAS_APPOINTMENTS"
            );
        }

        foreach (var slot in slots)
        {
            _unitOfWork.Repository<AvailabilitySlot>().HardDelete(slot);
        }

        foreach (var rule in rules)
        {
            _unitOfWork.Repository<AvailabilityRule>().HardDelete(rule);
        }

        _unitOfWork.Repository<Doctor>().HardDelete(doctor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DoctorModel.Response> SetStatus(Guid id, bool isActive)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id, includeDeleted: true)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        if (!isActive)
        {
            var rules = await _unitOfWork.Repository<AvailabilityRule>()
                .FindAsync(r => r.DoctorId == id);

            foreach (var rule in rules)
            {
                _unitOfWork.Repository<AvailabilityRule>().Delete(rule);
            }

            _unitOfWork.Repository<Doctor>().Delete(doctor);
            await _unitOfWork.SaveChangesAsync();

            return doctor.ToResponse();
        }

        var license = doctor.LicenseNumber.Trim();
        var existingActive = (await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.Id != id && d.LicenseNumber.ToLower() == license.ToLower()))
            .FirstOrDefault();

        if (existingActive != null)
        {
            throw new ConflictException("Ya existe otro médico activo con esa matrícula.", "DUPLICATE_LICENSE_NUMBER");
        }

        doctor.Restore();
        _unitOfWork.Repository<Doctor>().Update(doctor);
        await _unitOfWork.SaveChangesAsync();

        return doctor.ToResponse();
    }

    public async Task<IEnumerable<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var today = DateTime.Today;
        var rules = await _unitOfWork.Repository<AvailabilityRule>().FindAsync(
            r => r.DoctorId == id && r.Month == today.Month && r.Year == today.Year);

        return rules.Select(r => new DoctorModel.AvailabilityResponse(
            r.Id,
            SpanishDayOfWeek.ToString(r.DayOfWeek),
            r.StartTime.ToString("HH:mm"),
            r.EndTime.ToString("HH:mm")
        ));
    }
}
