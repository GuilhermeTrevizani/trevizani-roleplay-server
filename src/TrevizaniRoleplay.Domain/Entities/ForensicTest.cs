using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class ForensicTest : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid FactionId { get; private set; }
    public string Identifier { get; private set; } = string.Empty;

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public ICollection<ForensicTestItem>? Items { get; private set; }

    public void Create(Guid characterId, Guid factionId, string identifier,
        ICollection<ForensicTestItem> items)
    {
        CharacterId = characterId;
        FactionId = factionId;
        Identifier = identifier;
        Items = items;
    }
}