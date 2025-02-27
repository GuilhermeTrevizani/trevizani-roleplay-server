using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum EmergencyCallType : byte
{
    [Display(Name = Globalization.POLICE)]
    Police = 1,

    [Display(Name = Globalization.FIREFIGHTER)]
    Firefighter = 2,

    [Display(Name = Globalization.POLICE_AND_FIREFIGHTER)]
    PoliceAndFirefighter = 3,
}