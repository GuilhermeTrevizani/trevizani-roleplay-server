using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CharacterJob : byte
{
    [Display(Name = Globalization.NONE)]
    None = 1,

    [Display(Name = Globalization.TAXI_DRIVER)]
    TaxiDriver = 2,

    [Display(Name = Globalization.MECHANIC)]
    Mechanic = 3,

    [Display(Name = Globalization.GARBAGE_COLLECTOR)]
    GarbageCollector = 4,

    [Display(Name = Globalization.TRUCKER)]
    Trucker = 5,
}