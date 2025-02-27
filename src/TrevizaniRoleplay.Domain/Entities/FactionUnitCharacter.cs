using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionUnitCharacter : BaseEntity
{
    public Guid FactionUnitId { get; private set; }
    public Guid CharacterId { get; private set; }

    [JsonIgnore]
    public FactionUnit? FactionUnit { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    public void Create(Guid characterId)
    {
        CharacterId = characterId;
    }
}