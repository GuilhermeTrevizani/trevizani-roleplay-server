namespace TrevizaniRoleplay.Core.Models.Responses;

public class FactionEquipmentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool PropertyOrVehicle { get; set; }
    public bool SWAT { get; set; }
    public bool UPR { get; set; }
}