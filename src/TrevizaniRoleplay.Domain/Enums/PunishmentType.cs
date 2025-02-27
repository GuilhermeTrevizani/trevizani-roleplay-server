using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum PunishmentType : byte
{
    Kick = 1,

    Ban = 2,

    [Display(Name = Globalization.WARN)]
    Warn = 3,

    [Display(Name = Globalization.JAIL)]
    Ajail = 4,
}