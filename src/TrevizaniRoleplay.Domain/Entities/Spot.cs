using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Spot : BaseEntity
{
    public SpotType Type { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public uint Dimension { get; private set; }

    public void Create(SpotType type, float posX, float posY, float posZ, uint dimension)
    {
        Type = type;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
    }

    public void Update(SpotType type, float posX, float posY, float posZ, uint dimension)
    {
        Type = type;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
    }
}