using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class PunishmentTypeExtensions
{
    public static string GetDescription(this PunishmentType punishmentType)
    {
        return punishmentType switch
        {
            PunishmentType.Kick => Resources.Kick,
            PunishmentType.Ban => Resources.Ban,
            PunishmentType.Warn => Resources.Warn,
            PunishmentType.Ajail => Resources.Ajail,
            _ => punishmentType.ToString(),
        };
    }
}