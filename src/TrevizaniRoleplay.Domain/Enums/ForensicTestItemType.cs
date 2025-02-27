using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum ForensicTestItemType : byte
{
    [Display(Name = Globalization.BLOOD)]
    Blood = 1,

    [Display(Name = Globalization.BULLET_SHELL)]
    BulletShell = 2,
}