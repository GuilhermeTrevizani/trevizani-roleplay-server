namespace TrevizaniRoleplay.Core.Models.Requests;

public class UCPActionReloadFactionVehicleRequest
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
}