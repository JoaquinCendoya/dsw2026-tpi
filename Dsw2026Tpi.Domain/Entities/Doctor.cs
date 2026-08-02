using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public Guid SpecialtyId { get; private set; }
    public Specialty Specialty { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor() { }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Specialty specialty, Guid? id = null) : base(id)
    {
        UpdateProfile(name, licenseNumber, specialty);
    }

    public void UpdateProfile(string name, string licenseNumber, Specialty specialty)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("INVALID_DOCTOR_NAME", "INVALID_DOCTOR_NAME")
                .WithDetail("name", "required");

        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ValidationException("INVALID_LICENSE_NUMBER", "INVALID_LICENSE_NUMBER")
                .WithDetail("licenseNumber", "required");

        if (specialty == null)
            throw new BusinessRuleException("INVALID_SPECIALTY", "INVALID_SPECIALTY")
                .WithDetail("specialty", "required");

        Name = name;
        LicenseNumber = licenseNumber;
        Specialty = specialty;
        SpecialtyId = specialty.Id;
    }
}
