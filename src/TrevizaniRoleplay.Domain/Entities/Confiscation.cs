using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Confiscation : BaseEntity
{
    public Guid? CharacterId { get; private set; }
    public Guid PoliceOfficerCharacterId { get; private set; }
    public Guid FactionId { get; private set; }
    public string? Description { get; private set; }
    public DateTime? DescriptionDate { get; private set; }
    public string Identifier { get; private set; } = string.Empty;

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerCharacter { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public ICollection<ConfiscationItem>? Items { get; private set; }

    public void Create(Guid? characterId, Guid policeOfficerCharacterid, Guid factionId, ICollection<ConfiscationItem> items, string identifier)
    {
        CharacterId = characterId;
        PoliceOfficerCharacterId = policeOfficerCharacterid;
        FactionId = factionId;
        Items = items;
        Identifier = identifier;
    }

    public void SetDescription(string description)
    {
        Description = description;
        DescriptionDate = DateTime.Now;
    }

    public void Update(Guid? characterId, Guid policeOfficerCharacterid, string identifier)
    {
        CharacterId = characterId;
        PoliceOfficerCharacterId = policeOfficerCharacterid;
        Identifier = identifier;
    }
}