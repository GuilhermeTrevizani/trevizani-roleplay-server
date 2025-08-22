using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class User : BaseEntity
{
    public string DiscordId { get; private set; } = string.Empty;
    public string DiscordUsername { get; private set; } = string.Empty;
    public string DiscordDisplayName { get; private set; } = string.Empty;
    public string RegisterIp { get; private set; } = string.Empty;
    public string LastAccessIp { get; private set; } = string.Empty;
    public DateTime LastAccessDate { get; private set; } = DateTime.Now;
    public UserStaff Staff { get; private set; } = UserStaff.None;
    public int NameChanges { get; private set; }
    public int HelpRequestsAnswersQuantity { get; private set; }
    public int StaffDutyTime { get; private set; }
    public bool TimeStampToggle { get; private set; } = true;
    public UserPremium Premium { get; private set; } = UserPremium.None;
    public DateTime? PremiumValidDate { get; private set; }
    public bool PMToggle { get; private set; }
    public bool FactionChatToggle { get; private set; }
    public int PlateChanges { get; private set; }
    public bool AnnouncementToggle { get; private set; }
    public int ChatFontType { get; private set; }
    public int ChatLines { get; private set; } = 10;
    public int ChatFontSize { get; private set; } = 14;
    public string StaffFlagsJSON { get; private set; } = "[]";
    public bool FactionToggle { get; private set; }
    public int CharacterApplicationsQuantity { get; private set; }
    public DateTime? CooldownDismantle { get; private set; }
    public DateTime? PropertyRobberyCooldown { get; private set; }
    public bool VehicleTagToggle { get; private set; }
    public string ChatBackgroundColor { get; private set; } = "000000";
    public int AjailMinutes { get; private set; }
    public bool ShowNametagId { get; private set; } = true;
    public int PremiumPoints { get; private set; }
    public int NumberChanges { get; private set; }
    public int CharacterSlots { get; private set; } = 2;
    public int ExtraInteriorFurnitureSlots { get; private set; }
    public int ExtraOutfitSlots { get; private set; }
    public DateTime? DiscordBoosterDate { get; private set; }
    public bool AmbientSoundToggle { get; private set; }
    public string DisplayResolution { get; private set; } = string.Empty;
    public int FreezingTimePropertyEntrance { get; private set; } = 3;
    public bool ShowOwnNametag { get; private set; }
    public UserReceiveSMSDiscord ReceiveSMSDiscord { get; private set; } = UserReceiveSMSDiscord.All;
    public bool ReceiveNotificationsOnDiscord { get; private set; } = true;

    [JsonIgnore]
    public ICollection<Character>? Characters { get; private set; }

    [NotMapped]
    public string Name => string.IsNullOrWhiteSpace(DiscordDisplayName) ? DiscordUsername : DiscordDisplayName;

    public void Create(string discordId, string discordUsername, string discordDisplayName, string ip,
        UserStaff staff, string staffFlagsJson)
    {
        DiscordId = discordId;
        DiscordUsername = discordUsername;
        DiscordDisplayName = discordDisplayName;
        RegisterIp = LastAccessIp = ip;
        Staff = staff;
        StaffFlagsJSON = staffFlagsJson;
    }

    public void UpdateLastAccess(string ip, string discordUsername, string discordDisplayName, string displayResolution)
    {
        LastAccessDate = DateTime.Now;
        LastAccessIp = ip;
        DiscordUsername = discordUsername;
        DiscordDisplayName = discordDisplayName;
        DisplayResolution = displayResolution;
    }

    public void SetPremium(UserPremium premium)
    {
        Premium = premium;
        PremiumValidDate = (PremiumValidDate > DateTime.Now && Premium == premium ? PremiumValidDate.Value : DateTime.Now).AddDays(30);
    }

    public void AddCharacterApplicationsQuantity()
    {
        CharacterApplicationsQuantity++;
    }

    public void AddHelpRequestsAnswersQuantity()
    {
        HelpRequestsAnswersQuantity++;
    }

    public void SetLastAccessDate()
    {
        LastAccessDate = DateTime.Now;
    }

    public void SetPMToggle(bool value)
    {
        PMToggle = value;
    }

    public void AddStaffDutyTime()
    {
        StaffDutyTime++;
    }

    public void RemoveNameChange()
    {
        NameChanges--;
    }

    public void UpdateSettings(bool timeStampToggle, bool announcementToggle, bool pmToggle,
        bool factionChatToggle, int chatFontType, int chatLines, int chatFontSize,
        bool factionToggle, bool vehicleTagToggle, string chatBackgroundColor, bool showNametagId,
        bool ambientSoundToggle, int freezingTimePropertyEntrance, bool showOwnNametag,
        UserReceiveSMSDiscord receiveSMSDiscord, bool receiveNotificationsOnDiscord)
    {
        TimeStampToggle = timeStampToggle;
        AnnouncementToggle = announcementToggle;
        PMToggle = pmToggle;
        FactionChatToggle = factionChatToggle;
        ChatFontType = chatFontType;
        ChatLines = chatLines;
        ChatFontSize = chatFontSize;
        FactionToggle = factionToggle;
        VehicleTagToggle = vehicleTagToggle;
        ChatBackgroundColor = chatBackgroundColor;
        ShowNametagId = showNametagId;
        AmbientSoundToggle = ambientSoundToggle;
        FreezingTimePropertyEntrance = freezingTimePropertyEntrance;
        ShowOwnNametag = showOwnNametag;
        ReceiveSMSDiscord = receiveSMSDiscord;
        ReceiveNotificationsOnDiscord = receiveNotificationsOnDiscord;
    }

    public void SetStaff(UserStaff staff, string staffFlagsJson)
    {
        Staff = staff;
        StaffFlagsJSON = staffFlagsJson;
    }

    public void RemovePlateChanges()
    {
        PlateChanges--;
    }

    public void SetCooldownDismantle(DateTime date)
    {
        CooldownDismantle = date;
    }

    public void SetPropertyRobberyCooldown(DateTime date)
    {
        PropertyRobberyCooldown = date;
    }

    public void SetVehicleTagToggle(bool value)
    {
        VehicleTagToggle = value;
    }

    public void SetTimeStampToggle(bool value)
    {
        TimeStampToggle = value;
    }

    public void SetAjailMinutes(int minutes)
    {
        AjailMinutes = minutes;
    }

    public UserPremium GetCurrentPremium()
    {
        if ((PremiumValidDate ?? DateTime.MinValue) < DateTime.Now)
            return UserPremium.None;

        return Premium;
    }

    public void SetChatFontSize(int chatFontSize)
    {
        ChatFontSize = chatFontSize;
    }

    public void SetChatLines(int chatLines)
    {
        ChatLines = chatLines;
    }

    public void AddPremiumPoints(int premiumPoints)
    {
        PremiumPoints += premiumPoints;
    }

    public void RemovePremiumPoints(int premiumPoints)
    {
        PremiumPoints -= premiumPoints;
    }

    public void AddCharacterSlots()
    {
        CharacterSlots++;
    }

    public void AddExtraOutfitSlots(int slots)
    {
        ExtraOutfitSlots += slots;
    }

    public void AddExtraInteriorFurnitureSlots(int slots)
    {
        ExtraInteriorFurnitureSlots += slots;
    }

    public void AddNameChanges()
    {
        NameChanges++;
    }

    public void AddNumberChanges()
    {
        NumberChanges++;
    }

    public void AddPlateChanges()
    {
        PlateChanges++;
    }

    public void RemoveNumberChanges()
    {
        NumberChanges--;
    }

    public void SetDiscordBoosterDate(DateTime discordBoosterDate)
    {
        DiscordBoosterDate = discordBoosterDate;
    }
}