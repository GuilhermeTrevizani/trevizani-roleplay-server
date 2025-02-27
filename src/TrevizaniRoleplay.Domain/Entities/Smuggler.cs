namespace TrevizaniRoleplay.Domain.Entities;

public class Smuggler : BaseEntity
{
    public uint Cellphone { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public uint Dimension { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float RotR { get; private set; }
    public float RotP { get; private set; }
    public float RotY { get; private set; }
    public string AllowedCharactersJSON { get; private set; } = "[]";
    public int Value { get; private set; }
    public int CooldownQuantityLimit { get; private set; }
    public int CooldownMinutes { get; private set; }
    public DateTime CooldownDate { get; private set; } = DateTime.Now;
    public int Quantity { get; private set; }

    public void Create(uint cellphone, string model, uint dimension, float posX, float posY, float posZ,
        float rotR, float rotP, float rotY, string allowedCharactersJSON, int value,
        int cooldownQuantityLimit, int cooldownMinutes)
    {
        Cellphone = cellphone;
        Model = model;
        Dimension = dimension;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
        AllowedCharactersJSON = allowedCharactersJSON;
        Value = value;
        CooldownQuantityLimit = cooldownQuantityLimit;
        CooldownMinutes = cooldownMinutes;
    }

    public void Update(string model, uint dimension, float posX, float posY, float posZ,
        float rotR, float rotP, float rotY, string allowedCharactersJSON, int value,
        int cooldownQuantityLimit, int cooldownMinutes)
    {
        Model = model;
        Dimension = dimension;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
        AllowedCharactersJSON = allowedCharactersJSON;
        Value = value;
        CooldownQuantityLimit = cooldownQuantityLimit;
        CooldownMinutes = cooldownMinutes;
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void SetCooldown()
    {
        CooldownDate = DateTime.Now.AddMinutes(CooldownMinutes);
        Quantity = 0;
    }
}