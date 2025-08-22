namespace TrevizaniRoleplay.Core.Models.Requests;

public class SaveFactionRequest
{
    public Guid Id { get; set; }
    public string Color { get; set; } = string.Empty;
    public string ChatColor { get; set; } = string.Empty;
}