using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class ItemCategoryResponse
{
    public ItemCategory Value { get; set; }
    public string Label { get; set; } = default!;
    public bool HasType { get; set; }
}