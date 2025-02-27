namespace TrevizaniRoleplay.Core.Models.Responses;

public class CharacterPatrimonyResponse
{
    public int Position { get; set; }
    public string User { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public int ConnectedTime { get; set; }
    public string Job { get; set; } = string.Empty;
    public string Vehicles { get; set; } = string.Empty;
    public string Properties { get; set; } = string.Empty;
    public string Companies { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}