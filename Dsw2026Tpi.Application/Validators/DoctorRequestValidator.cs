using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;

public class DoctorRequestValidator : AbstractValidator<DoctorModel.Request>
{
    public DoctorRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(3, 100).WithMessage("El nombre debe tener entre 3 y 100 caracteres.");

        RuleFor(x => x.SpecialtyId)
            .MustAsync(async (id, cancellation) =>
            {
                var specialty = await unitOfWork.Repository<Specialty>().GetByIdAsync(id);
                return specialty is not null;
            })
            .WithMessage("La especialidad indicada no existe.");
    }
}