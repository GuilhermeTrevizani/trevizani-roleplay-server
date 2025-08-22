namespace TrevizaniRoleplay.Core.Models.Requests;

public class FactionEquipmentItemRequest
{
    public Guid? Id { get; set; }
    public Guid FactionEquipmentId { get; set; }
    public string Weapon { get; set; } = default!;
    public uint Ammo { get; set; }
    public IEnumerable<uint> Components { get; set; } = default!;
}