using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Jail : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid PoliceOfficerCharacterId { get; private set; }
    public Guid FactionId { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? DescriptionDate { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerCharacter { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid characterId, Guid policeOfficerCharacterId, Guid factionId, int minutes, string reason)
    {
        CharacterId = characterId;
        PoliceOfficerCharacterId = policeOfficerCharacterId;
        FactionId = factionId;
        Reason = reason;
        EndDate = DateTime.Now.AddMinutes(minutes);
    }

    public void SetDescription(string description)
    {
        Description = description;
        DescriptionDate = DateTime.Now;
    }
}