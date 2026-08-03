using Dsw2026Tpi.Application.Common;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class AvailabilityRequestValidator : AbstractValidator<AvailabilityModel.Request>
{
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
                .Must(SpanishDayOfWeek.IsValid)
                .WithMessage("El día indicado no es válido.");

            day.RuleFor(d => d)
            .Must(d => d.StartTime < d.EndTime)
            .WithMessage("La hora de inicio debe ser anterior a la hora de fin.");
        });

        RuleFor(x => x.Days)
            .Must(days => !HasOverlappingRanges(days))
            .WithMessage("Los horarios indicados para un mismo día no pueden solaparse.");
    }

    private static bool HasOverlappingRanges(List<AvailabilityModel.DayConfig> days)
    {
        var groupedByDay = days.GroupBy(d => d.Day.ToUpperInvariant());

        foreach (var group in groupedByDay)
        {
            var ranges = group.ToList();
            for (var i = 0; i < ranges.Count; i++)
            {
                for (var j = i + 1; j < ranges.Count; j++)
                {
                    if (ranges[i].StartTime < ranges[j].EndTime && ranges[j].StartTime < ranges[i].EndTime)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}