using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public int WeekRentValue { get; private set; }
    public DateTime? RentPaymentDate { get; private set; }
    public Guid? CharacterId { get; private set; }
    public string Color { get; private set; } = "000000";
    public ushort BlipType { get; private set; }
    public byte BlipColor { get; private set; }
    public CompanyType Type { get; private set; }
    public int Safe { get; private set; }
    public bool EntranceBenefit { get; private set; }
    public DateTime? EntranceBenefitCooldown { get; private set; }
    public string EntranceBenefitUsersJson { get; private set; } = "[]";

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public ICollection<CompanyCharacter>? Characters { get; private set; }

    [JsonIgnore]
    public ICollection<CompanyItem>? Items { get; private set; }

    [NotMapped]
    public Guid? EmployeeOnDuty { get; private set; }

    [JsonIgnore]
    public ICollection<CompanyTuningPrice>? TuningPrices { get; private set; }

    [JsonIgnore]
    public ICollection<CompanySafeMovement>? SafeMovements { get; private set; }

    public void Create(string name, float posX, float posY, float posZ, int weekRentValue, CompanyType type,
        ushort blipType, byte blipColor, bool entranceBenefit)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        WeekRentValue = weekRentValue;
        Type = type;
        BlipType = blipType;
        BlipColor = blipColor;
        Characters = [];
        Items = [];
        TuningPrices = [];
        EntranceBenefit = entranceBenefit;
    }

    public void Update(string name, float posX, float posY, float posZ, int weekRentValue, CompanyType type,
        ushort blipType, byte blipColor, bool entranceBenefit)
    {
        Name = name;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        WeekRentValue = weekRentValue;
        Type = type;
        BlipType = blipType;
        BlipColor = blipColor;
        EntranceBenefit = entranceBenefit;
    }

    public void Rent(Guid characterId)
    {
        CharacterId = characterId;
        RenewRent();
    }

    public void Update(string color, ushort blipType, byte blipColor)
    {
        Color = color;
        BlipType = blipType;
        BlipColor = blipColor;
    }

    public void RenewRent()
    {
        RentPaymentDate = DateTime.Now.AddDays(7);
    }

    public void ResetOwner()
    {
        CharacterId = null;
        RentPaymentDate = null;
    }

    public void SetEmployeeOnDuty(Guid? id)
    {
        EmployeeOnDuty = id;
    }

    public void DepositSafe(int value)
    {
        Safe += value;
    }

    public void WithdrawSafe(int value)
    {
        Safe -= value;
    }

    public void SetCharacterId(Guid characterId)
    {
        CharacterId = characterId;
    }

    public void SetEntranceBenefit(DateTime? entranceBenefitCooldown, string entranceBenefitUsersJson)
    {
        EntranceBenefitCooldown = entranceBenefitCooldown;
        EntranceBenefitUsersJson = entranceBenefitUsersJson;
    }
}