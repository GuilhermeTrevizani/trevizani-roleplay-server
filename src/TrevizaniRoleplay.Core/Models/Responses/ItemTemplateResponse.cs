using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class ItemTemplateResponse
{
    public Guid Id { get; set; }
    public ItemCategory Category { get; set; }
    public string CategoryDisplay { get; set; } = default!;
    public string Name { get; set; } = default!;
    public uint Weight { get; set; }
    public string Image { get; set; } = default!;
    public string ObjectModel { get; set; } = default!;
    public string Type { get; set; } = default!;
}
