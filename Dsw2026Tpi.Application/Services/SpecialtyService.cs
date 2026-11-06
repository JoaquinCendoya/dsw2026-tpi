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
            request.Descending
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
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        // Borrado lógico en cascada: dar de baja a todos los médicos asignados y sus disponibilidades
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
    }
}