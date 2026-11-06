using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

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
            request.Descending,
            request.IncludeInactive
        );

        return specialties.Map(s => s.ToResponse());
    }

    public async Task<SpecialtyModel.Response> Create(SpecialtyModel.Request request)
    {
        var name = request.Name.Trim();
        var existingWithName = (await _unitOfWork.Repository<Specialty>()
            .FindAsync(s => s.Name.ToLower() == name.ToLower()))
            .FirstOrDefault();

        if (existingWithName is not null)
        {
            throw new ConflictException("Ya existe una especialidad con ese nombre.", "DUPLICATE_SPECIALTY_NAME");
        }

        var specialty = new Specialty(name, request.Description.Trim());

        await _unitOfWork.Repository<Specialty>().AddAsync(specialty);
        await _unitOfWork.SaveChangesAsync();

        return specialty.ToResponse();
    }

    public async Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        var name = request.Name.Trim();
        var existingWithName = (await _unitOfWork.Repository<Specialty>()
            .FindAsync(s => s.Id != id && s.Name.ToLower() == name.ToLower()))
            .FirstOrDefault();

        if (existingWithName is not null)
        {
            throw new ConflictException("Ya existe otra especialidad con ese nombre.", "DUPLICATE_SPECIALTY_NAME");
        }

        specialty.UpdateDetails(name, request.Description.Trim());

        _unitOfWork.Repository<Specialty>().Update(specialty);
        await _unitOfWork.SaveChangesAsync();

        return specialty.ToResponse();
    }

    public async Task Delete(Guid id)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id, includeDeleted: true)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        // Baja física en cascada: buscar todos los médicos asignados (incluso los dados de baja lógica)
        var doctors = (await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.SpecialtyId == id, includeDeleted: true)).ToList();

        var doctorIds = doctors.Select(d => d.Id).ToList();

        if (doctorIds.Any())
        {
            var rules = (await _unitOfWork.Repository<AvailabilityRule>()
                .FindAsync(r => doctorIds.Contains(r.DoctorId), includeDeleted: true)).ToList();
            var ruleIds = rules.Select(r => r.Id).ToList();

            var slots = (await _unitOfWork.Repository<AvailabilitySlot>()
                .FindAsync(s => ruleIds.Contains(s.AvailabilityRuleId), includeDeleted: true)).ToList();
            var slotIds = slots.Select(s => s.Id).ToList();

            var appointments = (await _unitOfWork.Repository<Appointment>()
                .FindAsync(a => slotIds.Contains(a.AvailabilitySlotId), includeDeleted: true)).ToList();

            if (appointments.Any())
            {
                throw new ConflictException(
                    "No se puede eliminar físicamente la especialidad porque contiene médicos con turnos registrados. Puede desactivarla desde el botón de estado.",
                    "SPECIALTY_HAS_APPOINTMENTS"
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

            foreach (var doctor in doctors)
            {
                _unitOfWork.Repository<Doctor>().HardDelete(doctor);
            }
        }

        _unitOfWork.Repository<Specialty>().HardDelete(specialty);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<SpecialtyModel.Response> SetStatus(Guid id, bool isActive)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id, includeDeleted: true)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        if (!isActive)
        {
            // Baja lógica en cascada
            var doctors = await _unitOfWork.Repository<Doctor>()
                .FindAsync(d => d.SpecialtyId == id);

            foreach (var doctor in doctors)
            {
                var rules = await _unitOfWork.Repository<AvailabilityRule>()
                    .FindAsync(r => r.DoctorId == doctor.Id);

                foreach (var rule in rules)
                {
                    _unitOfWork.Repository<AvailabilityRule>().Delete(rule);
                }

                _unitOfWork.Repository<Doctor>().Delete(doctor);
            }

            _unitOfWork.Repository<Specialty>().Delete(specialty);
            await _unitOfWork.SaveChangesAsync();

            return specialty.ToResponse();
        }

        var name = specialty.Name.Trim();
        var existingActive = (await _unitOfWork.Repository<Specialty>()
            .FindAsync(s => s.Id != id && s.Name.ToLower() == name.ToLower()))
            .FirstOrDefault();

        if (existingActive != null)
        {
            throw new ConflictException("Ya existe una especialidad activa con ese nombre.", "DUPLICATE_SPECIALTY_NAME");
        }

        specialty.Restore();
        _unitOfWork.Repository<Specialty>().Update(specialty);
        await _unitOfWork.SaveChangesAsync();

        return specialty.ToResponse();
    }
}