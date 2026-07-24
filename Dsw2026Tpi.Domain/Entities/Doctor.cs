namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; init; }
    public string LicenseNumber { get; init; }
    public Guid SpecialityId { get; init; }
    public Speciality Speciality { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor() { }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Speciality speciality, Guid? id = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");

        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality ?? throw new ArgumentNullException(nameof(speciality));
        SpecialityId = speciality.Id;
    }
}
