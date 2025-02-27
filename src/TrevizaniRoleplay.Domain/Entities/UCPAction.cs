using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class UCPAction : BaseEntity
{
    public UCPActionType Type { get; private set; }
    public Guid UserId { get; private set; }
    public string Json { get; private set; } = string.Empty;

    [JsonIgnore]
    public User? User { get; private set; }

    public void Create(UCPActionType type, Guid userId, string json)
    {
        Type = type;
        UserId = userId;
        Json = json;
    }
}