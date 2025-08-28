using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffBlipScript : Script
{
    [Command(["blips"], "Staff", "Abre o painel de gerenciamento de blips")]
    public static void CMD_blips(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Blips))
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffBlip:Show", GetBlipsJson());
    }

    [RemoteEvent(nameof(StaffBlipGoto))]
    public static void StaffBlipGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var blip = Global.Blips.FirstOrDefault(x => x.Id == id);
            if (blip is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(blip.PosX, blip.PosY, blip.PosZ), 0, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffBlipSave))]
    public async Task StaffBlipSave(Player playerParam, string idString, string name, Vector3 pos, int type, int color)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Blips))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            name ??= string.Empty;
            if (name.Length < 1 || name.Length > 100)
            {
                player.SendNotification(NotificationType.Error, "Nome deve ter entre 1 e 100 caracteres.");
                return;
            }

            if (type < 1 || type > Constants.MAX_BLIP_TYPE)
            {
                player.SendNotification(NotificationType.Error, string.Format("Tipo deve ser entre 1 e {0}.", Constants.MAX_BLIP_TYPE));
                return;
            }

            if (color < 1 || color > 85)
            {
                player.SendNotification(NotificationType.Error, "Cor deve ser entre 1 e 85.");
                return;
            }

            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            var blip = new Domain.Entities.Blip();
            if (isNew)
            {
                blip.Create(name, pos.X, pos.Y, pos.Z, Convert.ToUInt16(type), Convert.ToByte(color));
            }
            else
            {
                blip = Global.Blips.FirstOrDefault(x => x.Id == id);
                if (blip == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                blip.Update(name, pos.X, pos.Y, pos.Z, Convert.ToUInt16(type), Convert.ToByte(color));
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Blips.AddAsync(blip);
            else
                context.Blips.Update(blip);

            await context.SaveChangesAsync();

            blip.CreateIdentifier();

            if (isNew)
                Global.Blips.Add(blip);

            await player.WriteLog(LogType.Staff, $"Gravar Blip | {Functions.Serialize(blip)}", null);
            player.SendNotification(NotificationType.Success, $"Blip {(isNew ? "criado" : "editado")}.");
            UpdateBlips();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffBlipRemove))]
    public async Task StaffBlipRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Blips))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var blip = Global.Blips.FirstOrDefault(x => x.Id == id);
            if (blip != null)
            {
                var context = Functions.GetDatabaseContext();
                context.Blips.Remove(blip);
                await context.SaveChangesAsync();
                Global.Blips.Remove(blip);
                blip.RemoveIdentifier();
                await player.WriteLog(LogType.Staff, $"Remover Blip | {Functions.Serialize(blip)}", null);
            }

            player.SendNotification(NotificationType.Success, "Blip excluído.");
            UpdateBlips();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateBlips()
    {
        var json = GetBlipsJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Blips)))
            target.Emit("StaffBlip:Update", json);
    }

    private static string GetBlipsJson()
    {
        return Functions.Serialize(Global.Blips.OrderByDescending(x => x.RegisterDate).Select(x => new
        {
            x.Id,
            x.Name,
            x.Type,
            x.Color,
            x.PosX,
            x.PosY,
            x.PosZ,
        }));
    }
}