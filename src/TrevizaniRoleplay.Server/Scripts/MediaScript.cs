using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class MediaScript : Script
{
    [Command("transmissao")]
    public async Task CMD_transmissao(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Media)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção de mídia.");
            return;
        }

        Global.TransmissionActive = !Global.TransmissionActive;

        if (Global.TransmissionActive)
        {
            foreach (var target in Global.SpawnedPlayers)
            {
                target.CanTalkInTransmission = false;
                target.FollowingTransmission = true;
                target.SendMessage(MessageType.None, $"[{player.Faction.Name}] Uma transmissão foi iniciada.", $"#{player.Faction.Color}");
            }
            player.CanTalkInTransmission = player.FollowingTransmission = true;
        }
        else
        {
            foreach (var target in Global.SpawnedPlayers)
                target.CanTalkInTransmission = target.FollowingTransmission = false;
            player.SendMessage(MessageType.Success, "Você parou a transmissão.");
        }

        await player.WriteLog(LogType.Faction, $"/transmissao {Global.TransmissionActive}", null);
    }

    [Command("vertransmissao")]
    public static void CMD_vertransmissao(MyPlayer player)
    {
        if (!Global.TransmissionActive)
        {
            player.SendMessage(MessageType.Error, "Não há transmissão ativa.");
            return;
        }

        player.FollowingTransmission = !player.FollowingTransmission;
        player.SendMessage(MessageType.Success, $"Você {(player.FollowingTransmission ? "agora está" : "não está mais")} acompanhando a transmissão.");
    }

    [Command("t", "/t (mensagem)", GreedyArg = true)]
    public async Task CMD_t(MyPlayer player, string message)
    {
        if (!Global.TransmissionActive)
        {
            player.SendMessage(MessageType.Error, "Não há transmissão ativa.");
            return;
        }

        if (!player.CanTalkInTransmission)
        {
            player.SendMessage(MessageType.Error, "Você não pode falar em uma transmissão.");
            return;
        }

        message = Functions.CheckFinalDot(message);
        var senderMessage = $"[TRANSMISSÃO] {player.ICName}: {message}";

        foreach (var target in Global.SpawnedPlayers.Where(x => x.FollowingTransmission || x.CanTalkInTransmission))
            target.SendMessage(MessageType.None, senderMessage, Constants.CELLPHONE_MAIN_COLOR);

        await player.WriteLog(LogType.General, $"/t {message}", null);
    }

    [Command("tplayer", "/tplayer (ID ou nome)")]
    public async Task CMD_tplayer(MyPlayer player, string idOrName)
    {
        if (player.Faction?.Type != FactionType.Media)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção de mídia.");
            return;
        }

        if (!Global.TransmissionActive)
        {
            player.SendMessage(MessageType.Error, "Não há transmissão ativa.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        target.CanTalkInTransmission = !target.CanTalkInTransmission;
        player.SendMessage(MessageType.Success, $"Você {(target.CanTalkInTransmission ? "adicionou" : "removeu")} a permissão de falar em uma transmissão para {target.ICName}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} {(target.CanTalkInTransmission ? "adicionou" : "removeu")} a permissão de falar em uma transmissão para você.{(target.CanTalkInTransmission ? " Use /t." : string.Empty)}");
        await player.WriteLog(LogType.General, $"/tplayer {target.CanTalkInTransmission}", target);
    }
}