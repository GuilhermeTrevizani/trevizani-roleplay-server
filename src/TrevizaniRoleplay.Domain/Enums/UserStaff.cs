namespace TrevizaniRoleplay.Domain.Enums;

public enum UserStaff : byte
{
    None = 1,
    ServerSupport = 2,
    JuniorServerAdmin = 5,
    ServerAdminI = 10,
    ServerAdminII = 11,
    SeniorServerAdmin = 15,
    LeadServerAdmin = 20,
    ServerManager = 254,
    HeadServerDeveloper = 255,
}