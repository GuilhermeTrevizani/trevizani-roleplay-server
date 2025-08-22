using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionEquipment : BaseEntity
{
    public Guid FactionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool PropertyOrVehicle { get; private set; }
    public bool SWAT { get; private set; }
    public bool UPR { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public ICollection<FactionEquipmentItem>? Items { get; private set; }

    public void Create(Guid factionId, string name, bool propertyOrVehicle, bool swat, bool upr)
    {
        FactionId = factionId;
        Name = name;
        PropertyOrVehicle = propertyOrVehicle;
        SWAT = swat;
        UPR = upr;
        Items = [];
    }

    public void Update(string name, bool propertyOrVehicle, bool swat, bool upr)
    {
        Name = name;
        PropertyOrVehicle = propertyOrVehicle;
        SWAT = swat;
        UPR = upr;
    }
}