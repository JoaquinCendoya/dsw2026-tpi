using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class AvailabilityRequestValidator : AbstractValidator<AvailabilityModel.Request>
{
    private static readonly string[] ValidDays =
    [
        "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SABADO", "DOMINGO"
    ];

    public AvailabilityRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.DoctorId)
            .MustAsync(async (id, cancellation) =>
            {
                var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(id);
                return doctor is not null;
            })
            .WithMessage("El médico indicado no existe.");

        RuleFor(x => x.Days)
            .NotEmpty().WithMessage("Debe indicar al menos un día de atención.");

        RuleForEach(x => x.Days).ChildRules(day =>
        {
            day.RuleFor(d => d.Day)
                .Must(d => ValidDays.Contains(d.ToUpper()))
                .WithMessage("El día indicado no es válido.");

            day.RuleFor(d => d)
            .Must(d => d.StartTime < d.EndTime)
            .WithMessage("La hora de inicio debe ser anterior a la hora de fin.");
        });
    }
}