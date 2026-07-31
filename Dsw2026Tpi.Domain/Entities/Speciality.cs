using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Domain.Entities;

public class Speciality: EntityBase
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Speciality() { }
#pragma warning restore CS8618
    #endregion

    public Speciality(string name, string description, Guid? id = null) : base(id)
    {
        UpdateDetails(name, description);
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("INVALID_SPECIALITY_NAME", "INVALID_SPECIALITY_NAME")
                .WithDetail("name", "required");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("INVALID_SPECIALITY_DESCRIPTION", "INVALID_SPECIALITY_DESCRIPTION")
                .WithDetail("description", "required");

        Name = name;
        Description = description;
    }
}
