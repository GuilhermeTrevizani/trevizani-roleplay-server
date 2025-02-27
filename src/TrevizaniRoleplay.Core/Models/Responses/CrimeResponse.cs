namespace TrevizaniRoleplay.Core.Models.Responses;

public class CrimeResponse
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PrisonMinutes { get; set; }
    public int FineValue { get; set; }
    public int DriverLicensePoints { get; set; }
}