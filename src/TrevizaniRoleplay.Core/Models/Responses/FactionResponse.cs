using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class FactionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string TypeDisplay { get; set; } = default!;
    public FactionType Type { get; set; }
    public int Slots { get; set; }
    public string? Leader { get; set; }
}