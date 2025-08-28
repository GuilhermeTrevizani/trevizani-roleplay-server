using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffFactionStorageScript : Script
{
    [Command(["armazenamentos"], "Staff", "Abre o painel de gerenciamento de armazenamentos")]
    public static void CMD_armazenamentos(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffFactionStorage:Show", GetFactionsStoragesJson());
    }

    [RemoteEvent(nameof(StaffFactionStorageGoto))]
    public static void StaffFactionStorageGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var factionStorage = Global.FactionsStorages.FirstOrDefault(x => x.Id == id);
            if (factionStorage is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(factionStorage.PosX, factionStorage.PosY, factionStorage.PosZ), factionStorage.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateFactionStorages()
    {
        var json = GetFactionsStoragesJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.FactionsStorages)))
            target.Emit("StaffFactionStorage:Update", json);
    }

    [RemoteEvent(nameof(StaffFactionStorageRemove))]
    public async Task StaffFactionStorageRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var factionStorage = Global.FactionsStorages.FirstOrDefault(x => x.Id == id);
            if (factionStorage == null)
                return;

            var context = Functions.GetDatabaseContext();
            context.FactionsStorages.Remove(factionStorage);
            context.FactionsStoragesItems.RemoveRange(Global.FactionsStoragesItems.Where(x => x.FactionStorageId == id));
            await context.SaveChangesAsync();
            Global.FactionsStorages.Remove(factionStorage);
            Global.FactionsStoragesItems.RemoveAll(x => x.FactionStorageId == factionStorage.Id);
            factionStorage.RemoveIdentifier();

            await player.WriteLog(LogType.Staff, $"Remover Armazenamento | {Functions.Serialize(factionStorage)}", null);
            player.SendNotification(NotificationType.Success, "Armazenamento excluído.");
            UpdateFactionStorages();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionStorageSave))]
    public async Task StaffFactionStorageSave(Player playerParam, string idString, Vector3 pos, uint dimension, string factionName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == factionName?.ToLower());
            if (faction is null)
            {
                player.SendNotification(NotificationType.Error, $"Facção {factionName} não existe.");
                return;
            }

            var id = idString.ToGuid();
            var isNew = !id.HasValue;
            var factionStorage = new FactionStorage();
            if (isNew)
            {
                factionStorage.Create(faction!.Id, pos.X, pos.Y, pos.Z, dimension);
            }
            else
            {
                factionStorage = Global.FactionsStorages.FirstOrDefault(x => x.Id == id);
                if (factionStorage == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                factionStorage.Update(faction!.Id, pos.X, pos.Y, pos.Z, dimension);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.FactionsStorages.AddAsync(factionStorage);
            else
                context.FactionsStorages.Update(factionStorage);

            await context.SaveChangesAsync();

            if (isNew)
                Global.FactionsStorages.Add(factionStorage);

            factionStorage.CreateIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Armazenamento | {Functions.Serialize(factionStorage)}", null);
            player.SendNotification(NotificationType.Success, $"Armazenamento {(isNew ? "criado" : "editado")}.");
            UpdateFactionStorages();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionsStoragesItemsShow))]
    public static void StaffFactionsStoragesItemsShow(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            player.Emit("StaffFactionStorageItem:Show", GetFactionStoragesItemsJson(id!.Value), Functions.GetItemsTemplatesResponse(), idString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionStorageItemSave))]
    public async Task StaffFactionStorageItemSave(Player playerParam, string factionStorageItemIdString, string factionStorageIdString,
        string itemTemplateIdString, int quantity, int price)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == itemTemplateIdString.ToGuid());
            if (itemTemplate is null)
            {
                player.SendNotification(NotificationType.Error, $"Template {itemTemplateIdString} não existe.");
                return;
            }

            if (quantity < 0)
            {
                player.SendNotification(NotificationType.Error, "Estoque deve ser maior ou igual a 0.");
                return;
            }

            if (price <= 0)
            {
                player.SendNotification(NotificationType.Error, "Preço deve ser maior que 0.");
                return;
            }

            var factionStorageItem = new FactionStorageItem();
            var factionStorageItemId = factionStorageItemIdString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(factionStorageItemIdString);
            if (isNew)
            {
                var factionStorageId = factionStorageIdString.ToGuid();
                factionStorageItem.SetFactionStorageIdAndPrice(factionStorageId!.Value, price);
            }
            else
            {
                factionStorageItem = Global.FactionsStoragesItems.FirstOrDefault(x => x.Id == factionStorageItemId);
                if (factionStorageItem is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }
            }

            factionStorageItem.Create(itemTemplate.Id, 0, quantity, null);

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.FactionsStoragesItems.AddAsync(factionStorageItem);
            else
                context.FactionsStoragesItems.Update(factionStorageItem);

            await context.SaveChangesAsync();

            if (isNew)
                Global.FactionsStoragesItems.Add(factionStorageItem);

            await player.WriteLog(LogType.Staff, $"Gravar Item Armazenamento Facção | {Functions.Serialize(factionStorageItem)}", null);
            player.SendNotification(NotificationType.Success, $"Item {(isNew ? "criado" : "editado")}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionStorageItemRemove))]
    public async Task StaffFactionStorageItemRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var factionStorageItem = Global.FactionsStoragesItems.FirstOrDefault(x => x.Id == id);
            if (factionStorageItem is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.FactionsStoragesItems.Remove(factionStorageItem);
            await context.SaveChangesAsync();
            Global.FactionsStoragesItems.Remove(factionStorageItem);

            await player.WriteLog(LogType.Staff, $"Remover Item Armazenamento Facção | {Functions.Serialize(factionStorageItem)}", null);
            player.SendNotification(NotificationType.Success, "Item excluído.");
            StaffFactionsStoragesItemsShow(player, factionStorageItem.FactionStorageId.ToString());
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetFactionsStoragesJson()
    {
        return Functions.Serialize(Global.FactionsStorages
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Dimension,
                Faction = Global.Factions.FirstOrDefault(y => y.Id == x.FactionId)!.Name,
            }));
    }

    private static string GetFactionStoragesItemsJson(Guid factionStorageId)
    {
        return Functions.Serialize(Global.FactionsStoragesItems
            .Where(x => x.FactionStorageId == factionStorageId)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.ItemTemplateId,
                Name = x.GetName(),
                x.Quantity,
                x.Price,
            }));
    }
}