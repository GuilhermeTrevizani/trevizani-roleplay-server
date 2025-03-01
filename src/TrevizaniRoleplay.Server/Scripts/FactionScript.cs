using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Server.Extensions;
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

    [Command("faccao")]
    public async Task CMD_faccao(MyPlayer player)
    {
        if (!player.Character.FactionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção.");
            return;
        }

        await ShowFaction(player);
    }

    private async Task ShowFaction(MyPlayer player)
    {
        var ranksJson = Functions.Serialize(
            Global.FactionsRanks
            .Where(x => x.FactionId == player.Character.FactionId)
            .OrderBy(x => x.Position)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Salary
            })
        );

        var flagsJson = Functions.Serialize(
            player.Faction!.GetFlags()
            .Select(x => new
            {
                Value = x,
                Label = x.GetDescription(),
            })
        );

        const int DAYS_INTERVAL_ON_DUTY_SESSION_AVERAGE = 14;
        dynamic GetCharacterInfo(Character x)
        {
            var spawnedPlayer = Global.SpawnedPlayers.FirstOrDefault(y => y.Character.Id == x.Id);

            return new
            {
                x.Id,
                RankName = x.FactionRank!.Name,
                RankId = x.FactionRankId,
                x.Name,
                User = x.User!.Name,
                x.LastAccessDate,
                x.FactionRank.Position,
                x.Badge,
                IsOnline = spawnedPlayer is not null,
                IsOnDuty = spawnedPlayer?.OnDuty ?? false,
                FlagsJson = x.FactionFlagsJSON,
                AverageMinutesOnDutyLastTwoWeeks = x.Sessions
                    !.Where(y => y.Type == SessionType.FactionDuty && y.FinalDate.HasValue
                        && y.RegisterDate >= DateTime.Now.AddDays(-DAYS_INTERVAL_ON_DUTY_SESSION_AVERAGE))
                    .Sum(y => (y.FinalDate!.Value - y.RegisterDate).TotalMinutes) / DAYS_INTERVAL_ON_DUTY_SESSION_AVERAGE,
                Flags = Functions.Deserialize<FactionFlag[]>(x.FactionFlagsJSON).Select(x => x.GetDescription()).Order(),
            };
        }

        var context = Functions.GetDatabaseContext();
        var membersJson = Functions.Serialize((await Functions.GetActiveCharacters()
            .Where(x => x.FactionId == player.Character.FactionId)
            .Include(x => x.User)
            .Include(x => x.FactionRank)
            .Include(x => x.Sessions)
            .AsSplitQuery()
            .ToListAsync())
            .Select(GetCharacterInfo)
            .OrderByDescending(x => x.IsOnline)
                .ThenByDescending(x => x.IsOnDuty)
                    .ThenByDescending(x => x.Position)
                        .ThenBy(x => x.Name));

        var vehiclesJson = Functions.Serialize((await context.Vehicles
            .Where(x => x.FactionId == player.Character.FactionId && !x.Sold)
            .ToListAsync())
            .Select(x => new
            {
                x.Id,
                Name = NAPI.Vehicle.GetVehicleDisplayName(Functions.Hash(x.Model)),
                Model = x.Model.ToUpper(),
                x.Plate,
                x.Description,
            }));

        player.Emit("ShowFaction", ranksJson, membersJson, flagsJson,
            player.Faction.Id.ToString(), player.Faction.Name, player.Faction.HasDuty,
            player.IsFactionLeader,
            player.IsFactionLeader ? Functions.Serialize(player.Faction!.GetFlags()) : player.Character.FactionFlagsJSON,
            player.Faction.Color, player.Faction.ChatColor, vehiclesJson);
    }

    [Command("sairfaccao")]
    public async Task CMD_sairfaccao(MyPlayer player)
    {
        if (!player.Character.FactionId.HasValue)
        {
            player.SendNotification(NotificationType.Error, "Você não está em uma facção.");
            return;
        }

        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} saiu da facção.");
        await player.WriteLog(LogType.Faction, $"/sairfaccao {player.Character.FactionId}", null);
        await player.RemoveFromFaction();
        await player.Save();
    }

    [RemoteEvent(nameof(FactionRankSave))]
    public async Task FactionRankSave(Player playerParam, string factionIdString, string factionRankIdString, string name)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var factionId = new Guid(factionIdString);
            if (!player.IsFactionLeader || player.Character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var factionRankId = factionRankIdString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(factionRankIdString);
            var factionRank = new FactionRank();
            if (isNew)
            {
                factionRank.Create(factionId,
                    Global.FactionsRanks.Where(x => x.FactionId == factionId).Select(x => x.Position).DefaultIfEmpty(0).Max() + 1, name, 0);
            }
            else
            {
                factionRank = Global.FactionsRanks.FirstOrDefault(x => x.Id == factionRankId);
                if (factionRank == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                factionRank.Update(name);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.FactionsRanks.AddAsync(factionRank);
            else
                context.FactionsRanks.Update(factionRank);

            await context.SaveChangesAsync();

            if (isNew)
                Global.FactionsRanks.Add(factionRank);

            await player.WriteLog(LogType.Faction, $"Gravar Rank | {Functions.Serialize(factionRank)}", null);
            player.SendNotification(NotificationType.Success, $"Rank {(isNew ? "criado" : "editado")}.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FactionRankRemove))]
    public async Task FactionRankRemove(Player playerParam, string factionIdString, string factionRankIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var factionId = new Guid(factionIdString);
            if (!player.IsFactionLeader || player.Character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var factionRankId = new Guid(factionRankIdString);
            var factionRank = Global.FactionsRanks.FirstOrDefault(x => x.Id == factionRankId);
            if (factionRank == null)
                return;

            var context = Functions.GetDatabaseContext();
            if (await context.Characters.AnyAsync(x => x.FactionRankId == factionRankId))
            {
                player.SendNotification(NotificationType.Error, $"Não é possível remover o rank {factionRank.Name} pois existem personagens nele.");
                return;
            }

            context.FactionsRanks.Remove(factionRank);
            await context.SaveChangesAsync();
            Global.FactionsRanks.Remove(factionRank);

            await player.WriteLog(LogType.Faction, $"Remover Rank | {Functions.Serialize(factionRank)}", null);
            player.SendNotification(NotificationType.Success, $"Rank {factionRank.Name} excluído.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FactionRankOrder))]
    public async Task FactionRankOrder(Player playerParam, string factionIdString, string ranksJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var factionId = new Guid(factionIdString);
            if (!player.IsFactionLeader || player.Character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var ranks = Functions.Deserialize<List<FactionRank>>(ranksJSON);
            var factionRanks = Global.FactionsRanks.Where(x => x.FactionId == factionId);
            foreach (var rank in ranks)
            {
                var factionRank = factionRanks.FirstOrDefault(x => x.Id == rank.Id);
                factionRank?.SetPosition(rank.Position);
            }

            var context = Functions.GetDatabaseContext();
            context.FactionsRanks.UpdateRange(factionRanks);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Faction, $"Ordenar Ranks | {ranksJSON}", null);
            player.SendNotification(NotificationType.Success, "Ranks ordenados.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
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
            var members = await Functions.GetActiveCharacters().CountAsync(x => x.FactionId == faction.Id);
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

        player.SendNotification(NotificationType.Success, $"Você convidou {target.Character.Name} para a facção.");
        target.SendMessage(MessageType.Success, $"{player.User.Name} convidou você para {faction.Name}. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
    }

    [RemoteEvent(nameof(FactionMemberSave))]
    public async Task FactionMemberSave(Player playerParam, string factionIdString, string characterIdString, string factionRankIdString,
        int badge, string flagsJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var factionId = new Guid(factionIdString);
            if ((!player.FactionFlags.Contains(FactionFlag.EditMember) && !player.IsFactionLeader) || player.Character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var characterId = new Guid(characterIdString);
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == characterId);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhum jogador encontrado com o ID {characterId}.");
                return;
            }

            if (character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, $"Jogador não pertence a esta facção.");
                return;
            }

            var factionRankId = new Guid(factionRankIdString);
            var rank = Global.FactionsRanks.FirstOrDefault(x => x.Id == factionRankId);
            if (rank?.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, $"Rank {factionRankId} não existe na facção {factionId}.");
                return;
            }

            if (badge < 0)
            {
                player.SendNotification(NotificationType.Error, $"Distintivo deve ser maior ou igual a zero.");
                return;
            }

            if (badge > 0)
            {
                var characterTarget = await context.Characters.FirstOrDefaultAsync(x => x.FactionId == factionId
                    && x.Badge == badge
                    && !x.DeathDate.HasValue
                    && !x.DeletedDate.HasValue);
                if (characterTarget != null && characterTarget.Id != character.Id)
                {
                    player.SendNotification(NotificationType.Error, $"Distintivo {badge} está sendo usado por {characterTarget.Name}.");
                    return;
                }
            }

            if (character.FactionRank?.Position >= player.Character.FactionRank?.Position
                && character.Id != player.Character.Id)
            {
                player.SendNotification(NotificationType.Error, "Jogador possui um rank igual ou maior que o seu.");
                return;
            }

            var factionFlags = Functions.Deserialize<List<FactionFlag>>(flagsJSON);

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is not null)
            {
                target.Character.UpdateFaction(factionRankId, Functions.Serialize(factionFlags), badge);
                target.FactionFlags = factionFlags;
                target.SendMessage(MessageType.Success, $"{player.User.Name} alterou suas informações na facção.");
                await target.Save();
            }
            else
            {
                character.UpdateFaction(factionRankId, Functions.Serialize(factionFlags), badge);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }

            await player.WriteLog(LogType.Faction, $"Salvar Membro Facção {factionId} {characterId} {factionRankId} {badge} {flagsJSON}", target);
            player.SendNotification(NotificationType.Success, $"Você alterou as informações de {character.Name} na facção.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FactionMemberRemove))]
    public async Task FactionMemberRemove(Player playerParam, string factionIdString, string characterIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var factionId = new Guid(factionIdString);
            if ((!player.FactionFlags.Contains(FactionFlag.RemoveMember) && !player.IsFactionLeader) || player.Character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var characterId = new Guid(characterIdString);
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == characterId);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhum jogador encontrado com o ID {characterId}.");
                return;
            }

            if (character.FactionId != factionId)
            {
                player.SendNotification(NotificationType.Error, "Jogador não pertence a esta facção.");
                return;
            }

            var factionRank = Global.FactionsRanks.FirstOrDefault(x => x.Id == character.FactionRankId);
            if (factionRank is null
                || factionRank.Position >= player.FactionRank?.Position)
            {
                player.SendNotification(NotificationType.Error, "Jogador possui um rank igual ou maior que o seu.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is not null)
            {
                await target.RemoveFromFaction();
                await target.Save();
                target.SendMessage(MessageType.Success, $"{player.User.Name} expulsou você da facção.");
            }
            else
            {
                var faction = Global.Factions.FirstOrDefault(x => x.Id == factionId);
                if (faction?.HasDuty ?? false)
                {
                    var items = await context.CharactersItems.Where(x => x.CharacterId == character.Id).ToListAsync();
                    items = items.Where(x => !Functions.CanDropItem(faction, x)).ToList();
                    if (items.Count > 0)
                        context.CharactersItems.RemoveRange(items);
                }

                character.ResetFaction();

                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }

            await player.WriteLog(LogType.Faction, $"Expulsar Facção {factionId} {characterId}", target);
            player.SendNotification(NotificationType.Success, $"Você expulsou {character.Name} da facção.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
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

    [RemoteEvent(nameof(FactionSave))]
    public async Task FactionSave(Player playerParam, string idString, string color, string chatColor)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = new Guid(idString);
            var faction = Global.Factions.FirstOrDefault(x => x.Id == id);
            if (!player.IsFactionLeader || player.Character.FactionId != id || faction is null)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            faction.UpdateColors(color.Replace("#", string.Empty), chatColor.Replace("#", string.Empty));
            var context = Functions.GetDatabaseContext();
            context.Factions.Update(faction);

            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Faction, $"Gravar Facção | {Functions.Serialize(faction)}", null);
            player.SendNotification(NotificationType.Success, "Facção editada.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FactionVehicleSave))]
    public async Task FactionVehicleSave(Player playerParam, string idString, string description)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.FactionFlags.Contains(FactionFlag.ManageVehicles))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == id && x.FactionId == player.Character.FactionId);
            if (vehicle is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (Global.Vehicles.Any(x => x.VehicleDB.Id == vehicle.Id))
            {
                player.SendNotification(NotificationType.Error, "Veículo está spawnado.");
                return;
            }

            if (description.Length > 100)
            {
                player.SendNotification(NotificationType.Error, "Descrição deve ter no máximo 100 caracteres.");
                return;
            }

            vehicle.SetDescription(description);
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Faction, $"Editar Veículo | {vehicle.Id} {vehicle.Description}", null);
            player.SendNotification(NotificationType.Success, "Veículo modificado.");
            await ShowFaction(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}