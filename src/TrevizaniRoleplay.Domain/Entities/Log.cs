using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Log : BaseEntity
{
    public LogType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? OriginCharacterId { get; private set; }
    public string OriginIp { get; private set; } = string.Empty;
    public string OriginSocialClubName { get; private set; } = string.Empty;
    public Guid? OriginUserId { get; private set; }
    public Guid? TargetCharacterId { get; private set; }
    public string TargetIp { get; private set; } = string.Empty;
    public string TargetSocialClubName { get; private set; } = string.Empty;

    [JsonIgnore]
    public Character? OriginCharacter { get; private set; }

    [JsonIgnore]
    public Character? TargetCharacter { get; private set; }

    [JsonIgnore]
    public User? OriginUser { get; private set; }

    public void Create(LogType type, string description,
        Guid? originCharacterId, string originIp, string originSocialClubName, Guid? originUserId,
        Guid? targetCharacterId, string targetIp, string targetSocialClubName)
    {
        Type = type;
        Description = description;
        OriginCharacterId = originCharacterId;
        OriginIp = originIp;
        OriginSocialClubName = originSocialClubName;
        OriginUserId = originUserId;
        TargetCharacterId = targetCharacterId;
        TargetIp = targetIp;
        TargetSocialClubName = targetSocialClubName;
    }
}