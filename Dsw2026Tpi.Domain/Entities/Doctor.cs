using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public Guid SpecialityId { get; private set; }
    public Speciality Speciality { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor() { }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Speciality speciality, Guid? id = null) : base(id)
    {
        UpdateProfile(name, licenseNumber, speciality);
    }

    public void UpdateProfile(string name, string licenseNumber, Speciality speciality)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("INVALID_DOCTOR_NAME", "INVALID_DOCTOR_NAME")
                .WithDetail("name", "required");

        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ValidationException("INVALID_LICENSE_NUMBER", "INVALID_LICENSE_NUMBER")
                .WithDetail("licenseNumber", "required");

        if (speciality == null)
            throw new BusinessRuleException("INVALID_SPECIALITY", "INVALID_SPECIALITY")
                .WithDetail("speciality", "required");

        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        SpecialityId = speciality.Id;
    }
}
