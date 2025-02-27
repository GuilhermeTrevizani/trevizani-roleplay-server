namespace TrevizaniRoleplay.Core.Models.Responses;

public class AnimationResponse
{
    public Guid? Id { get; set; }
    public string Display { get; set; } = string.Empty;
    public string Dictionary { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Flag { get; set; }
    public bool OnlyInVehicle { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
}