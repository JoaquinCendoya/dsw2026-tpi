using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Dsw2026Tpi.Domain.Enums;

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

        string dniString = request.Patient.Dni.ToString();
        var patients = await _unitOfWork.Repository<Patient>().FindAsync(p => p.Dni == dniString);

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

    public async Task CancelAsync(Guid id)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Appointment));
        var availabilitySlot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(appointment.AvailabilitySlotId)
            ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

        appointment.Cancel();
        availabilitySlot.Release();

        _unitOfWork.Repository<Appointment>().Update(appointment);
        _unitOfWork.Repository<AvailabilitySlot>().Update(availabilitySlot);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AppointmentModel.SearchResponse>> GetByPatientDniAsync(AppointmentModel.PatientDto request)
    {
        string dniString = request.Dni.ToString();
        var patients = await _unitOfWork.Repository<Patient>().FindAsync(p => p.Dni == dniString);
        var patient = patients.FirstOrDefault()
                   ?? throw new EntityNotFoundException(nameof(Patient));

        var appointments = await _unitOfWork.Repository<Appointment>()
            .FindAsync(a => a.PatientId == patient.Id && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Attended, "AvailabilitySlot.AvailabilityRule.Doctor.Speciality");

        return appointments.Select(a => a.ToSearchResponse());
    }

    public async Task<Pagination<AppointmentModel.SearchResponse>> GetByDateAsync(DateOnly date, int pageSize, int pageIndex)
    {
        var appointments = await _unitOfWork.Repository<Appointment>()
            .PaginateAsync(pageSize, pageIndex, a => a.AvailabilitySlot.SlotDate == date, a => a.AvailabilitySlot.StartTime, "AvailabilitySlot.AvailabilityRule.Doctor.Speciality");

        return appointments.Map(a => a.ToSearchResponse());
    }

    public async Task<Pagination<AppointmentModel.SearchResponse>> SearchAsync(int pageSize, int pageIndex, Guid? specialtyId, Guid? doctorId, AppointmentModel.PatientDto? dni, DateOnly? date)
    {
        string? dniValue = dni?.Dni.ToString();

        var appointments = await _unitOfWork.Repository<Appointment>().PaginateAsync(
            pageSize,
            pageIndex,
            a => (specialtyId == null || a.AvailabilitySlot.AvailabilityRule.Doctor.SpecialityId == specialtyId) &&
                  (doctorId == null || a.AvailabilitySlot.AvailabilityRule.DoctorId == doctorId) &&
                  (dniValue == null || a.Patient.Dni == dniValue) &&
                  (date == null || a.AvailabilitySlot.SlotDate == date),
                  a => a.AvailabilitySlot.SlotDate,
            "AvailabilitySlot.AvailabilityRule.Doctor.Speciality");

        return appointments.Map(a => a.ToSearchResponse());
    }

    public async Task MarkAttendanceAsync(Guid id, bool attended)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Appointment));

        if (attended)
        {
            appointment.MarkAttended();
        }
        else
        {
            appointment.MarkNoShow();
        }

        _unitOfWork.Repository<Appointment>().Update(appointment);
        await _unitOfWork.SaveChangesAsync();
    }
}
