namespace TrevizaniRoleplay.Server.Models;

public class Announcement
{
    public required AnnouncementType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public required string Sender { get; set; } = string.Empty;
    public required string Message { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
}