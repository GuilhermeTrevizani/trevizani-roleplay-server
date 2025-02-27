using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class ConfiscationItem : BaseItem
{
    public Guid ConfiscationId { get; private set; }
    public string Identifier { get; private set; } = string.Empty;

    [JsonIgnore]
    public Confiscation? Confiscation { get; private set; }

    public void SetIdentifier(string identifier)
    {
        Identifier = identifier;
    }
}