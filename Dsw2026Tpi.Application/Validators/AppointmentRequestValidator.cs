using Dsw2026Tpi.Application.Models;
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

        RuleFor(x => x.AvailabilitySlotId)
            .NotEmpty().WithMessage("Debe indicar un horario disponible.");

        RuleFor(x => x.Patient.Dni)
            .Must(dni => dni.ToString().Length is 7 or 8 or 9)
            .WithMessage("El DNI debe tener 7, 8 o 9 dígitos.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo es obligatorio.")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres.");
    }
}