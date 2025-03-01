using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class FactionStorageScript : Script
{
    [Command("farmazenamento")]
    public static void CMD_farmazenamento(MyPlayer player)
    {
        if (!player.FactionFlags.Contains(FactionFlag.Storage))
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var factionStorage = Global.FactionsStorages.FirstOrDefault(x =>
            player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE
            && x.FactionId == player.Character.FactionId
            && x.Dimension == player.GetDimension());
        if (factionStorage is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está próximo de nenhum armazenamento da sua facção.");
            return;
        }

        ShowFactionStorage(player, factionStorage.Id);
    }

    [RemoteEvent(nameof(FactionStorageEquipItem))]
    public async Task FactionStorageEquipItem(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var factionStorageItem = Global.FactionsStoragesItems.FirstOrDefault(x => x.Id == id);
            if (factionStorageItem is null || factionStorageItem.Quantity == 0)
            {
                player.SendNotification(NotificationType.Error, "Item não possui estoque.");
                return;
            }

            if (player.Money < factionStorageItem.Price)
            {
                player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, factionStorageItem.Price));
                return;
            }

            var characterItem = new CharacterItem();
            characterItem.Create(factionStorageItem.ItemTemplateId, 0, 1, factionStorageItem.Extra);
            var res = await player.GiveItem(characterItem);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            await player.RemoveMoney(factionStorageItem.Price);

            factionStorageItem.SetQuantity(factionStorageItem.Quantity - 1);
            var context = Functions.GetDatabaseContext();
            context.FactionsStoragesItems.Update(factionStorageItem);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Faction, $"Pegar Item Armazenamento {factionStorageItem.GetName()} | {Functions.Serialize(factionStorageItem)}", null);
            player.SendNotification(NotificationType.Success, $"Você pegou {factionStorageItem.GetName()}.");

            ShowFactionStorage(player, factionStorageItem.FactionStorageId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void ShowFactionStorage(MyPlayer player, Guid factionStorageId)
    {
        player.Emit("ShowFactionStorage",
            Functions.Serialize(Global.FactionsStoragesItems
            .Where(x => x.FactionStorageId == factionStorageId)
            .Select(x => new
            {
                x.Id,
                Name = x.GetName(),
                x.Quantity,
                x.Price,
            })
            .OrderBy(x => x.Name)),
            player.Faction!.Name);
    }
}