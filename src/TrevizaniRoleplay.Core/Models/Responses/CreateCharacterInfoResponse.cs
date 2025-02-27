using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class CreateCharacterInfoResponse
{
    public string Name { get; set; } = string.Empty;
    public CharacterSex Sex { get; set; }
    public int Age { get; set; }
    public string History { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? Staffer { get; set; }
}