using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Property : BaseEntity
{
    public PropertyInterior Interior { get; private set; }
    public int Value { get; private set; }
    public Guid? CharacterId { get; private set; }
    public float EntrancePosX { get; private set; }
    public float EntrancePosY { get; private set; }
    public float EntrancePosZ { get; private set; }
    public uint EntranceDimension { get; private set; }
    public float ExitPosX { get; private set; }
    public float ExitPosY { get; private set; }
    public float ExitPosZ { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public uint Number { get; private set; }
    public uint LockNumber { get; private set; }
    public bool Locked { get; private set; }
    public byte ProtectionLevel { get; private set; }
    public int RobberyValue { get; private set; }
    public DateTime? RobberyCooldown { get; private set; }
    public Guid? FactionId { get; private set; }
    public string? Name { get; private set; }
    public Guid? ParentPropertyId { get; private set; }
    public float EntranceRotR { get; private set; }
    public float EntranceRotP { get; private set; }
    public float EntranceRotY { get; private set; }
    public float ExitRotR { get; private set; }
    public float ExitRotP { get; private set; }
    public float ExitRotY { get; private set; }
    public Guid? CompanyId { get; private set; }
    public byte? Time { get; private set; }
    public byte? Weather { get; private set; }
    public DateTime? PurchaseDate { get; private set; }

    [NotMapped]
    public string FormatedAddress => string.IsNullOrWhiteSpace(Name) ? $"{Number} {Address}" : Name;

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public ICollection<PropertyFurniture>? Furnitures { get; set; }

    [JsonIgnore]
    public ICollection<PropertyItem>? Items { get; set; }

    [JsonIgnore]
    public ICollection<PropertyEntrance>? Entrances { get; set; }

    [JsonIgnore]
    public Property? ParentProperty { get; private set; }

    [JsonIgnore]
    public Company? Company { get; private set; }

    public void Create(uint lockNumber, PropertyInterior interior, float entrancePosX, float entrancePosY, float entrancePosZ, uint entranceDimension,
        int value, float exitPosX, float exitPosY, float exitPosZ, string address, uint number, Guid? factionId, string? name, Guid? parentPropertyId,
        float entranceRotR, float entranceRotP, float entranceRotY,
        float exitRotR, float exitRotP, float exitRotY, Guid? companyId)
    {
        LockNumber = lockNumber;
        Interior = interior;
        EntrancePosX = entrancePosX;
        EntrancePosY = entrancePosY;
        EntrancePosZ = entrancePosZ;
        EntranceDimension = entranceDimension;
        Value = value;
        ExitPosX = exitPosX;
        ExitPosY = exitPosY;
        ExitPosZ = exitPosZ;
        Address = address;
        Number = number;
        Items = [];
        Furnitures = [];
        FactionId = factionId;
        Name = name;
        Entrances = [];
        ParentPropertyId = parentPropertyId;
        EntranceRotR = entranceRotR;
        EntranceRotP = entranceRotP;
        EntranceRotY = entranceRotY;
        ExitRotR = exitRotR;
        ExitRotP = exitRotP;
        ExitRotY = exitRotY;
        CompanyId = companyId;
    }

    public void Update(PropertyInterior interior, float entrancePosX, float entrancePosY, float entrancePosZ, uint entranceDimension,
        int value, float exitPosX, float exitPosY, float exitPosZ, string address, Guid? factionId, string? name, Guid? parentPropertyId,
        float entranceRotR, float entranceRotP, float entranceRotY,
        float exitRotR, float exitRotP, float exitRotY, Guid? companyId)
    {
        Interior = interior;
        EntrancePosX = entrancePosX;
        EntrancePosY = entrancePosY;
        EntrancePosZ = entrancePosZ;
        EntranceDimension = entranceDimension;
        Value = value;
        ExitPosX = exitPosX;
        ExitPosY = exitPosY;
        ExitPosZ = exitPosZ;
        Address = address;
        FactionId = factionId;
        Name = name;
        ParentPropertyId = parentPropertyId;
        EntranceRotR = entranceRotR;
        EntranceRotP = entranceRotP;
        EntranceRotY = entranceRotY;
        ExitRotR = exitRotR;
        ExitRotP = exitRotP;
        ExitRotY = exitRotY;
        CompanyId = companyId;
    }

    public void RemoveOwner()
    {
        CharacterId = null;
        Locked = false;
        ProtectionLevel = 0;
        RobberyValue = 0;
        RobberyCooldown = null;
        LockNumber = 0;
        PurchaseDate = null;
    }

    public void SetOwner(Guid characterId)
    {
        CharacterId = characterId;
        PurchaseDate = DateTime.Now;
    }

    public void SetLocked(bool locked)
    {
        Locked = locked;
    }

    public void SetProtectionLevel(byte value)
    {
        ProtectionLevel = value;
    }

    public void SetLockNumber(uint value)
    {
        LockNumber = value;
    }

    public void SetRobberyValue(int value)
    {
        RobberyValue = value;
    }

    public void ResetRobberyValue()
    {
        RobberyValue = 0;
    }

    public void SetRobberyCooldown(DateTime cooldown)
    {
        RobberyCooldown = cooldown;
    }

    public void SetTime(byte? time)
    {
        Time = time;
    }

    public void SetWeather(byte? weather)
    {
        Weather = weather;
    }
}