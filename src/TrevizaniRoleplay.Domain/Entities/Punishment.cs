using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Punishment : BaseEntity
{
    public PunishmentType Type { get; private set; }
    public int Duration { get; private set; }
    public Guid CharacterId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid StaffUserId { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public User? StaffUser { get; private set; }

    public void Create(PunishmentType type, int duration, Guid characterId, string reason, Guid staffUserId)
    {
        Type = type;
        Duration = duration;
        CharacterId = characterId;
        Reason = reason;
        StaffUserId = staffUserId;
    }
}