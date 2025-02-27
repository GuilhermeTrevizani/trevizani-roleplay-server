using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Body : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public uint Model { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public uint Dimension { get; private set; }
    public string PlaceOfDeath { get; private set; } = string.Empty;
    public string PersonalizationJSON { get; private set; } = string.Empty;
    public string OutfitJSON { get; private set; } = string.Empty;
    public string WoundsJSON { get; private set; } = string.Empty;
    public DateTime? MorgueDate { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public ICollection<BodyItem>? Items { get; private set; }

    public void Create(Guid characterId, string name, uint model, float posX, float posY, float posZ, uint dimension,
        string placeOfDeath, string personalizationJSON, string outfitJSON, string woundsJSON, ICollection<BodyItem> items)
    {
        CharacterId = characterId;
        Name = name;
        Model = model;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
        PlaceOfDeath = placeOfDeath;
        PersonalizationJSON = personalizationJSON;
        OutfitJSON = outfitJSON;
        WoundsJSON = woundsJSON;
        Items = items;
    }

    public void SetMorgueDate(DateTime morgueDate)
    {
        if (!MorgueDate.HasValue)
            MorgueDate = morgueDate;
    }

    public void SetPosition(float posX, float posY, float posZ, uint dimension)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
    }
}