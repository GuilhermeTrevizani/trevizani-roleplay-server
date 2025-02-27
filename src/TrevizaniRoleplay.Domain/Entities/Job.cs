using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Job : BaseEntity
{
    public CharacterJob CharacterJob { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public int Salary { get; private set; }
    public uint BlipType { get; set; }
    public byte BlipColor { get; set; }
    public string BlipName { get; set; } = string.Empty;
    public string VehicleRentModel { get; private set; } = string.Empty;
    public int VehicleRentValue { get; private set; }
    public float VehicleRentPosX { get; private set; }
    public float VehicleRentPosY { get; private set; }
    public float VehicleRentPosZ { get; private set; }
    public float VehicleRentRotR { get; private set; }
    public float VehicleRentRotP { get; private set; }
    public float VehicleRentRotY { get; private set; }

    public void Create(CharacterJob characterJob)
    {
        CharacterJob = characterJob;
    }

    public void Update(float posX, float posY, float posZ, int salary,
        uint blipType, byte blipColor, string blipName,
        string vehicleRentModel, int vehicleRentValue,
        float vehicleRentPosX, float vehicleRentPosY, float vehicleRentPosZ,
        float vehicleRentRotR, float vehicleRentRotP, float vehicleRentRotY)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Salary = salary;
        BlipType = blipType;
        BlipColor = blipColor;
        BlipName = blipName;
        VehicleRentModel = vehicleRentModel;
        VehicleRentValue = vehicleRentValue;
        VehicleRentPosX = vehicleRentPosX;
        VehicleRentPosY = vehicleRentPosY;
        VehicleRentPosZ = vehicleRentPosZ;
        VehicleRentRotR = vehicleRentRotR;
        VehicleRentRotP = vehicleRentRotP;
        VehicleRentRotY = vehicleRentRotY;
    }
}