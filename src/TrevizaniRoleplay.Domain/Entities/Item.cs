namespace TrevizaniRoleplay.Domain.Entities;

public class Item : BaseItem
{
    public uint Dimension { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float RotR { get; private set; }
    public float RotP { get; private set; }
    public float RotY { get; private set; }

    public void SetPosition(uint dimension, float posX, float posY, float posZ, float rotR, float rotP, float rotY)
    {
        Dimension = dimension;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
    }
}