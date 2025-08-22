using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Settings;

public class PolicySettings
{
    public const string POLICY_TESTER = nameof(UserStaff.Tester);
    public const string POLICY_GAME_ADMIN = nameof(UserStaff.GameAdmin);
    public const string POLICY_LEAD_ADMIN = nameof(UserStaff.LeadAdmin);
    public const string POLICY_HEAD_ADMIN = nameof(UserStaff.HeadAdmin);
    public const string POLICY_MANAGEMENT = nameof(UserStaff.Management);
    public const string POLICY_STAFF_FLAG_FURNITURES = nameof(StaffFlag.Furnitures);
    public const string POLICY_STAFF_FLAG_PROPERTIES = nameof(StaffFlag.Properties);
    public const string POLICY_STAFF_FLAG_FACTIONS = nameof(StaffFlag.Factions);
    public const string POLICY_STAFF_FLAG_ANIMATIONS = nameof(StaffFlag.Animations);
    public const string POLICY_STAFF_FLAG_ITEMS = nameof(StaffFlag.Items);
    public const string POLICY_STAFF_FLAG_DRUGS = nameof(StaffFlag.Drugs);
}