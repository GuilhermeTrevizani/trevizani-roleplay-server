namespace TrevizaniRoleplay.Core.Models.Responses;

public class ItemTemplateSelectOptionResponse
{
    public Guid Value { get; set; }
    public string Label { get; set; } = default!;
    public bool IsStack { get; set; }
}