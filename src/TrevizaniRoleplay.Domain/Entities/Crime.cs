namespace TrevizaniRoleplay.Domain.Entities;

public class Crime : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int PrisonMinutes { get; private set; }
    public int FineValue { get; private set; }
    public int DriverLicensePoints { get; private set; }

    public void Create(string name, int prisonMinutes, int fineValue, int driverLicensePoints)
    {
        Name = name;
        PrisonMinutes = prisonMinutes;
        FineValue = fineValue;
        DriverLicensePoints = driverLicensePoints;
    }

    public void Update(string name, int prisonMinutes, int fineValue, int driverLicensePoints)
    {
        Name = name;
        PrisonMinutes = prisonMinutes;
        FineValue = fineValue;
        DriverLicensePoints = driverLicensePoints;
    }
}