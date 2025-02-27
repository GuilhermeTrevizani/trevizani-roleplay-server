using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CharacterWeaponLicenseType : byte
{
    [Display(Name = Globalization.PF)]
    PF = 1,

    [Display(Name = Globalization.CCW)]
    CCW = 2,

    [Display(Name = Globalization.GC)]
    GC = 3,

    [Display(Name = Globalization.LEO)]
    LEO = 4,
}