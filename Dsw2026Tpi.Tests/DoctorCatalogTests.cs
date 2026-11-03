using System.Reflection;
using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Services;
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
    public void CatalogReadsRequireAuthenticationAndWritesRequireAdministratorRole()
    {
        var doctorRead = typeof(DoctorController).GetMethod(nameof(DoctorController.GetAll))!;
        var specialtyRead = typeof(SpecialtyController).GetMethod(nameof(SpecialtyController.GetAll))!;
        var doctorCreate = typeof(DoctorController).GetMethod(nameof(DoctorController.Create))!;
        var specialtyCreate = typeof(SpecialtyController).GetMethod(nameof(SpecialtyController.Create))!;

        doctorRead.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().BeNull();
        specialtyRead.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().BeNull();
        doctorCreate.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be(Policies.AdminPolicy);
        specialtyCreate.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be(Policies.AdminPolicy);
    }
}
