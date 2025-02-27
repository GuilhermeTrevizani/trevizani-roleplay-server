using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum UserStaff : byte
{
    [Display(Name = Globalization.NONE)]
    None = 1,

    [Display(Name = Globalization.SERVER_SUPPORT)]
    ServerSupport = 2,

    [Display(Name = Globalization.JUNIOR_SERVER_ADMIN)]
    JuniorServerAdmin = 5,

    [Display(Name = Globalization.SERVER_ADMIN_I)]
    ServerAdminI = 10,

    [Display(Name = Globalization.SERVER_ADMIN_II)]
    ServerAdminII = 11,

    [Display(Name = Globalization.SENIOR_SERVER_ADMIN)]
    SeniorServerAdmin = 15,

    [Display(Name = Globalization.LEAD_SERVER_ADMIN)]
    LeadServerAdmin = 20,

    [Display(Name = Globalization.SERVER_MANAGER)]
    ServerManager = 254,

    [Display(Name = Globalization.HEAD_SERVER_DEVELOPER)]
    HeadServerDeveloper = 255,
}