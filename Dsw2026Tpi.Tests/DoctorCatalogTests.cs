using System.Reflection;
using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Data.Repositories;
using Dsw2026Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Tests;

public class DoctorCatalogTests
{
    private static Dsw2026TpiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Dsw2026TpiDbContext(options);
    }

    [Fact]
    public async Task GetAll_FiltraPorEspecialidadYNombreAntesDePaginar()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);
        var cardiology = new Specialty("Cardiología", "Corazón");
        var pediatrics = new Specialty("Pediatría", "Infancias");
        await unitOfWork.Repository<Specialty>().AddAsync(cardiology);
        await unitOfWork.Repository<Specialty>().AddAsync(pediatrics);
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dra. Ana López", "MAT-1", cardiology));
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dr. Bruno Díaz", "MAT-2", cardiology));
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dra. Ana Ruiz", "MAT-3", pediatrics));
        await unitOfWork.SaveChangesAsync();
        var service = new DoctorService(unitOfWork);

        var result = await service.GetAll(new DoctorModel.SearchRequest(
            PageSize: 1,
            PageIndex: 1,
            Name: "Ana",
            SpecialtyId: cardiology.Id));

        result.Total.Should().Be(1);
        result.Data.Should().ContainSingle()
            .Which.Should().Match<DoctorModel.Response>(doctor => doctor.Name == "Dra. Ana López");
    }

    [Fact]
    public async Task GetAll_SinFiltrosConservaElListadoPaginadoOrdenado()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);
        var specialty = new Specialty("Clínica", "General");
        await unitOfWork.Repository<Specialty>().AddAsync(specialty);
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dra. Zoe", "MAT-3", specialty));
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dr. Abel", "MAT-1", specialty));
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dr. Luis", "MAT-2", specialty));
        await unitOfWork.SaveChangesAsync();
        var service = new DoctorService(unitOfWork);

        var result = await service.GetAll(new DoctorModel.SearchRequest(PageSize: 2, PageIndex: 1));

        result.Total.Should().Be(3);
        result.Data.Select(doctor => doctor.Name).Should().ContainInOrder("Dr. Abel", "Dr. Luis");
    }

    [Fact]
    public async Task GetAll_DevuelvePaginaVaciaCuandoNoHayCoincidencias()
    {
        await using var context = CreateContext();
        var service = new DoctorService(new UnitOfWork(context));

        var result = await service.GetAll(new DoctorModel.SearchRequest(
            PageSize: 10,
            PageIndex: 1,
            SpecialtyId: Guid.NewGuid()));

        result.Total.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOptions_DevuelveIdYNombreOrdenadosSoloParaLaEspecialidad()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);
        var cardiology = new Specialty("Cardiología", "Corazón");
        var pediatrics = new Specialty("Pediatría", "Infancias");
        await unitOfWork.Repository<Specialty>().AddAsync(cardiology);
        await unitOfWork.Repository<Specialty>().AddAsync(pediatrics);
        var doctorZoe = new Doctor("Dra. Zoe", "MAT-1", cardiology);
        var doctorAna = new Doctor("Dra. Ana", "MAT-2", cardiology);
        await unitOfWork.Repository<Doctor>().AddAsync(doctorZoe);
        await unitOfWork.Repository<Doctor>().AddAsync(doctorAna);
        await unitOfWork.Repository<Doctor>().AddAsync(new Doctor("Dr. Bruno", "MAT-3", pediatrics));
        await unitOfWork.SaveChangesAsync();
        var service = new DoctorService(unitOfWork);

        var result = (await service.GetOptions(cardiology.Id)).ToList();

        result.Should().HaveCount(2);
        result[0].Should().Be(new DoctorModel.OptionResponse(doctorAna.Id, "Dra. Ana"));
        result[1].Should().Be(new DoctorModel.OptionResponse(doctorZoe.Id, "Dra. Zoe"));
    }

    [Fact]
    public async Task GetAvailableSlots_DevuelveSoloSlotsLibresFuturosOrdenadosConSuIdPersistido()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);
        var specialty = new Specialty("Clínica", "General");
        var doctor = new Doctor("Dra. Ana", "MAT-1", specialty);
        var rule = new AvailabilityRule(doctor, DateTime.Today.AddDays(1).Month,
            DateTime.Today.AddDays(1).Year, DateTime.Today.AddDays(1).DayOfWeek,
            new TimeOnly(9, 0), new TimeOnly(11, 0));
        var firstSlot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(10, 0), new TimeOnly(10, 30));
        var secondSlot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(9, 30));
        var bookedSlot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 30), new TimeOnly(10, 0));
        bookedSlot.Book();

        await unitOfWork.Repository<Specialty>().AddAsync(specialty);
        await unitOfWork.Repository<Doctor>().AddAsync(doctor);
        await unitOfWork.Repository<AvailabilityRule>().AddAsync(rule);
        await unitOfWork.Repository<AvailabilitySlot>().AddAsync(firstSlot);
        await unitOfWork.Repository<AvailabilitySlot>().AddAsync(secondSlot);
        await unitOfWork.Repository<AvailabilitySlot>().AddAsync(bookedSlot);
        context.Entry(firstSlot).Property(slot => slot.RowVersion).CurrentValue = [];
        context.Entry(secondSlot).Property(slot => slot.RowVersion).CurrentValue = [];
        context.Entry(bookedSlot).Property(slot => slot.RowVersion).CurrentValue = [];
        await unitOfWork.SaveChangesAsync();

        var result = await new DoctorService(unitOfWork).GetAvailableSlotsAsync(
            doctor.Id,
            DateTime.Today,
            DateTime.Today.AddDays(2));

        result.Select(slot => slot.AvailabilitySlotId).Should().ContainInOrder(secondSlot.Id, firstSlot.Id);
        result.Should().HaveCount(2);
        result.Should().NotContain(slot => slot.AvailabilitySlotId == bookedSlot.Id);
    }

    [Fact]
    public async Task GetAvailableSlots_RechazaRangoInvalidoYMedicoInexistente()
    {
        await using var context = CreateContext();
        var service = new DoctorService(new UnitOfWork(context));

        var invalidRange = await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetAvailableSlotsAsync(
            Guid.NewGuid(), DateTime.Today.AddDays(2), DateTime.Today.AddDays(1)));
        invalidRange.Error.ErrorCode.Should().Be("INVALID_DATE_RANGE");

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetAvailableSlotsAsync(
            Guid.NewGuid(), DateTime.Today, DateTime.Today.AddDays(1)));
    }

    [Fact]
    public void CatalogReadsRequireAuthenticationAndWritesRequireAdministratorRole()
    {
        var doctorRead = typeof(DoctorController).GetMethod(nameof(DoctorController.GetAll))!;
        var specialtyRead = typeof(SpecialtyController).GetMethod(nameof(SpecialtyController.GetAll))!;
        var doctorOptionsRead = typeof(DoctorController).GetMethod(nameof(DoctorController.GetOptions))!;
        var doctorCreate = typeof(DoctorController).GetMethod(nameof(DoctorController.Create))!;
        var specialtyCreate = typeof(SpecialtyController).GetMethod(nameof(SpecialtyController.Create))!;

        doctorRead.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().BeNull();
        specialtyRead.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().BeNull();
        doctorOptionsRead.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().BeNull();
        doctorCreate.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be(Policies.AdminPolicy);
        specialtyCreate.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be(Policies.AdminPolicy);
    }
}
