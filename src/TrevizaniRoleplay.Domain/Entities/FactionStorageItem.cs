using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionStorageItem : BaseItem
{
    public Guid FactionStorageId { get; private set; }
    public int Price { get; private set; }

    [JsonIgnore]
    public FactionStorage? FactionStorage { get; private set; }

    public void SetFactionStorageIdAndPrice(Guid factionStorageId, int price)
    {
        FactionStorageId = factionStorageId;
        Price = price;
    }
}