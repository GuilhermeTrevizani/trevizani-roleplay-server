using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class GraffitiScript : Script
{
    [Command("grafitar")]
    public static void CMD_grafitar(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        var jsonFonts = Functions.Serialize(
            Enum.GetValues<GraffitiFont>()
            .Select(x => new
            {
                Value = (byte)x,
                Label = x.ToString(),
            })
            .OrderBy(x => x.Label)
        );

        player.Emit("Graffiti:Config", jsonFonts);
    }

    [RemoteEvent(nameof(GraffitiSave))]
    public async Task GraffitiSave(Player playerParam, string json)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var graffitiRequest = Functions.Deserialize<GraffitiRequest>(json);
            if (graffitiRequest is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (graffitiRequest.Text.Length < 1 || graffitiRequest.Text.Length > 35)
            {
                player.SendNotification(NotificationType.Error, "Texto deve ter entre 1 e 35 caracteres.");
                return;
            }

            if (graffitiRequest.Size < 0 || graffitiRequest.Size > 100)
            {
                player.SendNotification(NotificationType.Error, "Tamanho deve ser entre 1 e 100.");
                return;
            }

            var graffitiCount = player.GetCurrentPremium() switch
            {
                UserPremium.Gold => 5,
                UserPremium.Silver => 4,
                UserPremium.Bronze => 3,
                _ => 2,
            };

            if (Global.Graffitis.Count(x => x.CharacterId == player.Character.Id) >= graffitiCount)
            {
                player.SendNotification(NotificationType.Error, $"Não é possível prosseguir pois o máximo de {graffitiCount} grafites do seu personagem será atingido.");
                return;
            }

            var days = player.GetCurrentPremium() switch
            {
                UserPremium.Gold => 21,
                UserPremium.Silver => 14,
                UserPremium.Bronze => 7,
                _ => 5,
            };

            var graffiti = new Graffiti();
            graffiti.Create(player.Character.Id, graffitiRequest.Text, graffitiRequest.Size, graffitiRequest.Font,
                player.GetDimension(), graffitiRequest.PosX, graffitiRequest.PosY, graffitiRequest.PosZ,
                graffitiRequest.RotR, graffitiRequest.RotP, graffitiRequest.RotY,
                graffitiRequest.ColorR, graffitiRequest.ColorG, graffitiRequest.ColorB, graffitiRequest.ColorA, days);

            var context = Functions.GetDatabaseContext();
            await context.Graffitis.AddAsync(graffiti);
            await context.SaveChangesAsync();

            Global.Graffitis.Add(graffiti);
            graffiti.CreateIdentifier();

            await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) grafitou {graffiti.Text.Replace("<br />", " ")}.", UserStaff.JuniorServerAdmin, false);
            player.SendNotification(NotificationType.Success, "Grafite criado.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("rgrafite")]
    public async Task CMD_rgrafite(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        var graffiti = Global.Graffitis
           .Where(x => x.CharacterId == player.Character.Id
               && x.Dimension == player.GetDimension()
               && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE)
           .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
        if (graffiti is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um grafite de sua autoria.");
            return;
        }

        graffiti.RemoveIdentifier();
        Global.Graffitis.Remove(graffiti);
        var context = Functions.GetDatabaseContext();
        context.Graffitis.Remove(graffiti);
        await context.SaveChangesAsync();

        await player.WriteLog(LogType.Faction, $"/rgrafite {Functions.Serialize(graffiti)}", null);
        player.SendMessage(MessageType.Success, "Você removeu seu grafite.");
    }
}