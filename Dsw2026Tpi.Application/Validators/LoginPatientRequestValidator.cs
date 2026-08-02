using Dsw2026Tpi.Application.Dtos;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class LoginPatientRequestValidator : AbstractValidator<LoginPatientModel.Request>
{
    public LoginPatientRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato de email no es válido.");

        RuleFor(x => x.Dni)
            .Must(dni => dni.ToString().Length is 7 or 8)
            .WithMessage("El DNI debe tener 7 u 8 dígitos.");
    }
}