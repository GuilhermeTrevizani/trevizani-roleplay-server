using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.Extensions.DependencyInjection;

namespace TrevizaniRoleplay.Server.DiscordBOT;

public class Main
{
    public static async Task MainAsync(string token)
    {
        try
        {
            using var services = ConfigureServices();
            Global.DiscordClient = services.GetRequiredService<DiscordSocketClient>();

            Global.DiscordClient.Log += LogAsync;

            await Global.DiscordClient.LoginAsync(TokenType.Bot, token);
            await Global.DiscordClient.StartAsync();
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Functions.ConsoleLog(ex.Message);
        }
    }

    private static Task LogAsync(LogMessage log)
    {
        Functions.ConsoleLog(log.ToString());
        return Task.CompletedTask;
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new()
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages,
            }))
            .AddSingleton<CommandService>()
            .AddSingleton<HttpClient>()
            .BuildServiceProvider();
    }
}