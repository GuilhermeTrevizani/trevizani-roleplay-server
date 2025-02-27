namespace TrevizaniRoleplay.Domain.Entities;

public class CrackDen : BaseEntity
{
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public uint Dimension { get; private set; }
    public int OnlinePoliceOfficers { get; private set; }
    public int CooldownQuantityLimit { get; private set; }
    public int CooldownHours { get; private set; }
    public DateTime CooldownDate { get; private set; } = DateTime.Now;
    public int Quantity { get; private set; }

    public void Create(float posX, float posY, float posZ, uint dimension, int onlinePoliceOfficers, int cooldownQuantityLimit, int cooldownHours)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
        OnlinePoliceOfficers = onlinePoliceOfficers;
        CooldownQuantityLimit = cooldownQuantityLimit;
        CooldownHours = cooldownHours;
    }

    public void Update(float posX, float posY, float posZ, uint dimension, int onlinePoliceOfficers, int cooldownQuantityLimit, int cooldownHours)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
        OnlinePoliceOfficers = onlinePoliceOfficers;
        CooldownQuantityLimit = cooldownQuantityLimit;
        CooldownHours = cooldownHours;
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
        if (Quantity >= CooldownQuantityLimit)
        {
            CooldownDate = DateTime.Now.AddHours(CooldownHours);
            Quantity = 0;
        }
    }

    public void ResetCooldownDate()
    {
        CooldownDate = DateTime.Now;
    }
}