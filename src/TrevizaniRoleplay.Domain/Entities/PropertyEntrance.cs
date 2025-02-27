namespace TrevizaniRoleplay.Domain.Entities;

public class PropertyEntrance : BaseEntity
{
    public Guid PropertyId { get; private set; }
    public float EntrancePosX { get; private set; }
    public float EntrancePosY { get; private set; }
    public float EntrancePosZ { get; private set; }
    public float ExitPosX { get; private set; }
    public float ExitPosY { get; private set; }
    public float ExitPosZ { get; private set; }
    public float EntranceRotR { get; private set; }
    public float EntranceRotP { get; private set; }
    public float EntranceRotY { get; private set; }
    public float ExitRotR { get; private set; }
    public float ExitRotP { get; private set; }
    public float ExitRotY { get; private set; }

    public Property? Property { get; private set; }

    public void Create(Guid propertyId,
        float entrancePosX, float entrancePosY, float entrancePosZ,
        float exitPosX, float exitPosY, float exitPosZ,
        float entranceRotR, float entranceRotP, float entranceRotY,
        float exitRotR, float exitRotP, float exitRotY)
    {
        PropertyId = propertyId;
        EntrancePosX = entrancePosX;
        EntrancePosY = entrancePosY;
        EntrancePosZ = entrancePosZ;
        ExitPosX = exitPosX;
        ExitPosY = exitPosY;
        ExitPosZ = exitPosZ;
        EntranceRotR = entranceRotR;
        EntranceRotP = entranceRotP;
        EntranceRotY = entranceRotY;
        ExitRotR = exitRotR;
        ExitRotP = exitRotP;
        ExitRotY = exitRotY;
    }

    public void Update(float entrancePosX, float entrancePosY, float entrancePosZ,
        float exitPosX, float exitPosY, float exitPosZ,
        float entranceRotR, float entranceRotP, float entranceRotY,
        float exitRotR, float exitRotP, float exitRotY)
    {
        EntrancePosX = entrancePosX;
        EntrancePosY = entrancePosY;
        EntrancePosZ = entrancePosZ;
        ExitPosX = exitPosX;
        ExitPosY = exitPosY;
        ExitPosZ = exitPosZ;
        EntranceRotR = entranceRotR;
        EntranceRotP = entranceRotP;
        EntranceRotY = entranceRotY;
        ExitRotR = exitRotR;
        ExitRotP = exitRotP;
        ExitRotY = exitRotY;
    }
}