using Microsoft.AspNetCore.Authorization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Api.Authorization;

public class AuthorizationRequirement
    (UserStaff? userStaff, StaffFlag? staffFlag)
    : IAuthorizationRequirement
{
    public UserStaff? UserStaff { get; } = userStaff;
    public StaffFlag? StaffFlag { get; } = staffFlag;
}