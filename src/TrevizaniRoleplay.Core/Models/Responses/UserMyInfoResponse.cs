using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class UserMyInfoResponse
{
    public int PremiumPoints { get; set; }
    public UserStaff Staff { get; set; }
    public StaffFlag[] StaffFlags { get; set; } = [];
}