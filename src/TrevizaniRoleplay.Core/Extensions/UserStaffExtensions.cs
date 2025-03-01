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
            UserStaff.JuniorServerAdmin => Resources.JuniorServerAdmin,
            UserStaff.ServerAdminI => Resources.ServerAdminI,
            UserStaff.ServerAdminII => Resources.ServerAdminII,
            UserStaff.SeniorServerAdmin => Resources.SeniorServerAdmin,
            UserStaff.LeadServerAdmin => Resources.LeadServerAdmin,
            UserStaff.ServerManager => Resources.ServerManager,
            UserStaff.HeadServerDeveloper => Resources.HeadServerDeveloper,
            _ => userStaff.ToString(),
        };
    }
}