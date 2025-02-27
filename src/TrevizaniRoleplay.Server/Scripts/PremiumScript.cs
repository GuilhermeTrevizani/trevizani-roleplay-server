using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PremiumScript : Script
{
    [Command("premium")]
    public static void CMD_premium(MyPlayer player)
    {
        bool GetDifferentLevel(string name)
        {
            UserPremium? userPremium = name switch
            {
                Globalization.PREMIUM_GOLD => UserPremium.Gold,
                Globalization.PREMIUM_SILVER => UserPremium.Silver,
                Globalization.PREMIUM_BRONZE => UserPremium.Bronze,
                _ => null,
            };

            var currentPremium = player.GetCurrentPremium();
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
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (item.Value > player.User.PremiumPoints)
            {
                player.SendNotification(NotificationType.Error, $"Você não possui LS Points suficientes ({item.Value:N0}).");
                return;
            }

            ulong premiumDiscordRole = 0;

            switch (item.Name)
            {
                case Globalization.PREMIUM_BRONZE:
                    premiumDiscordRole = Global.PremiumBronzeDiscordRole;
                    player.User.SetPremium(UserPremium.Bronze);
                    player.Character.SetPremium(UserPremium.Bronze);
                    break;
                case Globalization.PREMIUM_SILVER:
                    premiumDiscordRole = Global.PremiumSilverDiscordRole;
                    player.User.SetPremium(UserPremium.Silver);
                    player.Character.SetPremium(UserPremium.Silver);
                    break;
                case Globalization.PREMIUM_GOLD:
                    premiumDiscordRole = Global.PremiumGoldDiscordRole;
                    player.User.SetPremium(UserPremium.Gold);
                    player.Character.SetPremium(UserPremium.Gold);
                    break;
                case Globalization.NAME_CHANGE:
                    player.User.AddNameChanges();
                    break;
                case Globalization.NUMBER_CHANGE:
                    player.User.AddNumberChanges();
                    break;
                case Globalization.PLATE_CHANGE:
                    player.User.AddPlateChanges();
                    break;
                case Globalization.CHARACTER_SLOT:
                    player.User.AddCharacterSlots();
                    break;
                case Globalization.OUTFIT_10:
                    player.User.AddExtraOutfitSlots(10);
                    break;
                case Globalization.INTERNAL_FURNITURES_50:
                    if (player.User.ExtraInteriorFurnitureSlots + 50 > 1000)
                    {
                        player.SendNotification(NotificationType.Error, "Não é possível prosseguir pois o limite de mobílias internas extra é de 1000.");
                        return;
                    }

                    player.User.AddExtraInteriorFurnitureSlots(50);
                    break;
                case Globalization.INTERNAL_FURNITURES_500:
                    if (player.User.ExtraInteriorFurnitureSlots + 500 > 1000)
                    {
                        player.SendNotification(NotificationType.Error, "Não é possível prosseguir pois o limite de mobílias internas extra é de 1000.");
                        return;
                    }
                    player.User.AddExtraInteriorFurnitureSlots(500);
                    break;
                default:
                    player.SendNotification(NotificationType.Error, $"Item Premium {name} não foi implementado. Por favor, reporte o bug.");
                    break;
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