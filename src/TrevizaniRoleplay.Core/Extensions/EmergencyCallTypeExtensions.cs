using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class EmergencyCallTypeExtensions
{
    public static string GetDescription(this EmergencyCallType emergencyCallType)
    {
        return emergencyCallType switch
        {
            EmergencyCallType.Police => Resources.Police,
            EmergencyCallType.Firefighter => Resources.Firefighter,
            EmergencyCallType.PoliceAndFirefighter => Resources.PoliceAndFirefighter,
            _ => emergencyCallType.ToString(),
        };
    }
}