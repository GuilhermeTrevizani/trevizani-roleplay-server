namespace TrevizaniRoleplay.Server.Models;

public class ForensicTestItemRequest
{
    public ForensicTestItemType Type { get; set; }
    public Guid OriginConfiscationItemId { get; set; }
    public Guid? TargetConfiscationItemId { get; set; }
    public string Identifier { get; set; } = string.Empty;
}