using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum WhoCanLogin : byte
{
    [Display(Name = Globalization.ALL)]
    All = 1,

    [Display(Name = Globalization.ONLY_STAFF_OR_PREMIUM_POINTS)]
    OnlyStaffOrPremiumPoints = 2,

    [Display(Name = Globalization.ONLY_STAFF)]
    OnlyStaff = 3,
}