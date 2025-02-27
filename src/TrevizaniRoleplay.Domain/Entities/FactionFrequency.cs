using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionFrequency : BaseEntity
{
    public Guid FactionId { get; private set; }
    public int Frequency { get; private set; }
    public string Name { get; private set; } = string.Empty;

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid factionId, int frequency, string name)
    {
        FactionId = factionId;
        Frequency = frequency;
        Name = name;
    }

    public void Update(int frequency, string name)
    {
        Frequency = frequency;
        Name = name;
    }
}