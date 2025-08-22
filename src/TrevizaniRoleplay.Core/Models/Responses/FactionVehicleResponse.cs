namespace TrevizaniRoleplay.Core.Models.Responses;

public class FactionVehicleResponse
{
    public Guid Id { get; set; }
    public string Model { get; set; } = default!;
    public string Plate { get; set; } = default!;
    public string Description { get; set; } = default!;
}