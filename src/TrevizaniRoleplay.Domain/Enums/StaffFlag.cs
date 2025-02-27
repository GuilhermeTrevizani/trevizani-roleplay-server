using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum StaffFlag : byte
{
    [Display(Name = Globalization.EVENTS)]
    Events = 1,

    [Display(Name = Globalization.DOORS)]
    Doors = 2,

    [Display(Name = Globalization.JOBS)]
    Jobs = 3,

    [Display(Name = Globalization.FACTIONS)]
    Factions = 4,

    [Display(Name = Globalization.FACTIONS_STORAGES)]
    FactionsStorages = 5,

    [Display(Name = Globalization.PROPERTIES)]
    Properties = 6,

    [Display(Name = Globalization.SPOTS)]
    Spots = 7,

    [Display(Name = Globalization.BLIPS)]
    Blips = 8,

    [Display(Name = Globalization.VEHICLES)]
    Vehicles = 9,

    [Display(Name = Globalization.CK)]
    CK = 10,

    [Display(Name = Globalization.GIVE_ITEM)]
    GiveItem = 11,

    [Display(Name = Globalization.CRACK_DENS)]
    CrackDens = 12,

    [Display(Name = Globalization.TRUCKER_LOCATIONS)]
    TruckerLocations = 13,

    [Display(Name = Globalization.FURNITURES)]
    Furnitures = 14,

    [Display(Name = Globalization.ANIMATIONS)]
    Animations = 15,

    [Display(Name = Globalization.COMPANIES)]
    Companies = 16,

    // 17 is free

    [Display(Name = Globalization.ITEMS)]
    Items = 18,

    [Display(Name = Globalization.VEHICLE_MAINTENANCE)]
    VehicleMaintenance = 19,

    [Display(Name = Globalization.DRUGS)]
    Drugs = 20,
}