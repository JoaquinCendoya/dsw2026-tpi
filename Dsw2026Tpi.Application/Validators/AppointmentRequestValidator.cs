using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class AppointmentRequestValidator : AbstractValidator<AppointmentModel.Request>
{
    public AppointmentRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.DoctorId)
            .MustAsync(async (id, cancellation) =>
            {
                var doctor = await unitOfWork.Repository<Doctor>().GetByIdAsync(id);
                return doctor is not null;
            })
            .WithMessage("El médico indicado no existe.");

        RuleFor(x => x.AvailabilityId)
            .NotEmpty().WithMessage("Debe indicar un horario disponible.");

        RuleFor(x => x.Patient.Dni)
            .Must(dni => dni.ToString().Length is >= 7 and <= 10)
            .WithMessage("El DNI debe tener entre 7 y 10 dígitos.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo es obligatorio.")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres.");
    }
}