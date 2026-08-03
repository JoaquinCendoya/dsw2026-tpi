using Dsw2026Tpi.Application.Models;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IPatientService
{
    Task<PatientModel.Response> GetByUserIdAsync(Guid userId);
}
