namespace TrevizaniRoleplay.Core.Models.Responses;

public class FurnitureResponse
{
    public Guid? Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool Door { get; set; }
    public bool AudioOutput { get; set; }
    public string TVTexture { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public bool UseSlot { get; set; }
}