namespace TrevizaniRoleplay.Domain.Enums;

public enum UserStaff : byte
{
    None = 1,
    Tester = 2,
    GameAdmin = 5,
    LeadAdmin = 15,
    HeadAdmin = 20,
    Management = 254,
    Founder = 255,
}