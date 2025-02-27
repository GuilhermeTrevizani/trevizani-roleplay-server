using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public UserStaff Staff { get; set; }
    public string DiscordUsername { get; set; } = string.Empty;
    public StaffFlag[] StaffFlags { get; set; } = [];
}