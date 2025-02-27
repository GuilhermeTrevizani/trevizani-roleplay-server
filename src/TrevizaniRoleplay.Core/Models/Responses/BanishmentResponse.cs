namespace TrevizaniRoleplay.Core.Models.Responses;

public class BanishmentResponse
{
    public Guid Id { get; set; }
    public DateTime RegisterDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string UserStaff { get; set; } = string.Empty;
}