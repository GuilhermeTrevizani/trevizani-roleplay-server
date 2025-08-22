using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Parameter : BaseEntity
{
    public int MaxCharactersOnline { get; private set; }
    public int HospitalValue { get; private set; } = 1;
    public int BarberValue { get; private set; } = 1;
    public int ClothesValue { get; private set; } = 1;
    public int DriverLicenseBuyValue { get; private set; } = 1;
    public int DriverLicenseRenewValue { get; private set; } = 1;
    public int FuelValue { get; private set; } = 1;
    public int Paycheck { get; private set; } = 1;
    public int AnnouncementValue { get; private set; } = 1;
    public int ExtraPaymentGarbagemanValue { get; private set; } = 1;
    public bool Blackout { get; private set; }
    public string IPLsJSON { get; private set; } = "[]";
    public int TattooValue { get; private set; } = 1;
    public int CooldownDismantleHours { get; private set; }
    public int PropertyRobberyConnectedTime { get; private set; }
    public int CooldownPropertyRobberyRobberHours { get; private set; }
    public int CooldownPropertyRobberyPropertyHours { get; private set; }
    public int PoliceOfficersPropertyRobbery { get; private set; }
    public byte InitialTimeCrackDen { get; private set; }
    public byte EndTimeCrackDen { get; private set; }
    public int FirefightersBlockHeal { get; private set; }
    public float PropertyProtectionLevelPercentageValue { get; private set; } = 1;
    public float VehicleDismantlingPercentageValue { get; private set; } = 1;
    public int VehicleDismantlingSeizedDays { get; private set; } = 1;
    public string VehicleDismantlingPartsChanceJSON { get; private set; } = "[]";
    public int VehicleDismantlingMinutes { get; private set; } = 1;
    public int PlasticSurgeryValue { get; private set; } = 1;
    public WhoCanLogin WhoCanLogin { get; private set; }
    public string FishingItemsChanceJSON { get; private set; } = "[]";
    public float VehicleInsurancePercentage { get; private set; }
    public float PrisonInsidePosX { get; private set; }
    public float PrisonInsidePosY { get; private set; }
    public float PrisonInsidePosZ { get; private set; }
    public uint PrisonInsideDimension { get; private set; }
    public float PrisonOutsidePosX { get; private set; }
    public float PrisonOutsidePosY { get; private set; }
    public float PrisonOutsidePosZ { get; private set; }
    public uint PrisonOutsideDimension { get; private set; }
    public string WeaponsInfosJSON { get; private set; } = "[]";
    public string BodyPartsDamagesJSON { get; private set; } = "[]";
    public int WeaponLicenseMonths { get; private set; }
    public int WeaponLicenseMaxWeapon { get; private set; }
    public int WeaponLicenseMaxAmmo { get; private set; }
    public int WeaponLicenseMaxAttachment { get; private set; }
    public int WeaponLicensePurchaseDaysInterval { get; private set; }
    public string PremiumItemsJSON { get; private set; } = "[]";
    public string AudioRadioStationsJSON { get; private set; } = "[]";
    public int UnemploymentAssistance { get; private set; }
    public string PremiumPointPackagesJSON { get; private set; } = "[]";
    public string MOTD { get; private set; } = string.Empty;
    public uint EntranceBenefitValue { get; private set; }
    public uint EntranceBenefitCooldownUsers { get; private set; }
    public uint EntranceBenefitCooldownHours { get; private set; }

    public void SetMaxCharactersOnline(int value)
    {
        MaxCharactersOnline = value;
    }

    public void Update(int hospitalValue, int barberValue, int clothesValue, int driverLicenseBuyValue,
        int paycheck, int driverLicenseRenewValue, int announcementValue, int extraPaymentGarbagemanValue, bool blackout,
        string iplsJSON, int tattooValue, int cooldownDismantleHours,
        int propertyRobberyConnectedTime, int cooldownPropertyRobberyRobberHours, int cooldownPropertyRobberyPropertyHours,
        int policeOfficersPropertyRobbery, byte initialTimeCrackDen, byte endTimeCrackDen, int firefightersBlockHeal,
        int fuelValue, float propertyProtectionLevelPercentageValue, float vehicleDismantlingPercentageValue,
        int vehicleDismantlingSeizedDays, string vehicleDismantlingPartsChanceJSON, int vehicleDismantlingMinutes,
        int plasticSurgeryValue, WhoCanLogin whoCanLogin, string fishingItemsChanceJSON, float vehicleInsurancePercentage,
        float prisonInsidePosX, float prisonInsidePosY, float prisonInsidePosZ, uint prisonInsideDimension,
        float prisonOutsidePosX, float prisonOutsidePosY, float prisonOutsidePosZ, uint prisonOutsideDimension,
        string weaponsInfosJSON, string bodyPartsDamagesJSON, int weaponLicenseMonths, int weaponLicenseMaxWeapon,
        int weaponLicenseMaxAmmo, int weaponLicenseMaxAttachment, int weaponLicensePurchaseDaysInterval,
        string premiumItemsJSON, string audioRadioStationsJson, int unemploymentAssistance, string premiumPointPackagesJSON,
        string motd, uint entranceBenefitValue, uint entranceBenefitCooldownUsers, uint entranceBenefitCooldownHours)
    {
        HospitalValue = hospitalValue;
        BarberValue = barberValue;
        ClothesValue = clothesValue;
        DriverLicenseBuyValue = driverLicenseBuyValue;
        Paycheck = paycheck;
        DriverLicenseRenewValue = driverLicenseRenewValue;
        AnnouncementValue = announcementValue;
        ExtraPaymentGarbagemanValue = extraPaymentGarbagemanValue;
        Blackout = blackout;
        IPLsJSON = iplsJSON;
        TattooValue = tattooValue;
        CooldownDismantleHours = cooldownDismantleHours;
        PropertyRobberyConnectedTime = propertyRobberyConnectedTime;
        CooldownPropertyRobberyRobberHours = cooldownPropertyRobberyRobberHours;
        CooldownPropertyRobberyPropertyHours = cooldownPropertyRobberyPropertyHours;
        PoliceOfficersPropertyRobbery = policeOfficersPropertyRobbery;
        InitialTimeCrackDen = initialTimeCrackDen;
        EndTimeCrackDen = endTimeCrackDen;
        FirefightersBlockHeal = firefightersBlockHeal;
        FuelValue = fuelValue;
        PropertyProtectionLevelPercentageValue = propertyProtectionLevelPercentageValue;
        VehicleDismantlingPercentageValue = vehicleDismantlingPercentageValue;
        VehicleDismantlingSeizedDays = vehicleDismantlingSeizedDays;
        VehicleDismantlingPartsChanceJSON = vehicleDismantlingPartsChanceJSON;
        VehicleDismantlingMinutes = vehicleDismantlingMinutes;
        PlasticSurgeryValue = plasticSurgeryValue;
        WhoCanLogin = whoCanLogin;
        FishingItemsChanceJSON = fishingItemsChanceJSON;
        VehicleInsurancePercentage = vehicleInsurancePercentage;
        PrisonInsidePosX = prisonInsidePosX;
        PrisonInsidePosY = prisonInsidePosY;
        PrisonInsidePosZ = prisonInsidePosZ;
        PrisonInsideDimension = prisonInsideDimension;
        PrisonOutsidePosX = prisonOutsidePosX;
        PrisonOutsidePosY = prisonOutsidePosY;
        PrisonOutsidePosZ = prisonOutsidePosZ;
        PrisonOutsideDimension = prisonOutsideDimension;
        WeaponsInfosJSON = weaponsInfosJSON;
        BodyPartsDamagesJSON = bodyPartsDamagesJSON;
        WeaponLicenseMonths = weaponLicenseMonths;
        WeaponLicenseMaxWeapon = weaponLicenseMaxWeapon;
        WeaponLicenseMaxAmmo = weaponLicenseMaxAmmo;
        WeaponLicenseMaxAttachment = weaponLicenseMaxAttachment;
        WeaponLicensePurchaseDaysInterval = weaponLicensePurchaseDaysInterval;
        PremiumItemsJSON = premiumItemsJSON;
        AudioRadioStationsJSON = audioRadioStationsJson;
        UnemploymentAssistance = unemploymentAssistance;
        PremiumPointPackagesJSON = premiumPointPackagesJSON;
        MOTD = motd;
        EntranceBenefitValue = entranceBenefitValue;
        EntranceBenefitCooldownUsers = entranceBenefitCooldownUsers;
        EntranceBenefitCooldownHours = entranceBenefitCooldownHours;
    }

    public void SetBodyPartsDamagesJSON(string bodyPartsDamagesJSON)
    {
        BodyPartsDamagesJSON = bodyPartsDamagesJSON;
    }

    public void SetWeaponsInfosJSON(string weaponsInfosJSON)
    {
        WeaponsInfosJSON = weaponsInfosJSON;
    }

    public void SetPremiumItemsJSON(string premiumItemsJSON)
    {
        PremiumItemsJSON = premiumItemsJSON;
    }
}