using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IDoctorService
{
    Task<Pagination<DoctorModel.Response>> GetAll(DoctorModel.SearchRequest request);
    Task<DoctorModel.Response> Create(DoctorModel.Request request);
    Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request);
    Task Delete(Guid id);
    Task<IEnumerable<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id);
}
