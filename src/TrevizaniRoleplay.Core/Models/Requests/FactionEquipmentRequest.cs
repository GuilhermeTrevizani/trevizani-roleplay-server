namespace TrevizaniRoleplay.Core.Models.Requests;

public class FactionEquipmentRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid FactionId { get; set; }
    public bool PropertyOrVehicle { get; set; }
    public bool SWAT { get; set; }
    public bool UPR { get; set; }
}