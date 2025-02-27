namespace TrevizaniRoleplay.Core.Models.Requests;

public class UCPActionSendDiscordMessageRequest
{
    public ulong DiscordUserId { get; set; }
    public string Message { get; set; } = string.Empty;
}