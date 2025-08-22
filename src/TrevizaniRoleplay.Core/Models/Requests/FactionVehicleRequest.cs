namespace TrevizaniRoleplay.Core.Models.Requests;

public class FactionVehicleRequest
{
    public string Model { get; set; } = default!;
    public Guid FactionId { get; set; }
}