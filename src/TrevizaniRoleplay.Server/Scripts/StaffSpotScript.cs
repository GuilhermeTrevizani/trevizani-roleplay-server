using GTANetworkAPI;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffSpotScript : Script
{
    [Command("pontos")]
    public static void CMD_pontos(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Spots))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var jsonTypes = Functions.Serialize(
            Enum.GetValues<SpotType>()
            .Select(x => new
            {
                Value = x,
                Label = x.GetDescription(),
            })
            .OrderBy(x => x.Label)
        );

        player.Emit("StaffSpot:Show", GetSpotsJson(), jsonTypes);
    }

    [RemoteEvent(nameof(StaffSpotGoto))]
    public static void StaffSpotGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var spot = Global.Spots.FirstOrDefault(x => x.Id == id);
            if (spot is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(spot.PosX, spot.PosY, spot.PosZ), spot.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("criarponto", "/criarponto (tipo)")]
    public Task CMD_criarponto(MyPlayer player, byte type)
    {
        return StaffSpotSave(player, string.Empty, type, player.GetPosition(), player.GetDimension());
    }

    [RemoteEvent(nameof(StaffSpotSave))]
    public async Task StaffSpotSave(Player playerParam, string idString, int type, Vector3 pos, uint dimension)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Spots))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (!Enum.IsDefined(typeof(SpotType), Convert.ToByte(type)))
            {
                player.SendNotification(NotificationType.Error, "Tipo inválido.");
                return;
            }

            var spot = new Spot();
            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            if (isNew)
            {
                spot.Create((SpotType)type, pos.X, pos.Y, pos.Z, dimension);
            }
            else
            {
                spot = Global.Spots.FirstOrDefault(x => x.Id == id);
                if (spot is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                spot.Update((SpotType)type, pos.X, pos.Y, pos.Z, dimension);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Spots.AddAsync(spot);
            else
                context.Spots.Update(spot);

            await context.SaveChangesAsync();

            spot.CreateIdentifier();

            if (isNew)
                Global.Spots.Add(spot);

            await player.WriteLog(LogType.Staff, $"Gravar Ponto | {Functions.Serialize(spot)}", null);
            player.SendNotification(NotificationType.Success, $"Ponto {(isNew ? "criado" : "editado")}.");
            UpdateSpots();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("delponto")]
    public async Task CMD_delponto(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Spots))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var spot = Global.Spots.Where(x => x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (spot is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum ponto.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        context.Spots.Remove(spot);
        await context.SaveChangesAsync();
        Global.Spots.Remove(spot);
        spot.RemoveIdentifier();
        await player.WriteLog(LogType.Staff, $"Remover Ponto | {Functions.Serialize(spot)}", null);
        player.SendMessage(MessageType.Success, "Ponto excluído.");
    }

    private static void UpdateSpots()
    {
        var json = GetSpotsJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Spots)))
            target.Emit("StaffSpot:Update", json);
    }

    private static string GetSpotsJson()
    {
        return Functions.Serialize(Global.Spots.OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Type,
                TypeDisplay = x.Type.GetDescription(),
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Dimension,
            }));
    }
}