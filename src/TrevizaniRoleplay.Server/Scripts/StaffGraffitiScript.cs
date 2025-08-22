using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffGraffitiScript : Script
{
    [Command("grafites")]
    public async Task CMD_grafites(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffGraffiti:Show", await GetGraffitisJson());
    }

    [RemoteEvent(nameof(StaffGraffitiGoto))]
    public static void StaffGraffitiGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var graffiti = Global.Graffitis.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (graffiti is null)
                return;

            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            player.SetPosition(new(graffiti.PosX, graffiti.PosY, graffiti.PosZ), graffiti.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffGraffitiRemove))]
    public async Task StaffGraffitiRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var graffiti = Global.Graffitis.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (graffiti is null)
                return;

            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            graffiti.RemoveIdentifier();
            Global.Graffitis.Remove(graffiti);
            var context = Functions.GetDatabaseContext();
            context.Graffitis.Remove(graffiti);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Remover Grafite | {Functions.Serialize(graffiti)}", null);
            player.SendNotification(NotificationType.Success, "Grafite removido.");
            player.Emit("StaffGraffiti:Update", await GetGraffitisJson());
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task<string> GetGraffitisJson()
    {
        var context = Functions.GetDatabaseContext();
        var characters = await context.Characters
            .Where(x => Global.Graffitis.Select(y => y.CharacterId).Contains(x.Id))
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

        return Functions.Serialize(Global.Graffitis
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Text,
                Font = x.Font.ToString(),
                x.Size,
                Character = GetCharacter(x.CharacterId),
                x.Dimension,
                x.PosX,
                x.PosY,
                x.PosZ,
                x.RotR,
                x.RotP,
                x.RotY,
                x.ColorR,
                x.ColorG,
                x.ColorB,
                x.ColorA,
            }));
    }
}