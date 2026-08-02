using Dsw2026Tpi.Application.Dtos;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class SpecialtyRequestValidator : AbstractValidator<SpecialtyModel.Request>
{
    public SpecialtyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(3, 100).WithMessage("El nombre debe tener entre 3 y 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .Length(10, 100).WithMessage("La descripción debe tener entre 10 y 100 caracteres.");
    }
}