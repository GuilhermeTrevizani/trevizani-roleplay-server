using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class InfoScript : Script
{
    [Command("infos")]
    public static void CMD_infos(MyPlayer player)
    {
        player.Emit("Info:Show", GetInfosJson(player));
    }

    [RemoteEvent(nameof(InfoSave))]
    public async Task InfoSave(Player playerParam, int days, string message, bool image)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var maxDays = player.GetCurrentPremium() switch
            {
                UserPremium.Gold => 30,
                UserPremium.Silver => 15,
                UserPremium.Bronze => 7,
                _ => 3,
            };

            if (days > maxDays)
            {
                player.SendNotification(NotificationType.Error, $"O máximo de dias permitido para seu nível de Premium é de {maxDays}.");
                return;
            }

            if (image && !Functions.IsValidImageUrl(message))
            {
                player.SendNotification(NotificationType.Error, $"URL de imagem inválida. Use Imgur.");
                return;
            }

            var infoCount = player.GetCurrentPremium() switch
            {
                UserPremium.Gold => 10,
                UserPremium.Silver => 5,
                UserPremium.Bronze => 3,
                _ => 1,
            };

            if (Global.Infos.Count(x => x.CharacterId == player.Character.Id) >= infoCount)
            {
                player.SendNotification(NotificationType.Error, $"Não é possível prosseguir pois o máximo de {infoCount} infos do seu personagem será atingido.");
                return;
            }

            var info = new Info();
            info.Create(player.GetPosition().X, player.GetPosition().Y, player.GetPosition().Z - 0.7f, player.GetDimension(), days, player.Character.Id, message, image);

            var context = Functions.GetDatabaseContext();
            await context.Infos.AddAsync(info);
            await context.SaveChangesAsync();

            Global.Infos.Add(info);
            info.CreateIdentifier();

            await player.WriteLog(LogType.Info, $"Criar Info | {Functions.Serialize(info)}", null);
            player.SendNotification(NotificationType.Success, "Info criada.");
            player.Emit("Info:Update", GetInfosJson(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(InfoRemove))]
    public async Task InfoRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var info = Global.Infos.FirstOrDefault(x => x.Id == id);
            if (info is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (info.CharacterId != player.Character.Id)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            info.RemoveIdentifier();
            Global.Infos.Remove(info);
            var context = Functions.GetDatabaseContext();
            context.Infos.Remove(info);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Info, $"Remover Info | {Functions.Serialize(info)}", null);
            player.SendNotification(NotificationType.Success, "Info removida.");
            player.Emit("Info:Update", GetInfosJson(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetInfosJson(MyPlayer player)
    {
        return Functions.Serialize(Global.Infos.Where(x => x.CharacterId == player.Character.Id)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                x.ExpirationDate,
                x.Message,
                x.Image,
            }));
    }
}