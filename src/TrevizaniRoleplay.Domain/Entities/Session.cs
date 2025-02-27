using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Session : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public SessionType Type { get; private set; }
    public DateTime? FinalDate { get; private set; }
    public string Ip { get; private set; } = string.Empty;
    public string SocialClubName { get; private set; } = string.Empty;

    [JsonIgnore]
    public Character? Character { get; private set; }

    public void Create(Guid characterId, SessionType type, string ip, string socialClubName)
    {
        CharacterId = characterId;
        Type = type;
        Ip = ip;
        SocialClubName = socialClubName;
    }

    public void End()
    {
        FinalDate = DateTime.Now;
    }
}