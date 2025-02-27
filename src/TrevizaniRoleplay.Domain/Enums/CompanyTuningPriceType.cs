using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CompanyTuningPriceType : byte
{
    [Display(Name = Globalization.SPOILERS)]
    Spoilers = 0,

    [Display(Name = Globalization.FRONT_BUMPER)]
    FrontBumper = 1,

    [Display(Name = Globalization.REAR_BUMPER)]
    RearBumper = 2,

    [Display(Name = Globalization.SIDE_SKIRT)]
    SideSkirt = 3,

    [Display(Name = Globalization.EXHAUST)]
    Exhaust = 4,

    [Display(Name = Globalization.FRAME)]
    Frame = 5,

    [Display(Name = Globalization.GRILLE)]
    Grille = 6,

    [Display(Name = Globalization.HOOD)]
    Hood = 7,

    [Display(Name = Globalization.FENDER)]
    Fender = 8,

    [Display(Name = Globalization.RIGHT_FENDER)]
    RightFender = 9,

    [Display(Name = Globalization.ROOF)]
    Roof = 10,

    [Display(Name = Globalization.ENGINE)]
    Engine = 11,

    [Display(Name = Globalization.BRAKES)]
    Brakes = 12,

    [Display(Name = Globalization.TRANSMISSION)]
    Transmission = 13,

    [Display(Name = Globalization.HORNS)]
    Horns = 14,

    [Display(Name = Globalization.SUSPENSION)]
    Suspension = 15,

    [Display(Name = Globalization.ARMOR)]
    Armor = 16,

    [Display(Name = Globalization.TURBO)]
    Turbo = 18,

    [Display(Name = Globalization.XENON)]
    Xenon = 22,

    [Display(Name = Globalization.PLATE_HOLDERS)]
    PlateHolders = 25,

    [Display(Name = Globalization.TRIM_DESIGN)]
    TrimDesign = 27,

    [Display(Name = Globalization.ORNAMENTS)]
    Ornaments = 28,

    [Display(Name = Globalization.DIAL_DESIGN)]
    DialDesign = 30,

    [Display(Name = Globalization.DOOR_SPEAKERS)]
    DoorSpeakers = 31,

    [Display(Name = Globalization.SEATS)]
    Seats = 32,

    [Display(Name = Globalization.STEERING_WHEEL)]
    SteeringWheel = 33,

    [Display(Name = Globalization.SHIFT_LEVER)]
    ShiftLever = 34,

    [Display(Name = Globalization.PLATE)]
    Plaques = 35,

    [Display(Name = Globalization.SPEAKERS)]
    Speakers = 36,

    [Display(Name = Globalization.TRUNK)]
    Trunk = 37,

    [Display(Name = Globalization.HYDRAULICS)]
    Hydraulics = 38,

    [Display(Name = Globalization.ENGINE_BLOCK)]
    EngineBlock = 39,

    [Display(Name = Globalization.AIR_FILTER)]
    AirFilter = 40,

    [Display(Name = Globalization.STRUTS)]
    Struts = 41,

    [Display(Name = Globalization.ARCH_COVER)]
    ArchCover = 42,

    [Display(Name = Globalization.AERIALS)]
    Aerials = 43,

    [Display(Name = Globalization.TRIM)]
    Trim = 44,

    [Display(Name = Globalization.TANK)]
    Tank = 45,

    [Display(Name = Globalization.DOORS_TRIM)]
    DoorsTrim1 = 46,

    [Display(Name = Globalization.DOORS_TRIM)]
    DoorsTrim2 = 47,

    [Display(Name = Globalization.ENVELOPING)]
    Enveloping = 48,

    [Display(Name = Globalization.ENVELOPING)]
    Enveloping2 = 49,

    [Display(Name = Globalization.WINDOW_TINT)]
    WindowTint2 = 55,

    [Display(Name = Globalization.PRIMARY_COLOR)]
    Color1 = 66,

    [Display(Name = Globalization.SECONDARY_COLOR)]
    Color2 = 67,

    [Display(Name = Globalization.WINDOW_TINT)]
    WindowTint = 69,

    [Display(Name = Globalization.DASHBOARD_COLOR)]
    DashboardColor = 74,

    [Display(Name = Globalization.TRIM_COLOR)]
    TrimColor = 75,

    [Display(Name = Globalization.DRIFT)]
    Drift = 244,

    [Display(Name = Globalization.EXTRA)]
    Extra = 245,

    [Display(Name = Globalization.LIVERY)]
    Livery = 246,

    [Display(Name = Globalization.XMR)]
    XMR = 247,

    [Display(Name = Globalization.PROTECTION_LEVEL)]
    ProtectionLevel = 248,

    [Display(Name = Globalization.TIRE_SMOKE)]
    TireSmoke = 249,

    [Display(Name = Globalization.INSUFILM)]
    Insufilm = 250,

    [Display(Name = Globalization.XENON_COLOR)]
    XenonColor = 251,

    [Display(Name = Globalization.NEON)]
    Neon = 252,

    [Display(Name = Globalization.WHEEL)]
    Wheel = 253,

    [Display(Name = Globalization.COLOR)]
    Color = 254,

    [Display(Name = Globalization.REPAIR)]
    Repair = 255,
}