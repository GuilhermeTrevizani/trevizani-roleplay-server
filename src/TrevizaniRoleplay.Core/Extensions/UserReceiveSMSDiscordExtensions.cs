using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class UserReceiveSMSDiscordExtensions
{
    public static string GetDescription(this UserReceiveSMSDiscord userReceiveSMSDiscord)
    {
        return userReceiveSMSDiscord switch
        {
            UserReceiveSMSDiscord.All => Resources.All,
            UserReceiveSMSDiscord.OnlyIndividuals => Resources.OnlyIndividuals,
            UserReceiveSMSDiscord.None => Resources.None,
            _ => userReceiveSMSDiscord.ToString(),
        };
    }
}