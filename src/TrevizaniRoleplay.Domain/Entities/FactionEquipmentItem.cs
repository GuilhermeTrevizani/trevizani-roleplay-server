using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionEquipmentItem : BaseEntity
{
    private FactionEquipmentItem()
    {
    }

    public FactionEquipmentItem(Guid factionEquipmentId, string weapon, uint ammo, string componentsJson)
    {
        FactionEquipmentId = factionEquipmentId;
        Weapon = weapon;
        Ammo = ammo;
        ComponentsJson = componentsJson;
    }

    public Guid FactionEquipmentId { get; private set; }
    public string Weapon { get; private set; } = default!;
    public uint Ammo { get; private set; }
    public string ComponentsJson { get; private set; } = default!;

    [JsonIgnore]
    public FactionEquipment? FactionEquipment { get; private set; }

    public void Update(string weapon, uint ammo, string componentsJson)
    {
        Weapon = weapon;
        Ammo = ammo;
        ComponentsJson = componentsJson;
    }
}