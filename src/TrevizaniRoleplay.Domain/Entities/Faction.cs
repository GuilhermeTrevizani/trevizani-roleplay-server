using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Faction : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public FactionType Type { get; private set; }
    public string Color { get; private set; } = "FFFFFF";
    public int Slots { get; private set; }
    public string ChatColor { get; private set; } = "FFFFFF";
    public Guid? CharacterId { get; private set; }
    public string ShortName { get; private set; } = string.Empty;

    [JsonIgnore]
    public Character? Character { get; private set; }

    [NotMapped]
    public bool BlockedChat { get; private set; }

    [NotMapped]
    public bool Government =>
        Type == FactionType.Police || Type == FactionType.Firefighter;

    [NotMapped]
    public bool HasBarriers =>
        Type == FactionType.Police || Type == FactionType.Firefighter || Type == FactionType.Government;

    [NotMapped]
    public bool HasGovernmentAdvertisement =>
        Type == FactionType.Police || Type == FactionType.Firefighter || Type == FactionType.Government || Type == FactionType.Judiciary;

    [NotMapped]
    public bool HasVehicles =>
        Type == FactionType.Police || Type == FactionType.Firefighter || Type == FactionType.Government || Type == FactionType.Judiciary
        || Type == FactionType.Media || Type == FactionType.Civil;

    [NotMapped]
    public bool HasDuty =>
        Type == FactionType.Police || Type == FactionType.Firefighter || Type == FactionType.Government || Type == FactionType.Judiciary;

    [NotMapped]
    public bool HasWalkieTalkie =>
        Type == FactionType.Police || Type == FactionType.Firefighter || Type == FactionType.Government || Type == FactionType.Judiciary;

    [NotMapped]
    public bool HasChat =>
        Type != FactionType.Criminal && Type != FactionType.Civil;

    [NotMapped]
    public bool CanSeizeVehicles =>
        Type == FactionType.Police || Type == FactionType.Government;

    [NotMapped]
    public bool HasMDC =>
        Type == FactionType.Police || Type == FactionType.Firefighter || Type == FactionType.Government;

    public void Create(string name, FactionType factionType, int slots, Guid? characterId, string shortName)
    {
        Name = name;
        Type = factionType;
        Slots = slots;
        CharacterId = characterId;
        ShortName = shortName;
    }

    public void Update(string name, FactionType factionType, int slots, Guid? characterId, string shortName)
    {
        Name = name;
        Type = factionType;
        Slots = slots;
        CharacterId = characterId;
        ShortName = shortName;
    }

    public List<FactionFlag> GetFlags()
    {
        var flags = Enum.GetValues<FactionFlag>().ToList();

        if (Government)
            flags.RemoveAll(x => x == FactionFlag.Storage);

        if (Type != FactionType.Police)
            flags.RemoveAll(x => x == FactionFlag.SWAT
                || x == FactionFlag.UPR
                || x == FactionFlag.WeaponLicense);

        if (Type != FactionType.Firefighter)
            flags.RemoveAll(x => x == FactionFlag.FireManager);

        if (!HasBarriers)
            flags.RemoveAll(x => x == FactionFlag.RemoveAllBarriers);

        if (!HasGovernmentAdvertisement)
            flags.RemoveAll(x => x == FactionFlag.GovernmentAdvertisement);

        if (!HasVehicles)
            flags.RemoveAll(x => x == FactionFlag.RespawnVehicles || x == FactionFlag.ManageVehicles);

        if (!HasWalkieTalkie)
            flags.RemoveAll(x => x == FactionFlag.HQ);

        if (!HasDuty)
            flags.RemoveAll(x => x == FactionFlag.Uniform);

        if (!HasChat)
            flags.RemoveAll(x => x == FactionFlag.BlockChat);

        return flags;
    }

    public void ToggleBlockedChat()
    {
        BlockedChat = !BlockedChat;
    }

    public void ResetCharacterId()
    {
        CharacterId = null;
    }

    public void UpdateColors(string color, string chatColor)
    {
        Color = color;
        ChatColor = chatColor;
    }
}