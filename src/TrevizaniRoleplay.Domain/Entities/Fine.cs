using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Fine : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid PoliceOfficerCharacterId { get; private set; }
    public int Value { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime? PaymentDate { get; private set; }
    public string? Description { get; private set; }
    public DateTime? DescriptionDate { get; private set; }
    public Guid FactionId { get; private set; }
    public int DriverLicensePoints { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerCharacter { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid characterId, Guid policeOfficerCharacterId, Guid factionId, string reason, int value, int driverLicensePoints)
    {
        CharacterId = characterId;
        PoliceOfficerCharacterId = policeOfficerCharacterId;
        FactionId = factionId;
        Reason = reason;
        Value = value;
        DriverLicensePoints = driverLicensePoints;
    }

    public void Pay()
    {
        PaymentDate = DateTime.Now;
    }

    public void SetDescription(string description)
    {
        Description = description;
        DescriptionDate = DateTime.Now;
    }
}