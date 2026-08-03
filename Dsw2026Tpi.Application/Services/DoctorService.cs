using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
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
            nameof(Doctor.Specialty)
        );

        return doctors.Map(d => d.ToResponse());
    }
    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        var doctor = new Doctor(request.Name, request.LicenseNumber, specialty);

        await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
        await _unitOfWork.SaveChangesAsync();

        return doctor.ToResponse();
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        doctor.UpdateProfile(request.Name, request.LicenseNumber, specialty);

        _unitOfWork.Repository<Doctor>().Update(doctor);
        await _unitOfWork.SaveChangesAsync();

        return doctor.ToResponse();
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        _unitOfWork.Repository<Doctor>().Delete(doctor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        var today = DateOnly.FromDateTime(DateTime.Today);

        var slots = await _unitOfWork.Repository<AvailabilitySlot>().FindAsync(
            s => s.AvailabilityRule.DoctorId == id &&
                 s.SlotDate >= today &&
                 s.Status == SlotStatus.Available);

        return slots
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .Select(s => s.ToResponse());
    }
}
