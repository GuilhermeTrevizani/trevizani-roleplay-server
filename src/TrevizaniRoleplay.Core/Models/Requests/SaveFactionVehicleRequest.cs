namespace TrevizaniRoleplay.Core.Models.Requests;

public class SaveFactionVehicleRequest
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public string Description { get; set; } = string.Empty;
}