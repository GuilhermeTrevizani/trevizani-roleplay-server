using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class ICChatScript : Script
{
    [Command(["me"], "Chat IC", "Interpretação de ações de um personagem", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_me(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.NormalMe);
        await player.WriteLog(LogType.ICChat, $"/me {message}", null);
    }

    [Command(["do"], "Chat IC", "Interpretação do ambiente", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_do(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.NormalDo);
        await player.WriteLog(LogType.ICChat, $"/do {message}", null);
    }

    [Command(["g"], "Chat IC", "Grita", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_g(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.ShoutIC);
        await player.WriteLog(LogType.ICChat, $"/g {message}", null);
    }

    [Command(["baixo"], "Chat IC", "Fala baixo", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_baixo(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.LowIC);
        await player.WriteLog(LogType.ICChat, $"/baixo {message}", null);
    }

    [Command(["s"], "Chat IC", "Sussura", "(ID ou nome) (mensagem)", GreedyArg = true)]
    public static async Task CMD_s(MyPlayer player, string idOrName, string message)
    {
        if (player.Character.Wound != CharacterWound.None)
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotExecuteThisCommandBecauseYouAreSeriouslyInjured);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        message = Functions.CheckFinalDot(message);

        player.SendMessage(MessageType.None, $"{player.ICName} sussurra: {message}", Constants.CELLPHONE_SECONDARY_COLOR);
        target.SendMessage(MessageType.None, $"{player.ICName} sussurra: {message}", Constants.CELLPHONE_MAIN_COLOR);
        player.SendMessageToNearbyPlayers($"sussurra algo para {target.ICName}.", MessageCategory.Ame);
        await player.WriteLog(LogType.ICChat, $"/s {message}", target);
    }

    [Command(["ame"], "Chat IC", "Interpretação de ações de um personagem", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_ame(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.Ame);
        await player.WriteLog(LogType.ICChat, $"/ame {message}", null);
    }

    [Command(["ado"], "Chat IC", "Interpretação do ambiente", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_ado(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.Ado);
        await player.WriteLog(LogType.ICChat, $"/ado {message}", null);
    }

    [Command(["mic"], "Chat IC", "Fala em um microfone", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_mic(MyPlayer player, string message)
    {
        if (!player.Items.Any(x => x.GetCategory() == ItemCategory.Microphone))
        {
            player.SendMessage(MessageType.Error, "Você não possui um microfone.");
            return;
        }

        player.SendMessageToNearbyPlayers(message, MessageCategory.Microphone);
        await player.WriteLog(LogType.ICChat, $"/mic {message}", null);
    }

    [Command(["autobaixo"], "Chat IC", "Ativa/desativa mensagens em tom baixo automaticamente")]
    public static async Task CMD_autobaixo(MyPlayer player)
    {
        player.AutoLow = !player.AutoLow;
        player.SendMessage(MessageType.Success, $"Agora suas mensagens IC estarão em tom {(player.AutoLow ? "baixo" : "normal")}.");
        await player.WriteLog(LogType.ICChat, "/autobaixo", null);
    }

    [Command(["para", "p"], "Chat IC", "Fala destinada para uma pessoa", "(ID ou nome) (mensagem)", GreedyArg = true)]
    public static async Task CMD_para(MyPlayer player, string idOrName, string message)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, 10))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        player.SendMessageToNearbyPlayers(message, MessageCategory.NormalIC, target.ICName);
        await player.WriteLog(LogType.ICChat, $"/para {message}", target);
    }

    [Command(["parabaixo", "pb"], "Chat IC", "Fala baixa destinada para uma pessoa", "(ID ou nome) (mensagem)", GreedyArg = true)]
    public static async Task CMD_parabaixo(MyPlayer player, string idOrName, string message)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, 5))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        player.SendMessageToNearbyPlayers(message, MessageCategory.LowIC, target.ICName);
        await player.WriteLog(LogType.ICChat, $"/parabaixo {message}", target);
    }

    [Command(["paragritar", "pg"], "Chat IC", "Grito destinado para uma pessoa", "(ID ou nome) (mensagem)", GreedyArg = true)]
    public static async Task CMD_paragritar(MyPlayer player, string idOrName, string message)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, 30))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        player.SendMessageToNearbyPlayers(message, MessageCategory.ShoutIC, target.ICName);
        await player.WriteLog(LogType.ICChat, $"/paragritar {message}", target);
    }

    [Command(["mealto", "mea"], "Chat IC", "Interpretação de ações de um personagem com dobro do range normal", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_mealto(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.ShoutMe);
        await player.WriteLog(LogType.ICChat, $"/mealto {message}", null);
    }

    [Command(["doalto", "doa"], "Chat IC", "Interpretação do ambiente com metade do dobro normal", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_doalto(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.ShoutDo);
        await player.WriteLog(LogType.ICChat, $"/doalto {message}", null);
    }

    [Command(["mebaixo", "meb"], "Chat IC", "Interpretação de ações de um personagem com metade do range normal", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_mebaixo(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.LowMe);
        await player.WriteLog(LogType.ICChat, $"/mebaixo {message}", null);
    }

    [Command(["dobaixo", "dob"], "Chat IC", "Interpretação do ambiente com metade do range normal", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_dobaixo(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.LowDo);
        await player.WriteLog(LogType.ICChat, $"/dobaixo {message}", null);
    }

    [Command(["cs", "cw"], "Chat IC", "Sussura para todos no veículo", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_cs(MyPlayer player, string message)
    {
        if (player.Character.Wound != CharacterWound.None)
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotExecuteThisCommandBecauseYouAreSeriouslyInjured);
            return;
        }

        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo.");
            return;
        }

        message = Functions.CheckFinalDot(message);
        message = $"{player.ICName} sussurra (veículo): {message}";

        foreach (var target in Global.SpawnedPlayers.Where(x => x.Vehicle == vehicle))
            target.SendMessage(MessageType.None, message, player == target ? Constants.CELLPHONE_SECONDARY_COLOR : Constants.CELLPHONE_MAIN_COLOR);

        await player.WriteLog(LogType.ICChat, $"/cs {message}", null);
    }
}