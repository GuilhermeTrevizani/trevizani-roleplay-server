using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class ItemTemplate : BaseEntity
{
    public ItemCategory Category { get; private set; }
    public uint Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public uint Weight { get; private set; }
    public string Image { get; private set; } = string.Empty;
    public string ObjectModel { get; private set; } = string.Empty;

    public void Create(ItemCategory itemCategory, uint type, string name, uint weight, string image, string objectModel)
    {
        Category = itemCategory;
        Type = type;
        Name = name;
        Weight = weight;
        Image = image;
        ObjectModel = objectModel;
    }

    public void SetId(Guid id)
    {
        Id = id;
    }

    public void Update(uint type, string name, uint weight, string image, string objectModel)
    {
        Type = type;
        Name = name;
        Weight = weight;
        Image = image;
        ObjectModel = objectModel;
    }
}