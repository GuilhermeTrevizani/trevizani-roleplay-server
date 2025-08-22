namespace TrevizaniRoleplay.Core.Models.Responses;

public class SafeMovementResponse
{
    public string Type { get; set; } = string.Empty;
    public uint Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Character { get; set; } = string.Empty;
}