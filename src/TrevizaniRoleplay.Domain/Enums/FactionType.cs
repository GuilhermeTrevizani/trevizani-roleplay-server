using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum FactionType : byte
{
    [Display(Name = Globalization.POLICE)]
    Police = 1,

    [Display(Name = Globalization.FIREFIGHTER)]
    Firefighter = 2,

    [Display(Name = Globalization.CRIMINAL)]
    Criminal = 3,

    [Display(Name = Globalization.MEDIA)]
    Media = 4,

    [Display(Name = Globalization.GOVERNMENT)]
    Government = 5,

    [Display(Name = Globalization.JUDICIARY)]
    Judiciary = 6,

    [Display(Name = Globalization.CIVIL)]
    Civil = 7,
}