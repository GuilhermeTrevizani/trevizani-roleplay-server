namespace TrevizaniRoleplay.Domain.Entities;

public class Blip : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public ushort Type { get; private set; }
    public byte Color { get; private set; }

    public void Create(string name, float posX, float posY, float posZ, ushort type, byte color)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Type = type;
        Color = color;
    }

    public void Update(string name, float posX, float posY, float posZ, ushort type, byte color)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Type = type;
        Color = color;
    }
}