using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class CharacterItem : BaseItem
{
    public Guid CharacterId { get; private set; }
    public byte Slot { get; private set; }
    public bool InUse { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    public void SetSlot(byte slot)
    {
        Slot = slot;
    }

    public void SetCharacterId(Guid characterId)
    {
        CharacterId = characterId;
    }

    public void SetInUse(bool inUse)
    {
        InUse = inUse;
    }

    public void SetExtra(string? extra)
    {
        Extra = extra;
    }

    public void SetSubtype(uint subtype)
    {
        Subtype = subtype;
    }
}