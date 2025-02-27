using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum ItemCategory : byte
{
    [Display(Name = Globalization.WEAPON)]
    Weapon = 1,

    [Display(Name = Globalization.PROPERTY_KEY)]
    PropertyKey = 2,

    [Display(Name = Globalization.MONEY)]
    Money = 3,

    [Display(Name = Globalization.BOOMBOX)]
    Boombox = 4,

    [Display(Name = Globalization.VEHICLE_KEY)]
    VehicleKey = 5,

    [Display(Name = Globalization.WALKIE_TALKIE)]
    WalkieTalkie = 6,

    [Display(Name = Globalization.CELLPHONE)]
    Cellphone = 7,

    [Display(Name = Globalization.DRUG)]
    Drug = 8,

    // 9 is free

    // 10 is free

    // 11 is free

    // 12 is free

    // 13 is free

    // 14 is free

    // 15 is free

    [Display(Name = Globalization.MICROPHONE)]
    Microphone = 16,

    [Display(Name = Globalization.VEHICLE_PART)]
    VehiclePart = 17,

    [Display(Name = Globalization.PICK_LOCK)]
    PickLock = 18,

    [Display(Name = Globalization.FOOD)]
    Food = 19,

    [Display(Name = Globalization.DISPLAY)]
    Display = 20,

    [Display(Name = Globalization.FISHING_ROD)]
    FishingRod = 21,

    [Display(Name = Globalization.BLOOD_SAMPLE)]
    BloodSample = 22,

    [Display(Name = Globalization.BANDAGE)]
    Bandage = 23,

    [Display(Name = Globalization.WEAPON_COMPONENT)]
    WeaponComponent = 24,

    [Display(Name = Globalization.PISTOL_AMMO)]
    PistolAmmo = 25,

    [Display(Name = Globalization.SHOTGUN_AMMO)]
    ShotgunAmmo = 26,

    // 27 is free

    [Display(Name = Globalization.ASSAULT_RIFLE_AMMO)]
    AssaultRifleAmmo = 28,

    [Display(Name = Globalization.LIGHT_MACHINE_GUN_AMMO)]
    LightMachineGunAmmo = 29,

    [Display(Name = Globalization.SNIPER_RIFLE_AMMO)]
    SniperRifleAmmo = 30,

    [Display(Name = Globalization.SUB_MACHINE_GUN_AMMO)]
    SubMachineGunAmmo = 31,

    [Display(Name = Globalization.PISTOL_BULLET_SHELL)]
    PistolBulletShell = 32,

    [Display(Name = Globalization.SHOTGUN_BULLET_SHELL)]
    ShotgunBulletShell = 33,

    [Display(Name = Globalization.ASSAULT_RIFLE_BULLET_SHELL)]
    AssaultRifleBulletShell = 34,

    [Display(Name = Globalization.LIGHT_MACHINE_GUN_BULLET_SHELL)]
    LightMachineGunBulletShell = 35,

    [Display(Name = Globalization.SNIPER_RIFLE_BULLET_SHELL)]
    SniperRifleBulletShell = 36,

    [Display(Name = Globalization.SUB_MACHINE_GUN_BULLET_SHELL)]
    SubMachineGunBulletShell = 37,
}