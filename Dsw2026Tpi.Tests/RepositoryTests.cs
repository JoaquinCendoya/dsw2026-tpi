using Dsw2026Tpi.Data.Repositories;
using Dsw2026Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Dsw2026Tpi.Data;

namespace Dsw2026Tpi.Tests;

public class RepositoryTests
{
    private Dsw2026TpiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new Dsw2026TpiDbContext(options);
    }

    [Fact]
    public async Task UnitOfWork_PersisteAgregadosRelacionados_EnUnaSolaTransaccion()
    {
        // Arrange: mismo DbContext compartido entre repos y UoW, como en DI real (scoped)
        var context = CreateContext();
        var specialtyRepo = new Repository<Specialty>(context);
        var doctorRepo = new Repository<Doctor>(context);
        var unitOfWork = new UnitOfWork(context);

        var specialty = new Specialty("Neurología", "Área del cerebro", Guid.NewGuid());
        var doctor = new Doctor("Dr. Juan Pérez", "MAT-12345", specialty);

        // Act: dos agregados relacionados, un solo commit
        await specialtyRepo.AddAsync(specialty);
        await doctorRepo.AddAsync(doctor);
        var affectedRows = await unitOfWork.SaveChangesAsync();

        // Assert: ambos persistidos, y la relación quedó correctamente enlazada
        affectedRows.Should().Be(2);

        var savedDoctor = await context.Set<Doctor>().FirstOrDefaultAsync();
        savedDoctor.Should().NotBeNull();
        savedDoctor!.SpecialtyId.Should().Be(specialty.Id);

        var savedSpecialty = await context.Set<Specialty>().FirstOrDefaultAsync();
        savedSpecialty.Should().NotBeNull();
        savedSpecialty!.Id.Should().Be(savedDoctor.SpecialtyId);
    }

    [Fact]
    public async Task Repository_NoPersisteNada_SiNoSeLlamaASaveChangesDelUnitOfWork()
    {
        // Arrange
        var context = CreateContext();
        var specialtyRepo = new Repository<Specialty>(context);

        // Act
        await specialtyRepo.AddAsync(new Specialty("Cardiología", "Área del corazón", Guid.NewGuid()));

        // Assert: trackeado en memoria, pero no commiteado (no se llamó SaveChangesAsync)
        context.ChangeTracker.Entries<Specialty>().Should().HaveCount(1);
        context.ChangeTracker.Entries<Specialty>().First().State.Should().Be(EntityState.Added);
        (await context.Set<Specialty>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UnitOfWork_Repository_DevuelveLaMismaInstancia_ParaElMismoTipo()
    {
        // Arrange
        var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);

        // Act
        var repo1 = unitOfWork.Repository<Specialty>();
        var repo2 = unitOfWork.Repository<Specialty>();

        // Assert: el cache interno del diccionario evita instanciar repos duplicados
        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public async Task Delete_AplicaSoftDelete_YQuedaExcluidoDeConsultas()
    {
        // Arrange
        var context = CreateContext();
        var unitOfWork = new UnitOfWork(context);
        var specialtyRepo = unitOfWork.Repository<Specialty>();

        var specialty = new Specialty("Dermatología", "Área de la piel", Guid.NewGuid());
        await specialtyRepo.AddAsync(specialty);
        await unitOfWork.SaveChangesAsync();

        // Act
        specialtyRepo.Delete(specialty);
        await unitOfWork.SaveChangesAsync();

        // Assert: sigue en la tabla físicamente (soft delete), pero el query filter global la excluye
        var raw = await context.Set<Specialty>().IgnoreQueryFilters().FirstOrDefaultAsync();
        raw.Should().NotBeNull();
        raw!.Deleted.Should().BeTrue();

        var visible = await specialtyRepo.GetAllAsync();
        visible.Should().NotContain(s => s.Id == specialty.Id);

        // Confirma que el query filter global también actúa sin pasar por el repo
        var directQuery = await context.Set<Specialty>().FirstOrDefaultAsync();
        directQuery.Should().BeNull();
    }
}
