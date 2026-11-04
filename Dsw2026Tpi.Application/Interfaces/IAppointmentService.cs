using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.SearchResponse> BookAsync(AppointmentModel.Request request);
    Task CancelAsync(Guid id, long dni);
    Task<Pagination<AppointmentModel.SearchResponse>> GetByPatientDniAsync(AppointmentModel.PatientDto request,int pageSize = 10 , int pageIndex = 1);
    Task<Pagination<AppointmentModel.SearchResponse>> GetByDateAsync(AppointmentModel.DailyRequest request);
    Task<Pagination<AppointmentModel.SearchResponse>> SearchAsync(AppointmentModel.SearchRequest request);
    Task MarkAttendanceAsync(Guid id, bool attended);
}
