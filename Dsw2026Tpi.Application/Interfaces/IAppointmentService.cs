using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.SearchResponse> BookAsync(AppointmentModel.Request request);
    Task CancelAsync(Guid id);
    Task<IEnumerable<AppointmentModel.SearchResponse>> GetByPatientDniAsync(AppointmentModel.PatientDto request);
    Task<Pagination<AppointmentModel.SearchResponse>> GetByDateAsync(DateOnly date, int pageSize, int pageIndex);
    Task<Pagination<AppointmentModel.SearchResponse>> SearchAsync(Guid? specialtyId, Guid? doctorId, DateOnly? date, AppointmentModel.PatientDto? patientDni, int pageSize, int pageIndex);
    Task MarkAttendanceAsync(Guid id, bool attended);
}
