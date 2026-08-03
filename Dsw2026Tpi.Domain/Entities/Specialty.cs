using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Domain.Entities;

public class Specialty: EntityBase
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    #region Constructor for EF
#pragma warning disable CS8618
    private Specialty() { }
#pragma warning restore CS8618
    #endregion

    public Specialty(string name, string description, Guid? id = null) : base(id)
    {
        UpdateDetails(name, description);
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("INVALID_SPECIALTY_NAME", "INVALID_SPECIALTY_NAME")
                .WithDetail("name", "required");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("INVALID_SPECIALTY_DESCRIPTION", "INVALID_SPECIALTY_DESCRIPTION")
                .WithDetail("description", "required");

        Name = name;
        Description = description;
    }
}
