using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class UserStaffExtensions
{
    public static string GetDescription(this UserStaff userStaff)
    {
        return userStaff switch
        {
            UserStaff.None => Resources.None,
            UserStaff.Tester => Resources.Tester,
            UserStaff.GameAdmin => Resources.GameAdmin,
            UserStaff.LeadAdmin => Resources.LeadAdmin,
            UserStaff.HeadAdmin => Resources.HeadAdmin,
            UserStaff.Management => Resources.Management,
            UserStaff.Founder => Resources.Founder,
            _ => userStaff.ToString(),
        };
    }
}