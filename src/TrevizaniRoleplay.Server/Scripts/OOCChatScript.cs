using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class OOCChatScript : Script
{
    [Command(["b"], "Chat OOC", "Chat OOC local", "(mensagem)", GreedyArg = true)]
    public static async Task CMD_b(MyPlayer player, string message)
    {
        player.SendMessageToNearbyPlayers(message, MessageCategory.OOC);
        await player.WriteLog(LogType.OOCChat, message, null);
    }

    [Command(["pm"], "Chat OOC", "Envia uma mensagem privada", "(ID ou nome) (mensagem)", GreedyArg = true)]
    public async Task CMD_pm(MyPlayer player, string idOrName, string mesage)
    {
        if (player.User.PMToggle && !player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, "Você está com as mensagens privadas desabilitadas.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
        {
            player.LastPMSessionId = null;
            return;
        }

        if (target.User.PMToggle && !player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, "Jogador está com as mensagens privadas desabilitadas.");
            return;
        }

        var nome = player.OnAdminDuty ?
            $"{{{player.StaffColor}}}{player.User.Name} ({player.SessionId})"
            :
            $"{player.ICName} ({player.SessionId})";

        var nomeTarget = target.OnAdminDuty ?
            $"{{{target.StaffColor}}}{target.User.Name} ({target.SessionId})"
            :
            $"{target.ICName} ({target.SessionId})";

        player.SendMessage(MessageType.None, $"(( PM para {nomeTarget}{{{Constants.CELLPHONE_SECONDARY_COLOR}}}: {mesage} ))", Constants.CELLPHONE_SECONDARY_COLOR);
        target.SendMessage(MessageType.None, $"(( PM de {nome}{{{Constants.CELLPHONE_MAIN_COLOR}}}: {mesage} ))", Constants.CELLPHONE_MAIN_COLOR);
        target.LastPMSessionId = player.SessionId;
        await player.WriteLog(LogType.PrivateMessages, mesage, target);
    }

    [Command(["re"], "Chat OOC", "Responde a última mensagem privada recebida", "(mensagem)", GreedyArg = true)]
    public async Task CMD_re(MyPlayer player, string mesage)
    {
        if (!player.LastPMSessionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você ainda não recebeu mensagens privadas.");
            return;
        }

        await CMD_pm(player, player.LastPMSessionId.ToString()!, mesage);
    }
}