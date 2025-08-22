using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffInfoScript : Script
{
    [Command("ainfos")]
    public async Task CMD_ainfos(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffInfo:Show", await GetInfosJson());
    }

    [RemoteEvent(nameof(StaffInfoGoto))]
    public static void StaffInfoGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var info = Global.Infos.FirstOrDefault(x => x.Id == id);
            if (info is null)
                return;

            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            player.SetPosition(new(info.PosX, info.PosY, info.PosZ), info.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffInfoRemove))]
    public async Task StaffInfoRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var info = Global.Infos.FirstOrDefault(x => x.Id == id);
            if (info is null)
                return;

            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            info.RemoveIdentifier();
            Global.Infos.Remove(info);
            var context = Functions.GetDatabaseContext();
            context.Infos.Remove(info);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Remover Info | {Functions.Serialize(info)}", null);
            player.SendNotification(NotificationType.Success, "Info removida.");
            player.Emit("StaffInfo:Update", await GetInfosJson());
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task<string> GetInfosJson()
    {
        var context = Functions.GetDatabaseContext();
        var characters = await context.Characters
            .Where(x => Global.Infos.Select(y => y.CharacterId).Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Name,
            })
            .ToListAsync();

        string GetCharacter(Guid characterId)
        {
            return characters.FirstOrDefault(x => x.Id == characterId)!.Name;
        }

        return Functions.Serialize(Global.Infos
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                x.ExpirationDate,
                Character = GetCharacter(x.CharacterId),
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Dimension,
                x.Message,
                x.Image,
            }));
    }
}