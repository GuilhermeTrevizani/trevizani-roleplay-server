namespace TrevizaniRoleplay.Domain.Enums;

public enum WhoCanLogin : byte
{
    All = 1,
    OnlyStaffOrUsersWithPremiumPoints = 2,
    OnlyStaff = 3,
}