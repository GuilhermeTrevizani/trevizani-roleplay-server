namespace TrevizaniRoleplay.Domain.Entities;

public class TruckerLocation : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public int DeliveryValue { get; private set; }
    public int LoadWaitTime { get; private set; }
    public int UnloadWaitTime { get; private set; }
    public string AllowedVehiclesJSON { get; private set; } = "[]";

    public void Create(string name, float posX, float posY, float posZ, int deliveryValue, int loadWaitTime, int unloadWaitTime,
        string allowedVehiclesJSON)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        DeliveryValue = deliveryValue;
        LoadWaitTime = loadWaitTime;
        UnloadWaitTime = unloadWaitTime;
        AllowedVehiclesJSON = allowedVehiclesJSON;
    }

    public void Update(string name, float posX, float posY, float posZ, int deliveryValue, int loadWaitTime, int unloadWaitTime,
        string allowedVehiclesJSON)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        DeliveryValue = deliveryValue;
        LoadWaitTime = loadWaitTime;
        UnloadWaitTime = unloadWaitTime;
        AllowedVehiclesJSON = allowedVehiclesJSON;
    }
}