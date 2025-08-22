namespace TrevizaniRoleplay.Core.Models.Requests;

public class SaveFactionRankRequest
{
    public Guid FactionId { get; set; }
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}