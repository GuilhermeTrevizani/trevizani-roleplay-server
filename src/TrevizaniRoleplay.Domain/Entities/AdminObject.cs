namespace TrevizaniRoleplay.Domain.Entities;

public class AdminObject : BaseEntity
{
    public string Model { get; private set; } = string.Empty;
    public uint Dimension { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float RotR { get; private set; }
    public float RotP { get; private set; }
    public float RotY { get; private set; }

    public void Create(string model, uint dimension,
        float posX, float posY, float posZ,
        float rotR, float rotP, float rotY)
    {
        Model = model;
        Dimension = dimension;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
    }

    public void Update(string model, uint dimension,
        float posX, float posY, float posZ,
        float rotR, float rotP, float rotY)
    {
        Model = model;
        Dimension = dimension;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
    }
}