using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionUnit : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid FactionId { get; private set; }
    public Guid CharacterId { get; private set; }
    public DateTime? FinalDate { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public string Status { get; private set; } = string.Empty;

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public ICollection<FactionUnitCharacter>? Characters { get; private set; }

    public void Create(string name, Guid factionId, Guid characterId,
        ICollection<FactionUnitCharacter> characters)
    {
        Name = name;
        FactionId = factionId;
        CharacterId = characterId;
        Characters = characters;
    }

    public void UpdatePosition(float x, float y)
    {
        PosX = x;
        PosY = y;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
    }

    public void End()
    {
        FinalDate = DateTime.Now;
    }
}