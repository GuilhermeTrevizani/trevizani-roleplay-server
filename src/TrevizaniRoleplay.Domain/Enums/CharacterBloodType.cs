using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CharacterBloodType : byte
{
    [Display(Name = Globalization.A_POSITIVE)]
    APositive = 1,

    [Display(Name = Globalization.A_NEGATIVE)]
    ANegative = 2,

    [Display(Name = Globalization.B_POSITIVE)]
    BPositive = 3,

    [Display(Name = Globalization.B_NEGATIVE)]
    BNegative = 4,

    [Display(Name = Globalization.AB_POSITIVE)]
    ABPositive = 5,

    [Display(Name = Globalization.AB_NEGATIVE)]
    ABNegative = 6,

    [Display(Name = Globalization.O_POSITIVE)]
    OPositive = 7,

    [Display(Name = Globalization.O_NEGATIVE)]
    ONegative = 8,
}