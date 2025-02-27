using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class BodyItem : BaseItem
{
    public Guid BodyId { get; private set; }
    public byte Slot { get; private set; }

    [JsonIgnore]
    public Body? Body { get; private set; }

    public void SetSlot(byte slot)
    {
        Slot = slot;
    }
}