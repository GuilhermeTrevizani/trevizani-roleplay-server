namespace TrevizaniRoleplay.Server.Models;

public class AddConfiscationItemRequest
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public string Identifier { get; set; } = string.Empty;
}