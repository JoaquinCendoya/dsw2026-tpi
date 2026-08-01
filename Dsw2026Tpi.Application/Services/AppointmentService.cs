using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppointmentModel.SearchResponse> BookAsync(AppointmentModel.Request request)
    {
        var availabilitySlot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(request.AvailabilitySlotId)
            ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));
        var availabilityRule = await _unitOfWork.Repository<AvailabilityRule>().GetByIdAsync(availabilitySlot.AvailabilityRuleId)
            ?? throw new EntityNotFoundException(nameof(AvailabilityRule));
        if (availabilityRule.DoctorId != request.DoctorId)
        {
            throw new ValidationException("El horario disponible no pertenece al médico indicado.", "DOCTOR_SLOT_MISMATCH")
            .WithDetail("availabilitySlotId", "doctor_mismatch");
        }

        var slotDateTime = availabilitySlot.SlotDate.ToDateTime(availabilitySlot.StartTime);
        if (slotDateTime <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("No se puede reservar un turno en el pasado.", "PAST_SLOT");
        }

        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId)
                    ?? throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _unitOfWork.Repository<Speciality>().GetByIdAsync(doctor.SpecialityId)
        ?? throw new EntityNotFoundException(nameof(Speciality));

        var patients = await _unitOfWork.Repository<Patient>().FindAsync(p => p.Dni == request.Patient.Dni.ToString());

        var patient = patients.FirstOrDefault()
                   ?? throw new EntityNotFoundException(nameof(Patient));

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        availabilitySlot.Book();
        var appointment = new Appointment(availabilitySlot, patient, request.Reason);

        _unitOfWork.Repository<AvailabilitySlot>().Update(availabilitySlot);
        await _unitOfWork.Repository<Appointment>().AddAsync(appointment);

        try
        {

            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("El turno ya fue reservado por otro paciente.", "APPOINTMENT_CONFLICT");
        }
        await transaction.CommitAsync();

        return appointment.ToSearchResponse();
    }
}