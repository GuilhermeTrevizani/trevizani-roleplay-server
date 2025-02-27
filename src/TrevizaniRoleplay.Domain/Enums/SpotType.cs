using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum SpotType : byte
{
    [Display(Name = Globalization.BANK)]
    Bank = 1,

    [Display(Name = Globalization.COMPANY)]
    Company = 2,

    [Display(Name = Globalization.CLOTHES_STORE)]
    ClothesStore = 3,

    [Display(Name = Globalization.FACTION_VEHICLE_SPAWN)]
    FactionVehicleSpawn = 4,

    [Display(Name = Globalization.VEHICLE_SEIZURE)]
    VehicleSeizure = 5,

    [Display(Name = Globalization.VEHICLE_RELEASE)]
    VehicleRelease = 6,

    [Display(Name = Globalization.BARBER_SHOP)]
    BarberShop = 7,

    [Display(Name = Globalization.MORGUE)]
    Morgue = 8,

    [Display(Name = Globalization.FORENSIC_LABORATORY)]
    ForensicLaboratory = 9,

    [Display(Name = Globalization.DMV)]
    DMV = 10,

    // 11 is free

    [Display(Name = Globalization.HEAL_ME)]
    HealMe = 12,

    // 13 is free

    [Display(Name = Globalization.GARBAGE_COLLECTOR)]
    GarbageCollector = 14,

    // 15 is free

    // 16 is free

    [Display(Name = Globalization.TATTOO_SHOP)]
    TattooShop = 17,

    // 18 is free

    [Display(Name = Globalization.VEHICLE_DISMANTLING)]
    VehicleDismantling = 19,

    [Display(Name = Globalization.PLASTIC_SURGERY)]
    PlasticSurgery = 20,
}