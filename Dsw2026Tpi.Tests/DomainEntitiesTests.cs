using FluentAssertions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Tests.Domain;

public class DomainEntitiesTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Specialty_Constructor_RechazaNombresInvalidos(string nombreInvalido)
    {
        // Se espera que la entidad rechace estados inconsistentes de forma nativa
        Action action = () => new Specialty(nombreInvalido, "Descripción", Guid.NewGuid());
        action.Should().Throw<ValidationException>(); // O la excepción específica de su dominio
    }

    [Fact]
    public void AvailabilitySlot_Book_ModificaEstadoExitosamente()
    {
        var doctor = new Doctor("Test", "123", new Specialty("Cardio", "Desc", Guid.NewGuid()), Guid.NewGuid());
        var rule = new AvailabilityRule(doctor, 1, 2026, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), Guid.NewGuid());
        var slot = new AvailabilitySlot(rule, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(9, 0), new TimeOnly(9, 30), Guid.NewGuid());

        slot.Book();

        slot.Status.Should().Be(SlotStatus.Booked);
    }
}