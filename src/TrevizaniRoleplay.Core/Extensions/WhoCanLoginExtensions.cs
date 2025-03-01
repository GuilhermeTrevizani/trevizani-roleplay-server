using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class WhoCanLoginExtensions
{
    public static string GetDescription(this WhoCanLogin whoCanLogin)
    {
        return whoCanLogin switch
        {
            WhoCanLogin.All => Resources.All,
            WhoCanLogin.OnlyStaffOrUsersWithPremiumPoints => Resources.OnlyStaffOrUsersWithPremiumPoints,
            WhoCanLogin.OnlyStaff => Resources.OnlyStaff,
            _ => whoCanLogin.ToString(),
        };
    }
}