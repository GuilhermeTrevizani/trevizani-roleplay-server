using Discord;
using Discord.WebSocket;
using TrevizaniRoleplay.Core.Extensions;

namespace TrevizaniRoleplay.Server.Extensions;

public static class EmergencyCallExtension
{
    public static async Task SendMessage(this EmergencyCall emergencyCall)
    {
        async Task Send911(FactionType factionType)
        {
            var relator = emergencyCall.Number == Constants.EMERGENCY_NUMBER ? Resources.EmergencyCenter : emergencyCall.Number.ToString("000-0000");
            foreach (var player in Global.SpawnedPlayers.Where(x => x.Faction?.Type == factionType && x.OnDuty))
            {
                player.SendMessage(Models.MessageType.None, "|__________CHAMADA EMERGENCIAL__________|", Constants.EMERGENCY_CALL_COLOR);
                player.SendMessage(Models.MessageType.None, $"Relator: {relator}", Constants.EMERGENCY_CALL_COLOR);
                player.SendMessage(Models.MessageType.None, $"Serviço: {emergencyCall.Type.GetDescription()}", Constants.EMERGENCY_CALL_COLOR);
                player.SendMessage(Models.MessageType.None, $"Localização: {emergencyCall.Location}", Constants.EMERGENCY_CALL_COLOR);
                player.SendMessage(Models.MessageType.None, $"Ocorrência: {emergencyCall.Message}", Constants.EMERGENCY_CALL_COLOR);
            }

            try
            {
                var discordChannel = factionType switch
                {
                    FactionType.Police => Global.PoliceEmergencyCallDiscordChannel,
                    FactionType.Firefighter => Global.FirefighterEmergencyCallDiscordChannel,
                    _ => 0u,
                };

                if (discordChannel == 0
                    || Global.DiscordClient is null
                    || Global.DiscordClient.GetChannel(discordChannel) is not SocketTextChannel channel)
                    return;

                var embedBuilder = new EmbedBuilder
                {
                    Title = "CHAMADA EMERGENCIAL",
                    Color = new Color(Global.MainRgba.Red, Global.MainRgba.Green, Global.MainRgba.Blue),
                };
                embedBuilder.AddField("Relator", relator);
                embedBuilder.AddField("Serviço", emergencyCall.Type.GetDescription());
                embedBuilder.AddField("Localização", emergencyCall.Location);
                embedBuilder.AddField("Ocorrência", emergencyCall.Message);
                embedBuilder.WithFooter($"Enviada em {DateTime.Now}.");

                await channel.SendMessageAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Functions.GetException(ex);
            }
        }

        if (emergencyCall.Type == EmergencyCallType.PoliceAndFirefighter || emergencyCall.Type == EmergencyCallType.Police)
            await Send911(FactionType.Police);

        if (emergencyCall.Type == EmergencyCallType.PoliceAndFirefighter || emergencyCall.Type == EmergencyCallType.Firefighter)
            await Send911(FactionType.Firefighter);
    }
}