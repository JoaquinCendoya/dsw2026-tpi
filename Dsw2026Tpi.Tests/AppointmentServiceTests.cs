using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace Dsw2026Tpi.Tests;

public class AppointmentServiceTests
{
    private readonly IUnitOfWork _unitOfWorkSubstitute;
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _unitOfWorkSubstitute = Substitute.For<IUnitOfWork>();
        _sut = new AppointmentService(_unitOfWorkSubstitute);
    }

    [Fact]
    public async Task BookAsync_CreaReservaYActualizaEstado_CuandoDatosSonValidos()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        var specialty = new Specialty("Cardiologia", "Descripción clinica", Guid.NewGuid());
        var doctor = new Doctor("Dr. Test", "12345", specialty, doctorId);

        var rule = new AvailabilityRule(doctor, DateTime.UtcNow.Month, DateTime.UtcNow.Year, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), ruleId);
        var slot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(10, 0), new TimeOnly(11, 0), slotId);

        var patient = new Patient(Guid.NewGuid(), "12345678", "Juan Perez", "555-1234", Guid.NewGuid());

        var slotRepo = Substitute.For<IRepository<AvailabilitySlot>>();
        var ruleRepo = Substitute.For<IRepository<AvailabilityRule>>();
        var docRepo = Substitute.For<IRepository<Doctor>>();
        var specRepo = Substitute.For<IRepository<Specialty>>();
        var patientRepo = Substitute.For<IRepository<Patient>>();
        var appointmentRepo = Substitute.For<IRepository<Appointment>>();

        slotRepo.GetByIdAsync(slotId).Returns(slot);
        ruleRepo.GetByIdAsync(ruleId).Returns(rule);
        docRepo.GetByIdAsync(doctorId).Returns(doctor);
        specRepo.GetByIdAsync(doctor.SpecialtyId).Returns(specialty);
        patientRepo.FindAsync(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(new List<Patient> { patient });

        _unitOfWorkSubstitute.Repository<AvailabilitySlot>().Returns(slotRepo);
        _unitOfWorkSubstitute.Repository<AvailabilityRule>().Returns(ruleRepo);
        _unitOfWorkSubstitute.Repository<Doctor>().Returns(docRepo);
        _unitOfWorkSubstitute.Repository<Specialty>().Returns(specRepo);
        _unitOfWorkSubstitute.Repository<Patient>().Returns(patientRepo);
        _unitOfWorkSubstitute.Repository<Appointment>().Returns(appointmentRepo);

        var transactionMock = Substitute.For<ITransaction>();
        _unitOfWorkSubstitute.BeginTransactionAsync().Returns(transactionMock);

        var request = new AppointmentModel.Request(doctorId, slotId, new AppointmentModel.PatientDto(12345678), "Chequeo general");

        var result = await _sut.BookAsync(request);

        result.Should().NotBeNull();
        slotRepo.Received(1).Update(Arg.Is<AvailabilitySlot>(s => s.Status == SlotStatus.Booked));
        await appointmentRepo.Received(1).AddAsync(Arg.Any<Appointment>());
        await _unitOfWorkSubstitute.Received(1).SaveChangesAsync();
        await transactionMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task BookAsync_LanzaValidationException_CuandoDoctorNoCoincideConElTurno()
    {
        var requestDoctorId = Guid.NewGuid();
        var realDoctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        var specialty = new Specialty("Cardiologia", "Descripción clinica", Guid.NewGuid());
        var realDoctor = new Doctor("Dr. Real", "12345", specialty, realDoctorId);

        var rule = new AvailabilityRule(realDoctor, DateTime.UtcNow.Month, DateTime.UtcNow.Year, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), ruleId);
        var slot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), new TimeOnly(10, 0), new TimeOnly(11, 0), slotId);

        var slotRepo = Substitute.For<IRepository<AvailabilitySlot>>();
        var ruleRepo = Substitute.For<IRepository<AvailabilityRule>>();

        slotRepo.GetByIdAsync(slotId).Returns(slot);
        ruleRepo.GetByIdAsync(ruleId).Returns(rule);

        _unitOfWorkSubstitute.Repository<AvailabilitySlot>().Returns(slotRepo);
        _unitOfWorkSubstitute.Repository<AvailabilityRule>().Returns(ruleRepo);

        var request = new AppointmentModel.Request(requestDoctorId, slotId, new AppointmentModel.PatientDto(12345678), "Chequeo medico");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.BookAsync(request));
        exception.Error.ErrorCode.Should().Be("DOCTOR_SLOT_MISMATCH");
    }

    [Fact]
    public async Task BookAsync_LanzaBusinessRuleException_CuandoTurnoEsEnElPasado()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        var specialty = new Specialty("Cardiologia", "Descripción clinica", Guid.NewGuid());
        var doctor = new Doctor("Dr. Test", "12345", specialty, doctorId);

        var rule = new AvailabilityRule(doctor, DateTime.UtcNow.Month, DateTime.UtcNow.Year, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), ruleId);
        var pastSlot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(10, 0), new TimeOnly(11, 0), slotId);

        var slotRepo = Substitute.For<IRepository<AvailabilitySlot>>();
        var ruleRepo = Substitute.For<IRepository<AvailabilityRule>>();

        slotRepo.GetByIdAsync(slotId).Returns(pastSlot);
        ruleRepo.GetByIdAsync(ruleId).Returns(rule);

        _unitOfWorkSubstitute.Repository<AvailabilitySlot>().Returns(slotRepo);
        _unitOfWorkSubstitute.Repository<AvailabilityRule>().Returns(ruleRepo);

        var request = new AppointmentModel.Request(doctorId, slotId, new AppointmentModel.PatientDto(12345678), "Chequeo medico");

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.BookAsync(request));
        exception.Error.ErrorCode.Should().Be("PAST_SLOT");
    }

    [Fact]
    public async Task BookAsync_LanzaConflictException_CuandoHayConcurrenciaDobleReserva()
    {
        var doctorId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();

        var specialty = new Specialty("Cardiologia", "Descripción clinica", Guid.NewGuid());
        var doctor = new Doctor("Dr. Test", "12345", specialty, doctorId);

        var rule = new AvailabilityRule(doctor, DateTime.UtcNow.Month, DateTime.UtcNow.Year, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), ruleId);
        var slot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(11, 0), slotId);

        var patient = new Patient(Guid.NewGuid(), "12345678", "Juan Perez", "555-1234", Guid.NewGuid());

        var slotRepo = Substitute.For<IRepository<AvailabilitySlot>>();
        var ruleRepo = Substitute.For<IRepository<AvailabilityRule>>();
        var docRepo = Substitute.For<IRepository<Doctor>>();
        var specRepo = Substitute.For<IRepository<Specialty>>();
        var patientRepo = Substitute.For<IRepository<Patient>>();
        var appointmentRepo = Substitute.For<IRepository<Appointment>>();

        slotRepo.GetByIdAsync(slotId).Returns(slot);
        ruleRepo.GetByIdAsync(ruleId).Returns(rule);
        docRepo.GetByIdAsync(doctorId).Returns(doctor);
        specRepo.GetByIdAsync(doctor.SpecialtyId).Returns(specialty);
        patientRepo.FindAsync(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(new List<Patient> { patient });

        _unitOfWorkSubstitute.Repository<AvailabilitySlot>().Returns(slotRepo);
        _unitOfWorkSubstitute.Repository<AvailabilityRule>().Returns(ruleRepo);
        _unitOfWorkSubstitute.Repository<Doctor>().Returns(docRepo);
        _unitOfWorkSubstitute.Repository<Specialty>().Returns(specRepo);
        _unitOfWorkSubstitute.Repository<Patient>().Returns(patientRepo);
        _unitOfWorkSubstitute.Repository<Appointment>().Returns(appointmentRepo);

        var transactionMock = Substitute.For<ITransaction>();
        _unitOfWorkSubstitute.BeginTransactionAsync().Returns(transactionMock);

        _unitOfWorkSubstitute.SaveChangesAsync().ThrowsAsync(new DbUpdateConcurrencyException());

        var request = new AppointmentModel.Request(doctorId, slotId, new AppointmentModel.PatientDto(12345678), "Chequeo medico");

        var exception = await Assert.ThrowsAsync<ConflictException>(() => _sut.BookAsync(request));
        exception.Error.ErrorCode.Should().Be(nameof(ErrorCodes.APPOINTMENT_CONFLICT));
    }

    [Fact]
    public async Task CancelAsync_LiberaTurnoYActualizaEstado_CuandoTransicionEsValida()
    {
        var appointmentId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var specialty = new Specialty("Cardiologia", "Descripción clinica", Guid.NewGuid());
        var doctor = new Doctor("Dr. Test", "12345", specialty, Guid.NewGuid());

        var rule = new AvailabilityRule(doctor, DateTime.UtcNow.Month, DateTime.UtcNow.Year, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), Guid.NewGuid());
        var slot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(11, 0), slotId);
        slot.Book();

        var patient = new Patient(Guid.NewGuid(), "12345678", "Juan Perez", "555-1234", Guid.NewGuid());
        var appointment = new Appointment(slot, patient, "Motivo de consulta", appointmentId);

        var appointmentRepo = Substitute.For<IRepository<Appointment>>();
        var slotRepo = Substitute.For<IRepository<AvailabilitySlot>>();

        appointmentRepo.GetByIdAsync(appointmentId).Returns(appointment);
        slotRepo.GetByIdAsync(slotId).Returns(slot);

        _unitOfWorkSubstitute.Repository<Appointment>().Returns(appointmentRepo);
        _unitOfWorkSubstitute.Repository<AvailabilitySlot>().Returns(slotRepo);

        await _sut.CancelAsync(appointmentId);

        slotRepo.Received(1).Update(Arg.Is<AvailabilitySlot>(s => s.Status == SlotStatus.Available));
        appointmentRepo.Received(1).Update(Arg.Is<Appointment>(a => a.Status == AppointmentStatus.Cancelled));
        await _unitOfWorkSubstitute.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CancelAsync_LanzaException_CuandoTurnoNoEstaEnEstadoValidoParaCancelar()
    {
        var appointmentId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        var specialty = new Specialty("Cardiología", "Descripción clínica", Guid.NewGuid());
        var doctor = new Doctor("Dr. Test", "12345", specialty, Guid.NewGuid());

        var rule = new AvailabilityRule(doctor, DateTime.UtcNow.Month, DateTime.UtcNow.Year, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), Guid.NewGuid());
        var slot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(11, 0), slotId);

        var patient = new Patient(Guid.NewGuid(), "12345678", "Juan Perez", "555-1234", Guid.NewGuid());
        var appointment = new Appointment(slot, patient, "Motivo de consulta", appointmentId);

        appointment.Cancel();

        var appointmentRepo = Substitute.For<IRepository<Appointment>>();
        var slotRepo = Substitute.For<IRepository<AvailabilitySlot>>();

        appointmentRepo.GetByIdAsync(appointmentId).Returns(appointment);
        slotRepo.GetByIdAsync(appointment.AvailabilitySlotId).Returns(slot);

        _unitOfWorkSubstitute.Repository<Appointment>().Returns(appointmentRepo);
        _unitOfWorkSubstitute.Repository<AvailabilitySlot>().Returns(slotRepo);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CancelAsync(appointmentId));
        exception.Error.ErrorCode.Should().Be("INVALID_APPOINTMENT_STATUS");
    }
}