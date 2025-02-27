namespace TrevizaniRoleplay.Domain.Entities;

public class Dealership : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float VehiclePosX { get; private set; }
    public float VehiclePosY { get; private set; }
    public float VehiclePosZ { get; private set; }
    public float VehicleRotR { get; private set; }
    public float VehicleRotP { get; private set; }
    public float VehicleRotY { get; private set; }

    public void Create(string name, float posX, float posY, float posZ,
        float vehiclePosX, float vehiclePosY, float vehiclePosZ,
        float vehicleRotR, float vehicleRotP, float vehicleRotY)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        VehiclePosX = vehiclePosX;
        VehiclePosY = vehiclePosY;
        VehiclePosZ = vehiclePosZ;
        VehicleRotR = vehicleRotR;
        VehicleRotP = vehicleRotP;
        VehicleRotY = vehicleRotY;
    }

    public void Update(string name, float posX, float posY, float posZ,
        float vehiclePosX, float vehiclePosY, float vehiclePosZ,
        float vehicleRotR, float vehicleRotP, float vehicleRotY)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        VehiclePosX = vehiclePosX;
        VehiclePosY = vehiclePosY;
        VehiclePosZ = vehiclePosZ;
        VehicleRotR = vehicleRotR;
        VehicleRotP = vehicleRotP;
        VehicleRotY = vehicleRotY;
    }
}