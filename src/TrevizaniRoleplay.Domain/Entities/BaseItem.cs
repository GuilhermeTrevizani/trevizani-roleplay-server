using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public abstract class BaseItem : BaseEntity
{
    public Guid ItemTemplateId { get; private set; }
    public uint Subtype { get; internal set; }
    public int Quantity { get; private set; }
    public string? Extra { get; internal set; }

    [JsonIgnore]
    public ItemTemplate? ItemTemplate { get; private set; }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }

    public void Create(Guid itemTemplateId, uint subtype, int quantity, string? extra)
    {
        ItemTemplateId = itemTemplateId;
        Subtype = subtype;
        Quantity = quantity;
        Extra = extra;
    }
}