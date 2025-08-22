namespace TrevizaniRoleplay.Domain.Entities;

public class Notification : BaseEntity
{
    private Notification()
    {
    }

    public Notification(Guid userId, string message)
    {
        UserId = userId;
        Message = message;
    }

    public Guid UserId { get; private set; }
    public string Message { get; private set; } = default!;
    public DateTime? ReadDate { get; private set; }

    public User? User { get; private set; }

    public void MarkAsRead()
    {
        ReadDate = DateTime.Now;
    }
}