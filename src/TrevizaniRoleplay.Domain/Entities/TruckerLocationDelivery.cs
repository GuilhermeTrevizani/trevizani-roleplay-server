using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class TruckerLocationDelivery : BaseEntity
{
    public Guid TruckerLocationId { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }

    [JsonIgnore]
    public TruckerLocation? TruckerLocation { get; private set; }

    public void Create(Guid truckerLocationId, float posX, float posY, float posZ)
    {
        TruckerLocationId = truckerLocationId;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
    }

    public void Update(Guid truckerLocationId, float posX, float posY, float posZ)
    {
        TruckerLocationId = truckerLocationId;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
    }
}