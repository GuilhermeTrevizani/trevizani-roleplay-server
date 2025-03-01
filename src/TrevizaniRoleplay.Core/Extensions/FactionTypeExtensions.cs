using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class FactionTypeExtensions
{
    public static string GetDescription(this FactionType factionType)
    {
        return factionType switch
        {
            FactionType.Police => Resources.Police,
            FactionType.Firefighter => Resources.Firefighter,
            FactionType.Criminal => Resources.Criminal,
            FactionType.Media => Resources.Media,
            FactionType.Government => Resources.Government,
            FactionType.Judiciary => Resources.Judiciary,
            FactionType.Civil => Resources.Civil,
            _ => factionType.ToString(),
        };
    }
}