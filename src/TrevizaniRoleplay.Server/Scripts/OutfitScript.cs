using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class OutfitScript : Script
{
    [Command("outfits")]
    public static void CMD_outfits(MyPlayer player)
    {
        if (!player.ValidPed)
        {
            player.SendMessage(MessageType.Error, Globalization.INVALID_SKIN_MESSAGE);
            return;
        }

        player.Emit("Outfits:Show",
            player.UsingOutfitsOnDuty ? player.Character.OutfitsOnDutyJSON : player.Character.OutfitsJSON,
            player.UsingOutfitsOnDuty ? player.Character.OutfitOnDuty : player.Character.Outfit,
            player.MaxOutfit);
    }

    [Command("outfit", "/outfit (slot)")]
    public static void CMD_outfit(MyPlayer player, int outfit) => SetOutfit(player, outfit);

    [RemoteEvent(nameof(SetOutfit))]
    public static void SetOutfit(Player playerParam, int setOutfit)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsActionsBlocked())
            {
                player.SendMessage(MessageType.Error, Globalization.ACTIONS_BLOCKED_MESSAGE);
                return;
            }

            if (!player.ValidPed)
            {
                player.SendMessage(MessageType.Error, Globalization.INVALID_SKIN_MESSAGE);
                return;
            }

            var outfit = Convert.ToByte(setOutfit);
            if (outfit < 1 || outfit > player.MaxOutfit)
            {
                player.SendMessage(MessageType.Error, $"Outfit deve ser entre 1 e {player.MaxOutfit}.");
                return;
            }

            if (player.UsingOutfitsOnDuty)
                player.Character.SetOutfitOnDuty(outfit, player.Character.OutfitsOnDutyJSON);
            else
                player.Character.SetOutfit(outfit, player.Character.OutfitsJSON);

            player.SetOutfit();
            player.SendMessage(MessageType.Success, $"Você alterou seu outfit para {setOutfit}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ToggleOutfitPart))]
    public static void ToggleOutfitPart(Player playerParam, string outfitJson)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.ValidPed)
            {
                player.SendMessage(MessageType.Error, Globalization.INVALID_SKIN_MESSAGE);
                return;
            }

            var outfitsJson = player.UsingOutfitsOnDuty ? player.Character.OutfitsOnDutyJSON : player.Character.OutfitsJSON;
            var outfits = Functions.Deserialize<IEnumerable<Outfit>>(outfitsJson).ToList();
            var outfit = Functions.Deserialize<Outfit>(outfitJson);
            var index = outfit.Slot - 1;
            outfits[index] = outfit;
            outfitsJson = Functions.Serialize(outfits);
            if (player.UsingOutfitsOnDuty)
                player.Character.SetOutfitOnDuty(player.Character.OutfitOnDuty, outfitsJson);
            else
                player.Character.SetOutfit(player.Character.Outfit, outfitsJson);
            player.SetOutfit();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}