namespace TrevizaniRoleplay.Core.Models.Responses;

public class LogResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string OriginCharacterName { get; set; } = string.Empty;
    public string OriginIp { get; set; } = string.Empty;
    public string OriginSocialClubName { get; set; } = string.Empty;
    public string TargetCharacterName { get; set; } = string.Empty;
    public string TargetIp { get; set; } = string.Empty;
    public string TargetSocialClubName { get; set; } = string.Empty;
}