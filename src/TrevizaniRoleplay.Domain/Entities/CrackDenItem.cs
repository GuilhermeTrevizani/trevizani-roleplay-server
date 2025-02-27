using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class CrackDenItem : BaseEntity
{
    public Guid CrackDenId { get; private set; }
    public Guid ItemTemplateId { get; private set; }
    public int Value { get; private set; }

    [JsonIgnore]
    public CrackDen? CrackDen { get; private set; }

    [JsonIgnore]
    public ItemTemplate? ItemTemplate { get; private set; }

    public void Create(Guid crackDenId, Guid itemTemplateId, int value)
    {
        CrackDenId = crackDenId;
        ItemTemplateId = itemTemplateId;
        Value = value;
    }

    public void Update(Guid itemTemplateId, int value)
    {
        ItemTemplateId = itemTemplateId;
        Value = value;
    }
}