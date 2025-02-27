using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionUniform : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string OutfitJSON { get; private set; } = string.Empty;
    public Guid FactionId { get; private set; }
    public CharacterSex Sex { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid factionId, string name, string outfitJSON, CharacterSex sex)
    {
        FactionId = factionId;
        Name = name;
        OutfitJSON = outfitJSON;
        Sex = sex;
    }
}