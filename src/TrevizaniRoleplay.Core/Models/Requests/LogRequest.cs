using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Requests;

public class LogRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public LogType? Type { get; set; }
    public string? OriginCharacter { get; set; }
    public string? OriginIp { get; set; }
    public string? OriginUser { get; set; }
    public string OriginSocialClubName { get; set; } = string.Empty;
    public string? TargetCharacter { get; set; }
    public string? TargetIp { get; set; }
    public string? TargetUser { get; set; }
    public string? Description { get; set; }
    public string TargetSocialClubName { get; set; } = string.Empty;
}