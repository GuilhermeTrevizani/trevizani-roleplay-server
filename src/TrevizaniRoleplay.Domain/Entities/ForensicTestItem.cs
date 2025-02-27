using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class ForensicTestItem : BaseEntity
{
    public ForensicTestItemType Type { get; private set; }
    public Guid ForensicTestId { get; private set; }
    public Guid OriginConfiscationItemId { get; private set; }
    public Guid? TargetConfiscationItemId { get; private set; }
    public string Identifier { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;

    [JsonIgnore]
    public ForensicTest? ForensicTest { get; private set; }

    [JsonIgnore]
    public ConfiscationItem? OriginConfiscationItem { get; private set; }

    [JsonIgnore]
    public ConfiscationItem? TargetConfiscationItem { get; private set; }

    public void Create(ForensicTestItemType type,
        Guid originConfiscationItemId, Guid? targetConfiscationItemId, string identifier)
    {
        Type = type;
        OriginConfiscationItemId = originConfiscationItemId;
        TargetConfiscationItemId = targetConfiscationItemId;
        Identifier = identifier;
    }

    public void SetResult(string result)
    {
        Result = result;
    }
}