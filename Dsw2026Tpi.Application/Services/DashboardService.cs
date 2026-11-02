using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Application.Services
{
    public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
    {
        public async Task<DashboardSummaryModel> GetSummaryAsync(DateOnly date, CancellationToken cancellationToken)
        {
            if (date == default)
                throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR))
                    .WithDetail("date", "required");

            // Los repositorios ya excluyen Deleted; los awaits van en secuencia porque comparten el DbContext.
            var activeDoctors = await unitOfWork.Repository<Doctor>()
                .CountAsync(d => !d.Deleted, cancellationToken);

            var activeSpecialities = await unitOfWork.Repository<Specialty>()
                .CountAsync(s => !s.Deleted, cancellationToken); // solo especialidades activas, aunque el repositorio ya excluye Deleted, se deja explícito para mayor claridad.

            var confirmedAppointments = await unitOfWork.Repository<Appointment>()
                .CountAsync(a => a.Status == AppointmentStatus.Booked
                              && a.AvailabilitySlot.SlotDate == date,
                            cancellationToken);

            return new DashboardSummaryModel(activeDoctors, activeSpecialities, confirmedAppointments);
        }
    }
}
