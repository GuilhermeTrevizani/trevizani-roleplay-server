using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CharacterSex : byte
{
    [Display(Name = Globalization.WOMAN)]
    Woman = 1,

    [Display(Name = Globalization.MAN)]
    Man = 2,
}