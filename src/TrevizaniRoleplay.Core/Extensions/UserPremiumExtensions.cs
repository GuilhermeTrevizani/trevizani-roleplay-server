using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class UserPremiumExtensions
{
    public static string GetDescription(this UserPremium userPremium)
    {
        return userPremium switch
        {
            UserPremium.None => Resources.None,
            UserPremium.Bronze => Resources.Bronze,
            UserPremium.Silver => Resources.Silver,
            UserPremium.Gold => Resources.Gold,
            _ => userPremium.ToString(),
        };
    }
}