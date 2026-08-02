using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
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

    public async Task<Pagination<SpecialtyModel.Response>> GetAll(int pageSize, int pageIndex, string name)
    {
        var specialties = await _unitOfWork.Repository<Specialty>().PaginateAsync(
            pageSize,
            pageIndex,
            s => string.IsNullOrWhiteSpace(name) || s.Name.Contains(name),
            s => s.Name
        );

        return specialties.Map(s => new SpecialtyModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialtyModel.Response> Create(SpecialtyModel.Request request)
    {
        var specialty = new Specialty(request.Name, request.Description);

        await _unitOfWork.Repository<Specialty>().AddAsync(specialty);
        await _unitOfWork.SaveChangesAsync();

        return new SpecialtyModel.Response(specialty.Id, specialty.Name, specialty.Description);
    }

    public async Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        specialty.UpdateDetails(request.Name, request.Description);

        _unitOfWork.Repository<Specialty>().Update(specialty);
        await _unitOfWork.SaveChangesAsync();

        return new SpecialtyModel.Response(specialty.Id, specialty.Name, specialty.Description);
    }

    public async Task Delete(Guid id)
    {
        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Specialty));

        _unitOfWork.Repository<Specialty>().Delete(specialty);
        await _unitOfWork.SaveChangesAsync();
    }
}