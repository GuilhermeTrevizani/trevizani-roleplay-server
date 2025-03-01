using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class FireScript : Script
{
    [Command("incendios")]
    public static void CMD_incendios(MyPlayer player)
    {
        if (!player.FactionFlags.Contains(FactionFlag.FireManager))
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("Fire:Show", GetFiresJson());
    }

    private static string GetFiresJson()
    {
        return Functions.Serialize(Global.Fires
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Description,
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Dimension,
                x.FireSpanLife,
                x.MaxFireSpan,
                x.SecondsNewFireSpan,
                x.PositionNewFireSpan,
                x.FireSpanDamage,
                Started = Global.ActiveFires.Any(y => y.Id == x.Id),
            }));
    }

    [RemoteEvent(nameof(FireRemove))]
    public async Task FireRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.FactionFlags.Contains(FactionFlag.FireManager))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var fire = Global.Fires.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (fire is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (Global.ActiveFires.Any(x => x.Id == fire.Id))
            {
                player.SendNotification(NotificationType.Error, "Você não pode excluir um incêndio em andamento.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.Fires.Remove(fire);
            await context.SaveChangesAsync();
            Global.Fires.Remove(fire);

            await player.WriteLog(LogType.Staff, $"Remover Incêndio | {Functions.Serialize(fire)}", null);
            player.SendNotification(NotificationType.Success, "Incêndio excluído.");
            CMD_incendios(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FireSave))]
    public async Task FireSave(Player playerParam, string idString, string description, Vector3 pos, uint dimension,
        int fireSpanLife, int maxFireSpan, int secondsNewFireSpan, float positionNewFireSpan, float fireSpanDamage)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.FactionFlags.Contains(FactionFlag.FireManager))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (secondsNewFireSpan <= 0)
            {
                player.SendNotification(NotificationType.Error, "Segundos Novo Foco deve ser maior que 0.");
                return;
            }

            var id = idString.ToGuid();
            var isNew = !id.HasValue;
            var fire = new Fire();
            if (isNew)
            {
                fire.Create(description, pos.X, pos.Y, pos.Z, dimension,
                    fireSpanLife, maxFireSpan, secondsNewFireSpan, positionNewFireSpan, fireSpanDamage);
            }
            else
            {
                fire = Global.Fires.FirstOrDefault(x => x.Id == id);
                if (fire is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                if (Global.ActiveFires.Any(x => x.Id == fire.Id))
                {
                    player.SendNotification(NotificationType.Error, "Você não pode editar um incêndio em andamento.");
                    return;
                }

                fire.Update(description, pos.X, pos.Y, pos.Z, dimension,
                fireSpanLife, maxFireSpan, secondsNewFireSpan, positionNewFireSpan, fireSpanDamage);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Fires.AddAsync(fire);
            else
                context.Fires.Update(fire);

            await context.SaveChangesAsync();

            if (isNew)
                Global.Fires.Add(fire);

            await player.WriteLog(LogType.Staff, $"Gravar Incêndio | {Functions.Serialize(fire)}", null);
            player.SendNotification(NotificationType.Success, $"Incêndio {(isNew ? "criado" : "editado")}.");
            CMD_incendios(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FireStart))]
    public async Task FireStart(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.FactionFlags.Contains(FactionFlag.FireManager))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var fire = Global.Fires.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (fire is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var activeFire = new ActiveFire();
            activeFire.Start(fire);

            await player.WriteLog(LogType.Staff, $"Iniciar Incêndio | {Functions.Serialize(fire)}", null);
            player.SendNotification(NotificationType.Success, "Você iniciou o incêndio.");
            CMD_incendios(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(FireStop))]
    public async Task FireStop(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.FactionFlags.Contains(FactionFlag.FireManager))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var fire = Global.Fires.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (fire is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var activeFire = Global.ActiveFires.FirstOrDefault(x => x.Id == fire.Id);
            activeFire?.Stop();

            await player.WriteLog(LogType.Staff, $"Parar Incêndio | {Functions.Serialize(fire)}", null);
            player.SendNotification(NotificationType.Success, "Você parou o incêndio.");
            CMD_incendios(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}