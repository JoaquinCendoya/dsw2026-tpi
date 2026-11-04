using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Mappings;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IUnitOfWork unitOfWork, ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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

        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(doctor.SpecialtyId)
        ?? throw new EntityNotFoundException(nameof(Specialty));

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
            _logger.LogWarning("Conflicto al reservar turno. SlotId: {SlotId}, Dni: {Dni}",
                request.AvailabilitySlotId, request.Patient.Dni);
            throw new ConflictException("El turno ya fue reservado por otro paciente.", "APPOINTMENT_CONFLICT");
        }
        await transaction.CommitAsync();

        _logger.LogInformation("Turno reservado. AppointmentId: {AppointmentId}, DoctorId: {DoctorId}, Dni: {Dni}",
            appointment.Id, request.DoctorId, request.Patient.Dni);

        return appointment.ToSearchResponse();
    }

    // VALIDACIÓN DE PACIENTE: Firma adaptada para verificar que el turno pertenezca al paciente autenticado
    public async Task CancelAsync(Guid id, long patientDni)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Appointment));

        // Cargar el paciente asociado para constatar identidad de forma segura
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(appointment.PatientId)
            ?? throw new EntityNotFoundException(nameof(Patient));

        if (patient.Dni != patientDni.ToString())
        {
            _logger.LogWarning("Intento de cancelación no autorizado. Turno: {AppointmentId}, Solicitante DNI: {Dni}", id, patientDni);
            throw new BusinessRuleException("El turno indicado no pertenece al paciente autenticado.", "UNAUTHORIZED_APPOINTMENT_ACCESS");
        }

        var availabilitySlot = await _unitOfWork.Repository<AvailabilitySlot>().GetByIdAsync(appointment.AvailabilitySlotId)
            ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

        appointment.Cancel();
        availabilitySlot.Release();

        _unitOfWork.Repository<Appointment>().Update(appointment);
        _unitOfWork.Repository<AvailabilitySlot>().Update(availabilitySlot);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Turno cancelado de forma segura por el paciente. AppointmentId: {AppointmentId}", id);
    }

    // VALIDACIÓN DE PACIENTE: Método paginado que consulta únicamente mediante el DNI seguro extraído del JWT
    public async Task<Pagination<AppointmentModel.SearchResponse>> GetByPatientDniAsync(AppointmentModel.PatientDto request,int pageSize = 10 , int pageIndex = 1)
    {
        string dniString = request.Dni.ToString();
        var patients = await _unitOfWork.Repository<Patient>().FindAsync(p => p.Dni == dniString);
        var patient = patients.FirstOrDefault()
                   ?? throw new EntityNotFoundException(nameof(Patient));

        // Trae los turnos paginados activos mapeando las relaciones necesarias requeridas por las tarjetas de la UI
        var appointments = await _unitOfWork.Repository<Appointment>().PaginateAsync(
            pageSize,
            pageIndex,
            a => a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked, // Filtrar solo activos según RN
            a => a.AvailabilitySlot.SlotDate,
            false, // Orden cronológico ascendente (los más próximos primero)
            "AvailabilitySlot.AvailabilityRule.Doctor.Specialty", "Patient");

        return appointments.Map(a => a.ToSearchResponse());
    }

    public async Task<Pagination<AppointmentModel.SearchResponse>> GetByDateAsync(AppointmentModel.DailyRequest request)
    {
        var appointments = await _unitOfWork.Repository<Appointment>()
            .PaginateAsync(request.PageSize, request.PageIndex, a => a.AvailabilitySlot.SlotDate == request.Date, a => a.AvailabilitySlot.StartTime,
                request.Descending, "AvailabilitySlot.AvailabilityRule.Doctor.Specialty", "Patient");

        return appointments.Map(a => a.ToSearchResponse());
    }


    // MÁS DATOS EN LA BÚSQUEDA: Filtros dinámicos robustos expandidos por Rango de Fechas y Estado (Enum)
    public async Task<Pagination<AppointmentModel.SearchResponse>> SearchAsync(AppointmentModel.SearchRequest request)
    {
        string? dniValue = request.Dni?.ToString();
        AppointmentStatus? statusFilter = request.Status;

        var appointments = await _unitOfWork.Repository<Appointment>().PaginateAsync(
            request.PageSize,
            request.PageIndex,
            a => (request.SpecialtyId == null || a.AvailabilitySlot.AvailabilityRule.Doctor.SpecialtyId == request.SpecialtyId) &&
                  (request.DoctorId == null || a.AvailabilitySlot.AvailabilityRule.DoctorId == request.DoctorId) &&
                  (dniValue == null || a.Patient.Dni == dniValue) &&
                  (statusFilter == null || a.Status == statusFilter.Value) &&
                  (request.Date == null || a.AvailabilitySlot.SlotDate == request.Date.Value) &&
                  (request.StartDate == null || a.AvailabilitySlot.SlotDate >= request.StartDate.Value) &&
                  (request.EndDate == null || a.AvailabilitySlot.SlotDate <= request.EndDate.Value),
                  a => a.AvailabilitySlot.SlotDate,
            request.Descending,
            "AvailabilitySlot.AvailabilityRule.Doctor.Specialty", "Patient");

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

        _logger.LogInformation("Turno actualizado. AppointmentId: {AppointmentId}, Estado: {Status}",
            id, attended ? "Asistido" : "Ausente");
    }
}
