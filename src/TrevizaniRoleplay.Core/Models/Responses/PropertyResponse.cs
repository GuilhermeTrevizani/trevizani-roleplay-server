namespace TrevizaniRoleplay.Core.Models.Responses;

public class PropertyResponse
{
    public Guid Id { get; set; }
    public uint Number { get; set; }
    public string InteriorDisplay { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Value { get; set; }
    public uint EntranceDimension { get; set; }
    public float EntrancePosX { get; set; }
    public float EntrancePosY { get; set; }
    public float EntrancePosZ { get; set; }
    public string FactionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float ExitPosX { get; set; }
    public float ExitPosY { get; set; }
    public float ExitPosZ { get; set; }
    public uint? ParentPropertyNumber { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}