namespace TrevizaniRoleplay.Core.Models.Responses;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = default!;
    public DateTime Date { get; set; }
    public DateTime? ReadDate { get; set; }
}