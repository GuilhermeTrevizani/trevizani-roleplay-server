using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionEquipmentItem : BaseItem
{
    public Guid FactionEquipmentId { get; private set; }

    [JsonIgnore]
    public FactionEquipment? FactionEquipment { get; private set; }

    public void SetFactionEquipmentId(Guid factionEquipmentId)
    {
        FactionEquipmentId = factionEquipmentId;
    }
}