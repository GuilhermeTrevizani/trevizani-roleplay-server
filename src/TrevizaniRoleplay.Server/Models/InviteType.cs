using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Models;

public enum InviteType
{
    [Display(Name = Globalization.FACTION)]
    Faction = 1,

    [Display(Name = Globalization.PROPERTY_SELL)]
    PropertySell = 2,

    [Display(Name = Globalization.FRISK)]
    Frisk = 3,

    [Display(Name = Globalization.VEHICLE_SELL)]
    VehicleSell = 4,

    [Display(Name = Globalization.COMPANY)]
    Company = 5,

    [Display(Name = Globalization.MECHANIC)]
    Mechanic = 6,

    [Display(Name = Globalization.VEHICLE_TRANSFER)]
    VehicleTransfer = 7,

    [Display(Name = Globalization.CARRY)]
    Carry = 8,

    [Display(Name = Globalization.POKER)]
    Poker = 9,
}