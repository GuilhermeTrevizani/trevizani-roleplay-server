namespace TrevizaniRoleplay.Core.Models.Responses;

public class AdministrativePunishmentResponse
{
    public string Character { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Staffer { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}