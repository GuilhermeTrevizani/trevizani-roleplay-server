using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class MyCharactersResponse
{
    public string? CreateCharacterWarning { get; set; }
    public IEnumerable<Character> Characters { get; set; } = [];

    public class Character
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime LastAccessDate { get; set; }
        public bool CanResendApplication { get; set; }
        public bool CanApplyNamechange { get; set; }
        public CharacterStatus Status { get; set; }
        public string DeathReason { get; set; } = string.Empty;
    }
}