using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string Model { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float RotR { get; private set; }
    public float RotP { get; private set; }
    public float RotY { get; private set; }
    public byte Color1R { get; private set; } = 255;
    public byte Color1G { get; private set; } = 255;
    public byte Color1B { get; private set; } = 255;
    public byte Color2R { get; private set; } = 255;
    public byte Color2G { get; private set; } = 255;
    public byte Color2B { get; private set; } = 255;
    public Guid? CharacterId { get; private set; }
    public string Plate { get; private set; } = string.Empty;
    public Guid? FactionId { get; private set; }
    public float EngineHealth { get; private set; } = 1000;
    public byte Livery { get; private set; } = 1;
    public int SeizedValue { get; private set; }
    public int Fuel { get; private set; }
    public bool Sold { get; private set; }
    public string DamagesJSON { get; private set; } = "[]";
    public float BodyHealth { get; private set; } = 1000;
    public uint LockNumber { get; private set; }
    public byte ProtectionLevel { get; private set; }
    public bool XMR { get; private set; }
    public string ModsJSON { get; private set; } = "[]";
    public byte WheelType { get; private set; }
    public byte WheelVariation { get; private set; }
    public byte WheelColor { get; private set; }
    public byte NeonColorR { get; private set; }
    public byte NeonColorG { get; private set; }
    public byte NeonColorB { get; private set; }
    public bool NeonLeft { get; private set; }
    public bool NeonRight { get; private set; }
    public bool NeonFront { get; private set; }
    public bool NeonBack { get; private set; }
    public byte HeadlightColor { get; private set; }
    public float LightsMultiplier { get; private set; } = 1;
    public byte WindowTint { get; private set; }
    public byte TireSmokeColorR { get; private set; }
    public byte TireSmokeColorG { get; private set; }
    public byte TireSmokeColorB { get; private set; }
    public bool SeizedDismantling { get; private set; }
    public DateTime? SeizedDate { get; private set; }
    public DateTime? InsuranceDate { get; private set; }
    public string ExtrasJSON { get; private set; } = "[true, true, true, true, true, true, true, true, true, true, true, true, true, true]";
    public string NewPlate { get; private set; } = string.Empty;
    public uint Dimension { get; private set; }
    public bool Drift { get; private set; }
    public string Description { get; private set; } = string.Empty;

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public ICollection<VehicleItem>? Items { get; private set; }

    [NotMapped]
    public bool ExemptInsurance => FactionId.HasValue;

    public void Create(string model, string plate, byte color1R, byte color1G, byte color1B, byte color2R, byte color2G, byte color2B)
    {
        Model = model;
        Plate = plate;
        Color1R = color1R;
        Color1G = color1G;
        Color1B = color1B;
        Color2R = color2R;
        Color2G = color2G;
        Color2B = color2B;
    }

    public void SetFuel(int fuel)
    {
        Fuel = fuel;
    }

    public void ChangePosition(float posX, float posY, float posZ, float rotR, float rotP, float rotY, uint dimension)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
        Dimension = dimension;
    }

    public void SetDamages(float engineHealth, float bodyHealth, string damagesJSON)
    {
        EngineHealth = engineHealth;
        BodyHealth = bodyHealth;
        DamagesJSON = damagesJSON;
    }

    public void SetSold()
    {
        Sold = true;
    }

    public void SetSeized(int value, bool dismantling, DateTime date)
    {
        SeizedValue = value;
        SeizedDismantling = dismantling;
        SeizedDate = date;
    }

    public void SetOwner(Guid characterId)
    {
        CharacterId = characterId;
    }

    public void SetLockNumber(uint value)
    {
        LockNumber = value;
    }

    public void SetPlate(string plate)
    {
        Plate = plate;
    }

    public void ResetSeized()
    {
        SeizedValue = 0;
    }

    public void SetFaction(Guid id)
    {
        FactionId = id;
    }

    public void SetColor(byte color1R, byte color1G, byte color1B, byte color2R, byte color2G, byte color2B)
    {
        Color1R = color1R;
        Color1G = color1G;
        Color1B = color1B;
        Color2R = color2R;
        Color2G = color2G;
        Color2B = color2B;
    }

    public void SetTunning(byte wheelType, byte wheelVariation, byte wheelColor,
        byte neonColorR, byte neonColorG, byte neonColorB,
        bool neonLeft, bool neonRight, bool neonFront, bool neonBack,
        byte headlightColor, float lightsMultiplier,
        byte windowTint, byte tireSmokeColorR, byte tireSmokeColorG, byte tireSmokeColorB,
        string modsJSON, byte protectionLevel, bool xmr, byte livery, string extrasJSON,
        bool drift)
    {
        WheelType = wheelType;
        WheelVariation = wheelVariation;
        WheelColor = wheelColor;
        NeonColorR = neonColorR;
        NeonColorG = neonColorG;
        NeonColorB = neonColorB;
        NeonLeft = neonLeft;
        NeonRight = neonRight;
        NeonFront = neonFront;
        NeonBack = neonBack;
        HeadlightColor = headlightColor;
        LightsMultiplier = lightsMultiplier;
        WindowTint = windowTint;
        TireSmokeColorR = tireSmokeColorR;
        TireSmokeColorG = tireSmokeColorG;
        TireSmokeColorB = tireSmokeColorB;
        ModsJSON = modsJSON;
        ProtectionLevel = protectionLevel;
        XMR = xmr;
        Livery = livery;
        ExtrasJSON = extrasJSON;
        Drift = drift;
    }

    public void SetInsuranceDate(DateTime insuranceDate)
    {
        InsuranceDate = insuranceDate;
    }

    public void SetNewPlate(string newPlate)
    {
        NewPlate = newPlate;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }
}