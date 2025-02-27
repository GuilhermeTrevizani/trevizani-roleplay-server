using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CompanyType : byte
{
    [Display(Name = Globalization.CONVENIENCE_STORE)]
    ConvenienceStore = 1,

    [Display(Name = Globalization.MECHANIC_WORKSHOP)]
    MechanicWorkshop = 2,

    [Display(Name = Globalization.OTHER)]
    Other = 3,

    [Display(Name = Globalization.FISHMONGER)]
    Fishmonger = 4,

    [Display(Name = Globalization.WEAPON_STORE)]
    WeaponStore = 5,
}