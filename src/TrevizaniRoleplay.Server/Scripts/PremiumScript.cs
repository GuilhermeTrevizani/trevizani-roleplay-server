using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PremiumScript : Script
{
    [Command(["premium"], "Geral", "Abre o painel de gerenciamento Premium")]
    public static void CMD_premium(MyPlayer player)
    {
        bool GetDifferentLevel(string name)
        {
            UserPremium? userPremium = null;
            if (name == Resources.PremiumGold)
                userPremium = UserPremium.Gold;
            else if (name == Resources.PremiumSilver)
                userPremium = UserPremium.Silver;
            else if (name == Resources.PremiumBronze)
                userPremium = UserPremium.Bronze;
            var currentPremium = player.User.GetCurrentPremium();
            return userPremium is not null && currentPremium != UserPremium.None && currentPremium != userPremium;
        }

        player.Emit("PremiumStore:Show", player.User.PremiumPoints,
            Functions.Serialize(Global.PremiumItems
            .OrderBy(x => x.Value)
            .Select(x => new
            {
                x.Name,
                x.Value,
                DifferentLevel = GetDifferentLevel(x.Name),
            })));
    }

    [RemoteEvent(nameof(BuyPremiumItem))]
    public async Task BuyPremiumItem(Player playerParam, string name)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var item = Global.PremiumItems.FirstOrDefault(x => x.Name == name);
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (item.Value > player.User.PremiumPoints)
            {
                player.SendNotification(NotificationType.Error, $"Você não possui LS Points suficientes ({item.Value:N0}).");
                return;
            }

            ulong premiumDiscordRole = 0;

            if (item.Name == Resources.PremiumBronze)
            {
                premiumDiscordRole = Global.PremiumBronzeDiscordRole;
                player.User.SetPremium(UserPremium.Bronze);
            }
            else if (item.Name == Resources.PremiumSilver)
            {
                premiumDiscordRole = Global.PremiumSilverDiscordRole;
                player.User.SetPremium(UserPremium.Silver);
            }
            else if (item.Name == Resources.PremiumGold)
            {
                premiumDiscordRole = Global.PremiumGoldDiscordRole;
                player.User.SetPremium(UserPremium.Gold);
            }
            else if (item.Name == Resources.NameChange)
            {
                player.User.AddNameChanges();
            }
            else if (item.Name == Resources.NumberChange)
            {
                player.User.AddNumberChanges();
            }
            else if (item.Name == Resources.PlateChange)
            {
                player.User.AddPlateChanges();
            }
            else if (item.Name == Resources.CharacterSlot)
            {
                player.User.AddCharacterSlots();
            }
            else if (item.Name == Resources.Outfits10)
            {
                player.User.AddExtraOutfitSlots(10);
            }
            else if (item.Name == Resources.InternalFurnitures50)
            {
                if (player.User.ExtraInteriorFurnitureSlots + 50 > 1000)
                {
                    player.SendNotification(NotificationType.Error, "Não é possível prosseguir pois o limite de mobílias internas extra é de 1000.");
                    return;
                }
                player.User.AddExtraInteriorFurnitureSlots(50);
            }
            else if (item.Name == Resources.InternalFurnitures500)
            {
                if (player.User.ExtraInteriorFurnitureSlots + 500 > 1000)
                {
                    player.SendNotification(NotificationType.Error, "Não é possível prosseguir pois o limite de mobílias internas extra é de 1000.");
                    return;
                }
                player.User.AddExtraInteriorFurnitureSlots(500);
            }
            else
            {
                player.SendNotification(NotificationType.Error, $"Item Premium {name} não foi implementado. Por favor, reporte o bug.");
            }

            player.User.RemovePremiumPoints(item.Value);
            await player.Save();

            if (premiumDiscordRole != 0 && Global.DiscordClient is not null)
            {
                var guild = Global.DiscordClient.GetGuild(Global.MainDiscordGuild);
                if (guild is not null)
                {
                    var user = guild.GetUser(Convert.ToUInt64(player.User.DiscordId));
                    if (user is not null)
                    {
                        if (!user.Roles.Any(x => x.Id == premiumDiscordRole))
                            await user.AddRoleAsync(premiumDiscordRole);
                    }
                }
            }

            await player.WriteLog(LogType.Premium, $"Compra de {item.Name} por {item.Value:N0} LS Points", null);
            player.SendNotification(NotificationType.Success, $"Você comprou {item.Name} por {item.Value:N0} LS Points.");
            CMD_premium(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}