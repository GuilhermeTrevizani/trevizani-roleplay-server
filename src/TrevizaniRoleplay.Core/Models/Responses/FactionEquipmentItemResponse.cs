namespace TrevizaniRoleplay.Core.Models.Responses;

public class FactionEquipmentItemResponse
{
    public Guid Id { get; set; }
    public string Weapon { get; set; } = default!;
    public uint Ammo { get; set; }
    public IEnumerable<uint> Components { get; set; } = default!;
}