using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class CrackDenSell : BaseEntity
{
    public Guid CrackDenId { get; private set; }
    public Guid CharacterId { get; private set; }
    public Guid ItemTemplateId { get; private set; }
    public int Quantity { get; private set; }
    public int Value { get; private set; }

    [JsonIgnore]
    public CrackDen? CrackDen { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public ItemTemplate? ItemTemplate { get; private set; }

    public void Create(Guid crackDenId, Guid characterId, Guid itemTemplateId, int quantity, int value)
    {
        CrackDenId = crackDenId;
        CharacterId = characterId;
        ItemTemplateId = itemTemplateId;
        Quantity = quantity;
        Value = value;
    }
}