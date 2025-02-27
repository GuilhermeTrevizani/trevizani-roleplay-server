using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Settings;

public class PolicySettings
{
    public const string POLICY_SERVER_SUPPORT = nameof(UserStaff.ServerSupport);
    public const string POLICY_JUNIOR_SERVER_ADMIN = nameof(UserStaff.JuniorServerAdmin);
    public const string POLICY_LEAD_SERVER_ADMIN = nameof(UserStaff.LeadServerAdmin);
    public const string POLICY_SERVER_MANAGER = nameof(UserStaff.ServerManager);
    public const string POLICY_STAFF_FLAG_FURNITURES = nameof(StaffFlag.Furnitures);
    public const string POLICY_STAFF_FLAG_PROPERTIES = nameof(StaffFlag.Properties);
    public const string POLICY_STAFF_FLAG_FACTIONS = nameof(StaffFlag.Factions);
    public const string POLICY_STAFF_FLAG_ANIMATIONS = nameof(StaffFlag.Animations);
}