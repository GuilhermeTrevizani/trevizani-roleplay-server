using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Server.Models;

public record UCPSettingsRequest(bool TimeStampToggle, bool AnnouncementToggle, bool PMToggle,
    bool FactionChatToggle, bool StaffChatToggle, int ChatFontType, int ChatLines, int ChatFontSize,
    bool FactionToggle, bool VehicleTagToggle, string ChatBackgroundColor, bool ShowNametagId,
    bool AmbientSoundToggle, int FreezingTimePropertyEntrance, bool ShowOwnNametag, bool StaffToggle,
    bool FactionWalkieTalkieToggle, UserReceiveSMSDiscord ReceiveSMSDiscord);