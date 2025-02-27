using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extesions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffFactionScript : Script
{
    [Command("faccoes")]
    public async Task CMD_faccoes(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Factions))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        var jsonTypes = Functions.Serialize(
            Enum.GetValues<FactionType>()
            .Select(x => new
            {
                Value = x,
                Label = x.GetDisplay(),
            })
        );

        player.Emit("StaffFaction:Show", await GetFactionsJson(), jsonTypes);
    }

    [RemoteEvent(nameof(StaffFactionSave))]
    public async Task StaffFactionSave(Player playerParam, string idString, string name, int type, int slots, string leaderName, string shortName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                player.SendNotification(NotificationType.Error, "Nome não preenchido.");
                return;
            }

            if (string.IsNullOrWhiteSpace(shortName))
            {
                player.SendNotification(NotificationType.Error, "Acrônimo não preenchido.");
                return;
            }

            if (!Enum.IsDefined(typeof(FactionType), Convert.ToByte(type)))
            {
                player.SendNotification(NotificationType.Error, "Tipo inválido.");
                return;
            }

            if (slots < 0)
            {
                player.SendNotification(NotificationType.Error, "Slots deve ser maior ou igual a zero.");
                return;
            }

            var id = idString.ToGuid();

            if (Global.Factions.Any(x => x.Id != id && x.Name.ToLower() == name.ToLower()))
            {
                player.SendNotification(NotificationType.Error, "Já existe uma facção com esse nome.");
                return;
            }

            if (Global.Factions.Any(x => x.Id != id && x.ShortName.ToLower() == shortName.ToLower()))
            {
                player.SendNotification(NotificationType.Error, "Já existe uma facção com esse acrônimo.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            Character? leader = null;
            if (!string.IsNullOrWhiteSpace(leaderName))
            {
                leader = await context.Characters.FirstOrDefaultAsync(x => x.Name == leaderName);
                if (leader is null)
                {
                    player.SendNotification(NotificationType.Error, $"Personagem {leaderName} não encontrado.");
                    return;
                }

                if (leader.FactionId.HasValue && leader.FactionId != id)
                {
                    player.SendNotification(NotificationType.Error, $"{leader.Name} já está em uma facção.");
                    return;
                }
            }

            var isNew = string.IsNullOrWhiteSpace(idString);
            var faction = new Faction();
            if (isNew)
            {
                faction.Create(name, (FactionType)type, slots, leader?.Id, shortName);
            }
            else
            {
                faction = Global.Factions.FirstOrDefault(x => x.Id == id);
                if (faction is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                if (faction.CharacterId.HasValue && faction.CharacterId != leader?.Id)
                {
                    var oldLeader = await context.Characters.FirstOrDefaultAsync(x => x.Id == faction.CharacterId)!;
                    var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == oldLeader!.Id);
                    if (target is not null)
                    {
                        await target.RemoveFromFaction();
                    }
                    else
                    {
                        oldLeader!.ResetFaction();
                        context.Characters.Update(oldLeader);

                        if (faction.HasDuty)
                        {
                            var items = await context.CharactersItems.Where(x => x.CharacterId == oldLeader.Id).ToListAsync();
                            items = items.Where(x => !Functions.CanDropItem(faction, x)).ToList();
                            if (items.Count > 0)
                                context.CharactersItems.RemoveRange(items);
                        }

                        await context.SaveChangesAsync();
                    }
                }

                faction.Update(name, (FactionType)type, slots, leader?.Id, shortName);
            }

            if (isNew)
                await context.Factions.AddAsync(faction);
            else
                context.Factions.Update(faction);

            await context.SaveChangesAsync();

            if (isNew)
                Global.Factions.Add(faction);

            if (leader is not null && leader.FactionId != faction.Id)
            {
                var lastRank = Global.FactionsRanks
                    .Where(x => x.FactionId == faction.Id)
                    .OrderByDescending(x => x.Position)
                    .FirstOrDefault();
                if (lastRank is null)
                {
                    lastRank = new();
                    lastRank.Create(faction.Id, 1, "Rank 1", 0);
                    await context.FactionsRanks.AddAsync(lastRank);
                    await context.SaveChangesAsync();
                    Global.FactionsRanks.Add(lastRank);
                }

                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == leader.Id);
                if (target is not null)
                {
                    target.Character.SetFaction(faction.Id, lastRank.Id, faction.Type == FactionType.Criminal);
                    await target.Save();
                }
                else
                {
                    leader.SetFaction(faction.Id, lastRank.Id, faction.Type == FactionType.Criminal);
                    context.Characters.Update(leader);
                    await context.SaveChangesAsync();
                }
            }

            player.SendNotification(NotificationType.Success, $"Facção {(isNew ? "criada" : "editada")}.");

            await player.WriteLog(LogType.Staff, $"Gravar Facção | {Functions.Serialize(faction)}", null);

            var json = await GetFactionsJson();
            foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Factions)))
                target.Emit("StaffFaction:Update", json);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionShowRanks))]
    public static void StaffFactionShowRanks(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            var json = GetFactionRanksJson(id!.Value);
            player.Emit("StaffFactionRank:Show", json, Global.Factions.FirstOrDefault(x => x.Id == id)!.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionRankSave))]
    public async Task StaffFactionRankSave(Player playerParam, string factionRankIdString, int salary)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (salary < 0)
            {
                player.SendNotification(NotificationType.Error, "Salário deve ser maior ou igual a 0.");
                return;
            }

            var factionRankId = factionRankIdString.ToGuid();
            var factionRank = Global.FactionsRanks.FirstOrDefault(x => x.Id == factionRankId);
            if (factionRank is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            factionRank.Update(salary);

            var context = Functions.GetDatabaseContext();
            context.FactionsRanks.Update(factionRank);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Gravar Rank | {Functions.Serialize(factionRank)}", null);
            player.SendNotification(NotificationType.Success, "Rank editado.");

            var json = GetFactionRanksJson(factionRank.FactionId);
            foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Factions)))
                target.Emit("StaffFactionRank:Update", json);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionShowMembers))]
    public async Task StaffFactionShowMembers(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid()!;

            var faction = Global.Factions.FirstOrDefault(x => x.Id == id)!;

            var json = await GetFactionMembersJson(id.Value);
            player.Emit("StaffFactionMember:Show", json, faction.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task<string> GetFactionsJson()
    {
        var context = Functions.GetDatabaseContext();
        var characters = await context.Characters.Where(x => Global.Factions.Select(y => y.CharacterId).Contains(x.Id)).ToListAsync();

        string? GetLeader(Guid? characterId)
        {
            return characters.FirstOrDefault(x => x.Id == characterId)?.Name;
        }

        return Functions.Serialize(Global.Factions.OrderByDescending(x => x.RegisterDate).Select(x => new
        {
            x.Id,
            x.Name,
            x.Type,
            x.Slots,
            TypeDisplay = x.Type.GetDisplay(),
            Leader = GetLeader(x.CharacterId),
            x.ShortName,
        }));
    }

    private static string GetFactionRanksJson(Guid factionId)
    {
        return Functions.Serialize(Global.FactionsRanks.Where(x => x.FactionId == factionId).OrderBy(x => x.Position).Select(x => new
        {
            x.Id,
            x.Name,
            x.Salary
        }));
    }

    private async Task<string> GetFactionMembersJson(Guid factionId)
    {
        var context = Functions.GetDatabaseContext();
        return Functions.Serialize((await Functions.GetActiveCharacters()
                .Where(x => x.FactionId == factionId)
                .Include(x => x.User)
                .Include(x => x.FactionRank)
                .ToListAsync())
                .Select(x => new
                {
                    RankName = x.FactionRank!.Name,
                    x.Name,
                    User = x.User!.Name,
                    x.LastAccessDate,
                    x.FactionRank.Position,
                    IsOnline = Global.SpawnedPlayers.Any(y => y.Character.Id == x.Id),
                })
                .OrderByDescending(x => x.IsOnline)
                    .ThenByDescending(x => x.Position)
                        .ThenBy(x => x.Name));
    }

    [Command("setfaccao", "/setfaccao (ID ou nome) (nome da facção)", GreedyArg = true)]
    public async Task CMD_setfaccao(MyPlayer player, string idOrName, string factionName)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Factions))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (target.Character.FactionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Jogador já está em uma facção.");
            return;
        }

        var faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == factionName.ToLower());
        if (faction is null)
        {
            player.SendMessage(MessageType.Error, $"Facção {factionName} não existe.");
            return;
        }

        var rank = Global.FactionsRanks.Where(x => x.FactionId == faction.Id).MinBy(x => x.Position);
        if (rank is null)
        {
            player.SendMessage(MessageType.Error, $"Facção {factionName} não possui ranks.");
            return;
        }

        target.Character.SetFaction(faction.Id, rank.Id, faction.Type == FactionType.Criminal);

        if (faction.Type != FactionType.Criminal)
            target.OnDuty = false;

        target.SendFactionMessage($"{target.Character.Name} entrou na facção.");
        await player.Save();
        await player.WriteLog(LogType.Staff, $"/setfaccao {factionName}", target);
        await Functions.SendServerMessage($"{player.User.Name} setou {target.Character.Name} na facção {faction.Name}.", UserStaff.JuniorServerAdmin, false);
    }
}