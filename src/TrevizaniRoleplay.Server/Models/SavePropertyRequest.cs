
using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Models;

public class SavePropertyRequest
{
    public Guid? Id { get; set; }
    public int Interior { get; set; }
    public int Value { get; set; }
    public uint Dimension { get; set; }
    public Vector3 EntrancePosition { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string? FactionName { get; set; }
    public string? Name { get; set; }
    public Vector3 ExitPosition { get; set; } = default!;
    public Vector3 EntranceRotation { get; set; } = default!;
    public Vector3 ExitRotation { get; set; } = default!;
    public string? CompanyName { get; set; }
    public int? ParentPropertyNumber { get; set; }
}