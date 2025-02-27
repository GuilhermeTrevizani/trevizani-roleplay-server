using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionStorage : BaseEntity
{
    public Guid FactionId { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public uint Dimension { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid factionId, float posX, float posY, float posZ, uint dimension)
    {
        FactionId = factionId;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
    }

    public void Update(Guid factionId, float posX, float posY, float posZ, uint dimension)
    {
        FactionId = factionId;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
    }
}