namespace TrevizaniRoleplay.Core.Models.Responses;

public class PropertyResponse
{
    public Guid Id { get; set; }
    public uint Number { get; set; }
    public string InteriorDisplay { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Value { get; set; }
    public string FactionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public uint? ParentPropertyNumber { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}