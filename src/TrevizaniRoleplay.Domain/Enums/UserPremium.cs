using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum UserPremium : byte
{
    [Display(Name = Globalization.NONE)]
    None = 1,

    [Display(Name = Globalization.BRONZE)]
    Bronze = 2,

    [Display(Name = Globalization.SILVER)]
    Silver = 3,

    [Display(Name = Globalization.GOLD)]
    Gold = 4,
}