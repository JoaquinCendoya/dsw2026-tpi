using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;

    public PatientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PatientModel.Response> GetByUserIdAsync(Guid userId)
    {
        var patients = await _unitOfWork.Repository<Patient>().FindAsync(p => p.UserId == userId);
        var patient = patients.FirstOrDefault()
            ?? throw new EntityNotFoundException(nameof(Patient));

        return patient.ToResponse();
    }
}
