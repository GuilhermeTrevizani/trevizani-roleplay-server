using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class FactionScript : Script
{
    [Command("f", "/f (mensagem)", GreedyArg = true)]
    public async Task CMD_f(MyPlayer player, string mensagem)
    {
        if (!player.Character.FactionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção.");
            return;
        }

        if (player.Faction!.BlockedChat)
        {
            player.SendMessage(MessageType.Error, "Chat da facção está bloqueado.");
            return;
        }

        if (player.User.FactionChatToggle)
        {
            player.SendMessage(MessageType.Error, "Você está com o chat da facção desabilitado.");
            return;
        }

        if (!player.Faction.HasChat)
        {
            player.SendMessage(MessageType.Error, "Sua facção não possui acesso ao chat.");
            return;
        }

        var message = $"(( {player.FactionRank!.Name} {player.Character.Name} ({player.SessionId}): {mensagem} ))";
        var color = $"#{player.Faction.ChatColor}";
        foreach (var target in Global.SpawnedPlayers.Where(x => x.Character.FactionId == player.Character.FactionId && !x.User.FactionChatToggle))
            target.SendMessage(MessageType.None, message, color);

        await player.WriteLog(LogType.FactionChat, $"{player.Character.FactionId} | {mensagem}", null);
    }

    [Command("blockf")]
    public static void CMD_blockf(MyPlayer player)
    {
        if (!player.FactionFlags.Contains(FactionFlag.BlockChat))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Faction!.ToggleBlockedChat();
        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} {(!player.Faction.BlockedChat ? "des" : string.Empty)}bloqueou o chat da facção.");
    }

    [Command("sairfaccao")]
    public async Task CMD_sairfaccao(MyPlayer player)
    {
        if (!player.Character.FactionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção.");
            return;
        }

        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} saiu da facção.");
        await player.WriteLog(LogType.Faction, $"/sairfaccao {player.Character.FactionId}", null);
        await player.RemoveFromFaction();
        await player.Save();
    }

    [Command("convidar", "/convidar (ID ou nome)", GreedyArg = true)]
    public async Task CMD_convidar(MyPlayer player, string idOrName)
    {
        var faction = player.Faction;
        if ((!player.FactionFlags.Contains(FactionFlag.InviteMember) && !player.IsFactionLeader) || faction is null)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (faction.Slots > 0)
        {
            var context = Functions.GetDatabaseContext();
            var members = await context.Characters.WhereActive(false).CountAsync(x => x.FactionId == faction.Id);
            if (members >= faction.Slots)
            {
                player.SendNotification(NotificationType.Error, $"Facção atingiu o máximo de slots ({faction.Slots}).");
                return;
            }
        }

        var rank = Global.FactionsRanks.Where(x => x.FactionId == faction.Id).MinBy(x => x.Position);
        if (rank is null)
        {
            player.SendNotification(NotificationType.Error, "Facção não possui ranks.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.Character.FactionId.HasValue)
        {
            player.SendNotification(NotificationType.Error, "Jogador já está em uma facção.");
            return;
        }

        var invite = new Invite
        {
            Type = InviteType.Faction,
            SenderCharacterId = player.Character.Id,
            Value = [faction.Id.ToString(), rank.Id.ToString()],
        };
        target.Invites.RemoveAll(x => x.Type == InviteType.Faction);
        target.Invites.Add(invite);

        await player.WriteLog(LogType.Faction, $"Convidar Facção {faction.Name}", target);

        player.SendMessage(MessageType.Success, $"Você convidou {target.Character.Name} para a facção.");
        target.SendMessage(MessageType.Success, $"{player.User.Name} convidou você para {faction.Name}. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
    }

    [RemoteEvent(nameof(CancelDropBarrier))]
    public static void CancelDropBarrier(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.DropBarrierModel = null;
            player.SendMessage(MessageType.Success, "Você cancelou o drop da barreira.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmDropBarrier))]
    public async Task ConfirmDropBarrier(Player playerParam, Vector3 position, Vector3 rotation)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (string.IsNullOrWhiteSpace(player.DropBarrierModel))
            {
                player.SendMessage(MessageType.Error, "Você não está colocando uma barreira.");
                return;
            }

            if (position.X == 0 && position.Y == 0)
            {
                player.SendMessage(MessageType.Error, "Não foi possível recuperar a posição do item.");
                return;
            }

            var barrier = Functions.CreateObject(player.DropBarrierModel, position, rotation, player.GetDimension(), true, true);
            barrier.CharacterId = player.Character.Id;
            barrier.FactionId = player.Character.FactionId;

            await player.WriteLog(LogType.Faction, $"/br {player.DropBarrierModel}", null);
            player.DropBarrierModel = null;
            player.SendMessageToNearbyPlayers($"coloca uma barreira no chão.", MessageCategory.Ame);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("membros")]
    public static void CMD_membros(MyPlayer player)
    {
        if (!player.Character.FactionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção.");
            return;
        }

        player.SendMessage(MessageType.Title, $"{player.Faction!.Name} - Membros online");

        var color = $"#{player.Faction!.ChatColor}";
        foreach (var target in Global.SpawnedPlayers
            .Where(x => x.Character.FactionId == player.Character.FactionId)
            .OrderByDescending(x => x.OnDuty)
            .ThenByDescending(x => x.FactionRank!.Position))
            player.SendMessage(MessageType.None, $"{target.FactionRank!.Name} {target.Character.Name}{(target.User.FactionChatToggle ? " (TOG)" : string.Empty)}", target.OnDuty ? color : "#FFFFFF");
    }
}