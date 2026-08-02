using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IUnitOfWork _unitOfWork;

    public SpecialityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string name)
    {
        var specialities = await _unitOfWork.Repository<Speciality>().PaginateAsync(
            pageSize,
            pageIndex,
            s => string.IsNullOrWhiteSpace(name) || s.Name.Contains(name),
            s => s.Name
        );

        return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
    {
        var speciality = new Speciality(request.Name, request.Description);

        await _unitOfWork.Repository<Speciality>().AddAsync(speciality);
        await _unitOfWork.SaveChangesAsync();

        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        var speciality = await _unitOfWork.Repository<Speciality>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        speciality.UpdateDetails(request.Name, request.Description);

        _unitOfWork.Repository<Speciality>().Update(speciality);
        await _unitOfWork.SaveChangesAsync();

        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _unitOfWork.Repository<Speciality>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        _unitOfWork.Repository<Speciality>().Delete(speciality);
        await _unitOfWork.SaveChangesAsync();
    }
}