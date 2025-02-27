using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class UCPActionExecuted : BaseEntity
{
    public UCPActionType Type { get; private set; }
    public Guid UserId { get; private set; }
    public string Json { get; private set; } = string.Empty;
    public DateTime UCPActionRegisterDate { get; private set; }

    [JsonIgnore]
    public User? User { get; private set; }

    public void Create(UCPActionType type, Guid userId, string json, DateTime ucpActionRegisterDate)
    {
        Type = type;
        UserId = userId;
        Json = json;
        UCPActionRegisterDate = ucpActionRegisterDate;
    }
}