using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class InviteTypeExtensions
{
    public static string GetDescription(this InviteType inviteType)
    {
        return inviteType switch
        {
            InviteType.Faction => Globalization.Resources.Faction,
            InviteType.PropertySell => Globalization.Resources.PropertySell,
            InviteType.Frisk => Globalization.Resources.Frisk,
            InviteType.VehicleSell => Globalization.Resources.VehicleSell,
            InviteType.Company => Globalization.Resources.Company,
            InviteType.Mechanic => Globalization.Resources.Mechanic,
            InviteType.VehicleTransfer => Globalization.Resources.VehicleTransfer,
            InviteType.Carry => Globalization.Resources.Carry,
            _ => string.Empty,
        };
    }
}