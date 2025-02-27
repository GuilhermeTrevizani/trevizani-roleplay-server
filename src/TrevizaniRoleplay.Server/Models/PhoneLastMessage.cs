namespace TrevizaniRoleplay.Server.Models;

public class PhoneLastMessage
{
    public Guid? Id { get; set; }
    public int Number { get; set; }
    public Guid? PhoneGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Read { get; set; }
    public DateTime RegisterDate { get; set; }
}