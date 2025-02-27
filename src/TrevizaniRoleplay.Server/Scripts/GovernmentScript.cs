using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class GovernmentScript : Script
{
    [Command("meg", "/meg (mensagem)", GreedyArg = true)]
    public static void CMD_meg(MyPlayer player, string message)
    {
        if (!(player.Faction?.Government ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma governamental ou não está em serviço.");
            return;
        }

        player.SendMessageToNearbyPlayers(message, MessageCategory.Megaphone);
    }

    [Command("gov", "/gov (mensagem)", GreedyArg = true)]
    public async Task CMD_gov(MyPlayer player, string message)
    {
        if (!(player.Faction?.HasGovernmentAdvertisement ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma governamental ou não está em serviço.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.GovernmentAdvertisement))
        {
            player.SendMessage(Models.MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        message = Functions.CheckFinalDot(message);
        foreach (var target in Global.SpawnedPlayers.Where(x => !x.User.AnnouncementToggle))
        {
            target.SendMessage(Models.MessageType.None, $"[{player.Faction.Name}] {message}", $"#{player.Faction.Color}");

            if (target.User.Staff != UserStaff.None)
                target.SendMessage(Models.MessageType.None, $"{player.Character.Name} ({player.SessionId}) ({player.User.Name}) enviou o anúncio governamental.", Constants.STAFF_COLOR);
        }

        Global.Announcements.Add(new()
        {
            Type = AnnouncementType.Government,
            Sender = player.Faction.Name,
            Message = message,
        });

        await player.WriteLog(LogType.GovernmentAdvertisement, message, null);

        if (Global.DiscordClient is null
            || Global.DiscordClient.GetChannel(Global.GovernmentAnnouncementDiscordChannel) is not SocketTextChannel channel)
            return;

        var cor = ColorTranslator.FromHtml($"#{player.Faction.Color}");
        var embedBuilder = new EmbedBuilder
        {
            Title = player.Faction.Name,
            Description = message,
            Color = new Discord.Color(cor.R, cor.G, cor.B),
        };
        embedBuilder.WithFooter($"Enviado em {DateTime.Now}.");

        await channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    [RemoteEvent(nameof(SpawnFactionVehicle))]
    public async Task SpawnFactionVehicle(Player playerParam, string vehicleIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var vehicleId = new Guid(vehicleIdString);
            if (Global.Vehicles.Any(x => x.VehicleDB.Id == vehicleId))
            {
                player.SendNotification(NotificationType.Error, "Veículo já está spawnado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var veh = await context.Vehicles
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == vehicleId);
            if (veh is null)
            {
                player.SendNotification(NotificationType.Error, "Veículo não encontrado.");
                return;
            }

            player.Emit(Constants.FACTION_VEHICLES_PAGE_CLOSE);
            veh.ChangePosition(player.GetPosition().X, player.GetPosition().Y, player.GetPosition().Z,
                0, 0, player.GetRotation().Z, player.GetDimension());

            var vehicle = await veh.Spawnar(player);
            vehicle.NameInCharge = player.Character.Name;
            vehicle.SetLocked(false);
            player.SetIntoVehicleEx(vehicle, Constants.VEHICLE_SEAT_DRIVER);
            await Task.Delay(500);
            vehicle.SetEngineStatus(true);
            player.SendNotification(NotificationType.Success, $"Você spawnou {vehicle.Identifier} ({vehicle.Id}).");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("uniforme")]
    public static void CMD_uniforme(MyPlayer player)
    {
        if (!(player.Faction?.HasDuty ?? false))
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental.");
            return;
        }

        player.EditOutfits(2);
    }

    [Command("freparar")]
    public async Task CMD_freparar(MyPlayer player)
    {
        if (!(player.Faction?.HasDuty ?? false))
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção habilitada.");
            return;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x == player.Vehicle && x.Driver == player && x.VehicleDB.FactionId == player.Character.FactionId);
        if (vehicle is null)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está dirigindo um veículo que pertence a sua facção.");
            return;
        }

        var spot = Global.Spots.FirstOrDefault(x => x.Type == SpotType.FactionVehicleSpawn
            && x.Dimension == player.GetDimension()
            && vehicle.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (spot is null)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está próximo de nenhum ponto de spawn de veículos da facção.");
            return;
        }

        player.ToggleGameControls(false);
        player.SendMessage(Models.MessageType.Success, $"Aguarde 5 segundos. Pressione DELETE para cancelar a ação.");
        player.CancellationTokenSourceAcao?.Cancel();
        player.CancellationTokenSourceAcao = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(5), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            Task.Run(async () =>
            {
                vehicle.RepairEx();
                player.ToggleGameControls(true);
                player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} reparou o veículo {vehicle.Identifier}.");
                await player.WriteLog(LogType.FactionVehicleRepair, vehicle.Identifier, null);
                player.CancellationTokenSourceAcao = null;
            });
        });
    }

    [Command("rapel")]
    public static void CMD_rapel(MyPlayer player)
    {
        if (!(player.Faction?.Government ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        var veh = (MyVehicle)player.Vehicle;
        var model = veh?.VehicleDB?.Model?.ToLower();
        if ((player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_BACK_LEFT && player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_BACK_RIGHT)
            || (model != VehicleModel.Polmav.ToString().ToLower()
                && model != VehicleModelMods.AS332.ToString().ToLower()
                && model != VehicleModelMods.AS350.ToString().ToLower()
                && model != VehicleModelMods.LGUARDMAV.ToString().ToLower()
                && model != VehicleModelMods.AW139.ToString().ToLower())
        )
        {
            player.SendMessage(Models.MessageType.Error, "Você não está nos assentos traseiros de um helicóptero apropriado.");
            return;
        }

        player.Emit("TaskRappelFromHeli");
    }

    [Command("mostrardistintivo", "/mostrardistintivo (ID ou nome)")]
    public static void CMD_mostrardistintivo(MyPlayer player, string idOrName)
    {
        if (player.Character.Badge == 0)
        {
            player.SendMessage(Models.MessageType.Error, "Você não possui um distintivo.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(Models.MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        target.SendMessage(Models.MessageType.None, $"Distintivo #{player.Character.Badge} de {player.Character.Name}", $"#{player.Faction!.Color}");
        target.SendMessage(Models.MessageType.None, $"{player.Faction.Name} - {player.FactionRank!.Name}");
        player.SendMessageToNearbyPlayers(player == target ? "olha seu próprio distintivo." : $"mostra seu distintivo para {target.ICName}.", MessageCategory.Ame);
    }

    [Command("hq", "/hq (mensagem)", GreedyArg = true)]
    public async Task CMD_hq(MyPlayer player, string message)
    {
        if (!(player.Faction?.HasWalkieTalkie ?? false))
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção habilitada.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.HQ))
        {
            player.SendMessage(Models.MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        if (player.FactionWalkieTalkieToggle)
        {
            player.SendMessage(Models.MessageType.Error, "Você ocultou as mensagens de rádio da facção.");
            return;
        }

        message = Functions.CheckFinalDot(message);
        var targetMessage = $"[HQ] {player.FactionRank!.Name} {player.Character.Name}: {message}";
        var targets = Global.SpawnedPlayers.Where(x => x.Character.FactionId == player.Character.FactionId && !x.FactionWalkieTalkieToggle);
        foreach (var target in targets)
            target.SendMessage(Models.MessageType.None, targetMessage, "#47C3F3");

        await player.WriteLog(LogType.Faction, $"/hq {message}", null);
    }

    [Command("br", "/br (tipo). Use /br 0 para visualizar os tipos.")]
    public static void CMD_br(MyPlayer player, int type)
    {
        if (!(player.Faction?.HasBarriers ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        var furnitures = Global.Furnitures.Where(x => x.Category.ToLower() == Globalization.BARRIERS).ToList();
        if (furnitures.Count == 0)
        {
            player.SendMessage(Models.MessageType.Error, "Nenhuma barreira criada. Por favor, reporte o bug.");
            return;
        }

        if (type == 0)
        {
            player.SendMessage(Models.MessageType.Title, "Lista de barreiras");
            foreach (var furniture in furnitures)
                player.SendMessage(Models.MessageType.None, $"{furnitures.IndexOf(furniture) + 1} - {furniture.Name}");
        }
        else
        {
            if (type > furnitures.Count)
            {
                player.SendMessage(Models.MessageType.Error, $"Tipo deve ser entre 1 e {furnitures.Count}.");
                return;
            }

            player.DropBarrierModel = furnitures[type - 1].Model;
            player.Emit("DropObject", player.DropBarrierModel, 1);
        }
    }

    [Command("rb")]
    public async Task CMD_rb(MyPlayer player)
    {
        if (!(player.Faction?.HasBarriers ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        var barrier = Global.Objects.Where(x =>
            x.CharacterId == player.Character.Id
            && x.GetDimension() == player.GetDimension()
            && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE)
        .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (barrier is null)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está próximo de nenhuma barreira criada por você.");
            return;
        }

        barrier.DestroyObject();
        player.SendMessageToNearbyPlayers($"retira a barreira do chão.", MessageCategory.Ame);
        await player.WriteLog(LogType.Faction, $"/rb | X: {barrier.Position.X} Y: {barrier.Position.Y} Z: {barrier.Position.Z}", null);
    }

    [Command("rball")]
    public async Task CMD_rball(MyPlayer player)
    {
        if (!(player.Faction?.HasBarriers ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.RemoveAllBarriers))
        {
            player.SendMessage(Models.MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        var barriers = Global.Objects.Where(x => x.FactionId == player.Faction.Id).ToList();
        if (barriers.Count == 0)
        {
            player.SendMessage(Models.MessageType.Error, "Não há barreiras criadas pela facção.");
            return;
        }

        foreach (var barrier in barriers)
            barrier.DestroyObject();

        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} removeu todas as barreiras da facção.");
        await player.WriteLog(LogType.Faction, $"/rball", null);
    }

    [Command("rballme")]
    public async Task CMD_rballme(MyPlayer player)
    {
        if (!(player.Faction?.HasBarriers ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        var barriers = Global.Objects.Where(x => x.CharacterId == player.Character.Id).ToList();
        if (barriers.Count == 0)
        {
            player.SendMessage(Models.MessageType.Error, "Não há barreiras criadas por você.");
            return;
        }

        foreach (var barrier in barriers)
            barrier.DestroyObject();

        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} removeu todas as suas barreiras.");
        await player.WriteLog(LogType.Faction, $"/rballme", null);
    }

    [Command("setdep", "/setdep (departamento [lista])")]
    public static void CMD_setdep(MyPlayer player, string shortName)
    {
        if (!(player.Faction?.HasWalkieTalkie ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        shortName = shortName.ToUpper();
        if (shortName == "LISTA")
        {
            player.SendMessage(Models.MessageType.Title, "Departamentos");
            player.SendMessage(Models.MessageType.None, "ALL - Todos");
            foreach (var govFaction in Global.Factions.Where(x => x.HasWalkieTalkie && x.Id != player.Faction.Id).OrderBy(x => x.ShortName))
                player.SendMessage(Models.MessageType.None, $"{govFaction.ShortName.ToUpper()} - {govFaction.Name}");
            return;
        }

        if (shortName != "ALL")
        {
            var faction = Global.Factions.FirstOrDefault(x => x.ShortName.ToUpper() == shortName && x.HasWalkieTalkie);
            if (faction is null)
            {
                player.SendMessage(Models.MessageType.Error, $"Departamento {shortName} não existe.");
                return;
            }

            if (faction.Id == player.Faction.Id)
            {
                player.SendMessage(Models.MessageType.Error, "Esse já o seu departamento.");
                return;
            }
        }

        player.TargetFactionDepartment = shortName;
        player.SendMessage(Models.MessageType.Success, $"Você alterou o departamento destino para {shortName}.");
    }

    [Command("dep", "/dep (mensagem)", GreedyArg = true)]
    public static void CMD_dep(MyPlayer player, string message)
    {
        if (!(player.Faction?.HasWalkieTalkie ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(Models.MessageType.Error, Globalization.ACTIONS_BLOCKED_MESSAGE);
            return;
        }

        message = Functions.CheckFinalDot(message);

        var targetMessage = $"[{player.Faction.ShortName} => {player.TargetFactionDepartment}] {player.ICName}: {message}";

        var targets = Global.SpawnedPlayers.Where(x => x.OnDuty &&
            (x.Character.FactionId == player.Faction.Id
                || x.Character.Faction?.ShortName.ToUpper() == player.TargetFactionDepartment.ToUpper()
                || (x.Faction?.HasWalkieTalkie == true && player.TargetFactionDepartment == "ALL")));
        foreach (var target in targets)
            target.SendMessage(Models.MessageType.None, targetMessage, "#a35353");

        player.SendMessageToNearbyPlayers(message, MessageCategory.WalkieTalkie);
    }

    [Command("usaruniforme", "/usaruniforme (tipo). Use /usaruniforme 0 para visualizar os tipos.")]
    public static void CMD_usaruniforme(MyPlayer player, int type)
    {
        if (!player.ValidPed)
        {
            player.SendMessage(Models.MessageType.Error, Globalization.INVALID_SKIN_MESSAGE);
            return;
        }

        if (!(player.Faction?.HasDuty ?? false) || !player.OnDuty)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        var factionUniforms = Global.FactionsUniforms.Where(x => x.FactionId == player.Faction.Id && x.Sex == player.Character.Sex).ToList();
        if (factionUniforms.Count == 0)
        {
            player.SendMessage(Models.MessageType.Error, "Sua facção não possui nenhuma pré-definição de uniforme.");
            return;
        }

        if (type == 0)
        {
            player.SendMessage(Models.MessageType.Title, "Uniformes");
            foreach (var factionUniformList in factionUniforms)
                player.SendMessage(Models.MessageType.None, $"{factionUniforms.IndexOf(factionUniformList) + 1} - {factionUniformList.Name}");
            return;
        }

        if (type > factionUniforms.Count)
        {
            player.SendMessage(Models.MessageType.Error, $"Tipo deve ser entre 1 e {factionUniforms.Count}.");
            return;
        }

        var factionUniform = factionUniforms[type - 1];
        var outfits = Functions.Deserialize<IEnumerable<Outfit>>(player.Character.OutfitsOnDutyJSON).ToList();
        outfits[player.Character.OutfitOnDuty - 1] = Functions.Deserialize<Outfit>(factionUniform.OutfitJSON);
        outfits[player.Character.OutfitOnDuty - 1].Slot = player.Character.OutfitOnDuty;
        player.Character.SetOutfitOnDuty(player.Character.OutfitOnDuty, Functions.Serialize(outfits));
        player.SetOutfit();

        player.SendMessage(Models.MessageType.Success, $"Você alterou seu uniforme para {factionUniform.Name}.");
    }

    [Command("ftow")]
    public async Task CMD_ftow(MyPlayer player)
    {
        if (!(player.Faction?.HasVehicles ?? false))
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma governamental ou não está em serviço.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.RespawnVehicles))
        {
            player.SendMessage(Models.MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        foreach (var vehicle in Global.Vehicles
            .Where(x => x.VehicleDB.FactionId == player.Character.FactionId
                && x.Occupants.Count == 0))
            await vehicle.Park(player);

        player.SendFactionMessage($"{player.User.Name} respawnou todos os veículos da facção sem ocupantes.");
        await player.WriteLog(LogType.Faction, "/ftow", null);
    }
}