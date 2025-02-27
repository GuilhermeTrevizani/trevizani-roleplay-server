using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class PropertyFurniture : BaseEntity
{
    public Guid PropertyId { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float RotR { get; private set; }
    public float RotP { get; private set; }
    public float RotY { get; private set; }
    public bool Interior { get; private set; }
    public bool Locked { get; private set; } = true;

    [JsonIgnore]
    public Property? Property { get; private set; }

    public void Create(Guid propertyId, string model, bool interior)
    {
        PropertyId = propertyId;
        Model = model;
        Interior = interior;
    }

    public void SetPosition(float posX, float posY, float posZ, float rotR, float rotP, float rotY)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
    }

    public void SetLocked(bool locked)
    {
        Locked = locked;
    }
}