using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Requests;

public class CreateCharacterRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public CharacterSex Sex { get; set; }
    public int Age { get; set; }
}