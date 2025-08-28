using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class WalkieTalkieScript : Script
{
    [Command(["canal"], "Rádio Comunicador", "Troca os canais de rádio", "(slot [1-5]) (canal)")]
    public static async Task CMD_canal(MyPlayer player, int slot, int channel)
    {
        var item = player.Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.WalkieTalkie && x.InUse);
        if (item is null)
        {
            player.SendMessage(MessageType.Error, "Você não possui um rádio comunicador equipado.");
            return;
        }

        if (slot < 1 || slot > 5)
        {
            player.SendMessage(MessageType.Error, "Slot deve ser entre 1 e 5.");
            return;
        }

        if (channel < 0)
        {
            player.SendMessage(MessageType.Error, "Canal inválido.");
            return;
        }

        var frequency = Global.FactionsFrequencies.FirstOrDefault(x => x.Frequency == channel);
        if (frequency is not null && frequency.FactionId != player.Character.FactionId)
        {
            player.SendMessage(MessageType.Error, $"Você não possui acesso ao canal {channel}.");
            return;
        }

        var extra = Functions.Deserialize<WalkieTalkieItem>(item.Extra!);

        if (slot == 1)
            player.WalkieTalkieItem.Channel1 = extra.Channel1 = channel;
        else if (slot == 2)
            player.WalkieTalkieItem.Channel2 = extra.Channel2 = channel;
        else if (slot == 3)
            player.WalkieTalkieItem.Channel3 = extra.Channel3 = channel;
        else if (slot == 4)
            player.WalkieTalkieItem.Channel4 = extra.Channel4 = channel;
        else if (slot == 5)
            player.WalkieTalkieItem.Channel5 = extra.Channel5 = channel;

        item.SetExtra(Functions.Serialize(extra));
        var context = Functions.GetDatabaseContext();
        context.CharactersItems.Update(item);
        await context.SaveChangesAsync();

        if (channel == 0)
            player.SendMessage(MessageType.Success, $"Você desativou seu canal de rádio do slot {slot}.");
        else
            player.SendMessage(MessageType.Success, $"Você alterou seu canal de rádio do slot {slot} para {channel}.");
    }

    [Command(["r"], "Rádio Comunicador", "Fala no canal de rádio 1", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(1, message, MessageCategory.NormalIC))
            await player.WriteLog(LogType.ICChat, $"/r {message}", null);
    }

    [Command(["r2"], "Rádio Comunicador", "Fala no canal de rádio 2", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r2(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(2, message, MessageCategory.NormalIC))
            await player.WriteLog(LogType.ICChat, $"/r2 {message}", null);
    }

    [Command(["r3"], "Rádio Comunicador", "Fala no canal de rádio 3", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r3(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(3, message, MessageCategory.NormalIC))
            await player.WriteLog(LogType.ICChat, $"/r3 {message}", null);
    }

    [Command(["r4"], "Rádio Comunicador", "Fala no canal de rádio 4", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r4(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(4, message, MessageCategory.NormalIC))
            await player.WriteLog(LogType.ICChat, $"/r4 {message}", null);
    }

    [Command(["r5"], "Rádio Comunicador", "Fala no canal de rádio 5", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r5(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(5, message, MessageCategory.NormalIC))
            await player.WriteLog(LogType.ICChat, $"/r5 {message}", null);
    }

    [Command(["rbaixo"], "Rádio Comunicador", "Fala baixo no canal de rádio 1", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_rbaixo(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(1, message, MessageCategory.LowIC))
            await player.WriteLog(LogType.ICChat, $"/rbaixo {message}", null);
    }

    [Command(["r2baixo"], "Rádio Comunicador", "Fala baixo no canal de rádio 2", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r2baixo(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(2, message, MessageCategory.LowIC))
            await player.WriteLog(LogType.ICChat, $"/r2baixo {message}", null);
    }

    [Command(["r3baixo"], "Rádio Comunicador", "Fala baixo no canal de rádio 3", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r3baixo(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(3, message, MessageCategory.LowIC))
            await player.WriteLog(LogType.ICChat, $"/r3baixo {message}", null);
    }

    [Command(["r4baixo"], "Rádio Comunicador", "Fala baixo no canal de rádio 4", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r4baixo(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(4, message, MessageCategory.LowIC))
            await player.WriteLog(LogType.ICChat, $"/r4baixo {message}", null);
    }

    [Command(["r5baixo"], "Rádio Comunicador", "Fala baixo no canal de rádio 5", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r5baixo(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(5, message, MessageCategory.LowIC))
            await player.WriteLog(LogType.ICChat, $"/r5baixo {message}", null);
    }

    [Command(["rme"], "Rádio Comunicador", "Interpretação de ações de um personagem no canal de rádio 1", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_rme(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(1, message, MessageCategory.NormalMe))
            await player.WriteLog(LogType.ICChat, $"/rme {message}", null);
    }

    [Command(["r2me"], "Rádio Comunicador", "Interpretação de ações de um personagem no canal de rádio 2", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r2me(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(2, message, MessageCategory.NormalMe))
            await player.WriteLog(LogType.ICChat, $"/r2me {message}", null);
    }

    [Command(["r3me"], "Rádio Comunicador", "Interpretação de ações de um personagem no canal de rádio 3", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r3me(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(3, message, MessageCategory.NormalMe))
            await player.WriteLog(LogType.ICChat, $"/r3me {message}", null);
    }

    [Command(["r4me"], "Rádio Comunicador", "Interpretação de ações de um personagem no canal de rádio 4", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r4me(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(4, message, MessageCategory.NormalMe))
            await player.WriteLog(LogType.ICChat, $"/r4me {message}", null);
    }

    [Command(["r5me"], "Rádio Comunicador", "Interpretação de ações de um personagem no canal de rádio 5", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r5me(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(5, message, MessageCategory.NormalMe))
            await player.WriteLog(LogType.ICChat, $"/r5me {message}", null);
    }

    [Command(["rdo"], "Rádio Comunicador", "Interpretação do ambiente no canal de rádio 1", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_rdo(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(1, message, MessageCategory.NormalDo))
            await player.WriteLog(LogType.ICChat, $"/rdo {message}", null);
    }

    [Command(["r2do"], "Rádio Comunicador", "Interpretação do ambiente no canal de rádio 2", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r2do(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(2, message, MessageCategory.NormalDo))
            await player.WriteLog(LogType.ICChat, $"/r2do {message}", null);
    }

    [Command(["r3do"], "Rádio Comunicador", "Interpretação do ambiente no canal de rádio 3", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r3do(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(3, message, MessageCategory.NormalDo))
            await player.WriteLog(LogType.ICChat, $"/r3do {message}", null);
    }

    [Command(["r4do"], "Rádio Comunicador", "Interpretação do ambiente no canal de rádio 4", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r4do(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(4, message, MessageCategory.NormalDo))
            await player.WriteLog(LogType.ICChat, $"/r4do {message}", null);
    }

    [Command(["r5do"], "Rádio Comunicador", "Interpretação do ambiente no canal de rádio 5", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_r5do(MyPlayer player, string message)
    {
        if (player.SendWalkieTalkieMessage(5, message, MessageCategory.NormalDo))
            await player.WriteLog(LogType.ICChat, $"/r5do {message}", null);
    }
}