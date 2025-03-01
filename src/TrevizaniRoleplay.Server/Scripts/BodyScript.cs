using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class BodyScript : Script
{
    [Command("recolhercorpo")]
    public async Task CMD_recolhercorpo(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendNotification(NotificationType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        var body = Global.Bodies.FirstOrDefault(x => x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (body is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está próximo de um corpo.");
            return;
        }

        if (!body.MorgueDate.HasValue)
        {
            var context = Functions.GetDatabaseContext();
            body.SetMorgueDate(DateTime.Now);
            context.Bodies.Update(body);
            await context.SaveChangesAsync();
        }

        body.RemoveIdentifier();
        Global.Bodies.Remove(body);

        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} enviou um corpo para o necrotério.");
        await player.WriteLog(LogType.Faction, $"/recolhercopo {body.Id} {body.Name}", null);
    }

    [Command("arecolhercorpo")]
    public async Task CMD_arecolhercorpo(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.JuniorServerAdmin)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var body = Global.Bodies.FirstOrDefault(x => x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (body is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está próximo de um corpo.");
            return;
        }

        if (!body.MorgueDate.HasValue)
        {
            var context = Functions.GetDatabaseContext();
            body.SetMorgueDate(DateTime.Now);
            context.Bodies.Update(body);
            await context.SaveChangesAsync();
        }

        body.RemoveIdentifier();
        Global.Bodies.Remove(body);

        await Functions.SendServerMessage($"{player.User.Name} enviou o corpo de {body.Name} ({body.RegisterDate}) para o necrotério.", UserStaff.JuniorServerAdmin, false);
        await player.WriteLog(LogType.Staff, $"/arecolhercopo {body.Id} {body.Name}", null);
    }

    [Command("aremovercorpo")]
    public async Task CMD_aremovercorpo(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.ServerAdminI)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var body = Global.Bodies.FirstOrDefault(x => x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (body is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um corpo.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        if (body.Items!.Count > 0)
            context.BodiesItems.RemoveRange(body.Items);

        context.Bodies.Remove(body);
        await context.SaveChangesAsync();

        body.RemoveIdentifier();
        Global.Bodies.Remove(body);

        await Functions.SendServerMessage($"{player.User.Name} removeu o corpo de {body.Name} ({body.RegisterDate}).", UserStaff.JuniorServerAdmin, false);
        await player.WriteLog(LogType.Staff, $"/aremovercorpo {Functions.Serialize(body)}", null);
    }

    [Command("corpoinv")]
    public static void CMD_corpoinv(MyPlayer player)
    {
        var body = Global.Bodies.FirstOrDefault(x => x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (body is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um corpo.");
            return;
        }

        body.ShowInventory(player, false);
    }

    [Command("corpoferimentos")]
    public static void CMD_corpoferimentos(MyPlayer player)
    {
        var body = Global.Bodies.FirstOrDefault(x => x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (body is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um corpo.");
            return;
        }

        var wounds = Functions.Deserialize<IEnumerable<Wound>>(body.WoundsJSON);

        player.Emit("ViewCharacterWounds", "Corpo", Functions.Serialize(wounds.Select(x => new
        {
            x.Date,
            x.Weapon,
            x.Damage,
            x.BodyPart,
        })), false);
    }

    [RemoteEvent(nameof(MorgueViewBody))]
    public async Task MorgueViewBody(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial ou não está em serviço.");
                return;
            }

            var spot = Global.Spots
                .FirstOrDefault(x => x.Type == SpotType.Morgue
                    && x.Dimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
            if (spot is null)
            {
                player.SendNotification(NotificationType.Error, "Você não está em ponto de necrotério.");
                return;
            }

            if (Global.Bodies.Any(x => x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE))
            {
                player.SendNotification(NotificationType.Error, "Já existe um corpo sendo visualizado no momento.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var id = idString.ToGuid();
            var body = await context.Bodies
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (body is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            body.SetPosition(spot.PosX, spot.PosY, spot.PosZ, spot.Dimension);

            Global.Bodies.Add(body);
            body.CreateIdentifier();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}