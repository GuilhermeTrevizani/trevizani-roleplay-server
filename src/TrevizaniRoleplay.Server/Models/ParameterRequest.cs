using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Server.Models;

public class ParameterRequest
{
    public int HospitalValue { get; set; }
    public int BarberValue { get; set; }
    public int ClothesValue { get; set; }
    public int DriverLicenseBuyValue { get; set; }
    public int DriverLicenseRenewValue { get; set; }
    public int FuelValue { get; set; }
    public int Paycheck { get; set; }
    public int AnnouncementValue { get; set; }
    public int ExtraPaymentGarbagemanValue { get; set; }
    public bool Blackout { get; set; }
    public int KeyValue { get; set; }
    public int LockValue { get; set; }
    public string IPLsJSON { get; set; } = string.Empty;
    public int TattooValue { get; set; }
    public int CooldownDismantleHours { get; set; }
    public int PropertyRobberyConnectedTime { get; set; }
    public int CooldownPropertyRobberyRobberHours { get; set; }
    public int CooldownPropertyRobberyPropertyHours { get; set; }
    public int PoliceOfficersPropertyRobbery { get; set; }
    public byte InitialTimeCrackDen { get; set; }
    public byte EndTimeCrackDen { get; set; }
    public int FirefightersBlockHeal { get; set; }
    public float PropertyProtectionLevelPercentageValue { get; set; }
    public float VehicleDismantlingPercentageValue { get; set; }
    public int VehicleDismantlingSeizedDays { get; set; }
    public string VehicleDismantlingPartsChanceJSON { get; set; } = string.Empty;
    public int VehicleDismantlingMinutes { get; set; }
    public int PlasticSurgeryValue { get; set; }
    public WhoCanLogin WhoCanLogin { get; set; }
    public string FishingItemsChanceJSON { get; set; } = string.Empty;
    public float VehicleInsurancePercentage { get; set; }
    public float PrisonInsidePosX { get; set; }
    public float PrisonInsidePosY { get; set; }
    public float PrisonInsidePosZ { get; set; }
    public int PrisonInsideDimension { get; set; }
    public float PrisonOutsidePosX { get; set; }
    public float PrisonOutsidePosY { get; set; }
    public float PrisonOutsidePosZ { get; set; }
    public int PrisonOutsideDimension { get; set; }
    public string WeaponsInfosJSON { get; set; } = string.Empty;
    public string BodyPartsDamagesJSON { get; set; } = string.Empty;
    public int WeaponLicenseMonths { get; set; }
    public int WeaponLicenseMaxWeapon { get; set; }
    public int WeaponLicenseMaxAmmo { get; set; }
    public int WeaponLicenseMaxAttachment { get; set; }
    public int WeaponLicensePurchaseDaysInterval { get; set; }
    public string PremiumItemsJSON { get; set; } = string.Empty;
    public string AudioRadioStationsJSON { get; set; } = string.Empty;
    public int UnemploymentAssistance { get; set; }
}