using Dsw2026Tpi.Application.Models;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterModel.Request>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato de email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
    }
}
