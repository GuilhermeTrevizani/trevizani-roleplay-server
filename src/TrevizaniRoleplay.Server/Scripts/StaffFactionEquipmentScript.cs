using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffFactionEquipmentScript : Script
{
    [Command("aequipamentos")]
    public static void CMD_aequipamentos(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        player.Emit("StaffFactionEquipment:Show", GetFactionsEquipmentsJson());
    }

    private static void UpdateFactionEquipments()
    {
        var json = GetFactionsEquipmentsJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.FactionsStorages)))
            target.Emit("StaffFactionEquipment:Update", json);
    }

    [RemoteEvent(nameof(StaffFactionEquipmentRemove))]
    public async Task StaffFactionEquipmentRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var factionEquipment = Global.FactionsEquipments.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (factionEquipment is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var context = Functions.GetDatabaseContext();
            if (factionEquipment.Items!.Count > 0)
                context.FactionsEquipmentsItems.RemoveRange(factionEquipment.Items);

            context.FactionsEquipments.Remove(factionEquipment);
            await context.SaveChangesAsync();
            Global.FactionsEquipments.Remove(factionEquipment);

            await player.WriteLog(LogType.Staff, $"Remover Equipamento | {Functions.Serialize(factionEquipment)}", null);
            player.SendNotification(NotificationType.Success, "Equipamento excluído.");
            UpdateFactionEquipments();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionEquipmentSave))]
    public async Task StaffFactionEquipmentSave(Player playerParam, string idString, string name, string factionName, bool propertyOrvehicle, bool swat, bool upr)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            name ??= string.Empty;
            if (name.Length < 1 || name.Length > 25)
            {
                player.SendNotification(NotificationType.Error, "Nome deve ter entre 1 e 25 caracteres.");
                return;
            }

            var faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == factionName?.ToLower());
            if (faction is null)
            {
                player.SendNotification(NotificationType.Error, $"Facção {factionName} não existe.");
                return;
            }

            var id = idString.ToGuid();
            if (Global.FactionsEquipments.Any(x => x.Id != id && x.FactionId == faction.Id && x.Name.ToLower() == name.ToLower()))
            {
                player.SendNotification(NotificationType.Error, "Já existe um registro com esse nome para essa facção.");
                return;
            }

            var isNew = !id.HasValue;
            var factionEquipment = new FactionEquipment();
            if (isNew)
            {
                factionEquipment.Create(faction!.Id, name, propertyOrvehicle, swat, upr);
            }
            else
            {
                factionEquipment = Global.FactionsEquipments.FirstOrDefault(x => x.Id == id);
                if (factionEquipment is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                factionEquipment.Update(faction!.Id, name, propertyOrvehicle, swat, upr);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.FactionsEquipments.AddAsync(factionEquipment);
            else
                context.FactionsEquipments.Update(factionEquipment);

            await context.SaveChangesAsync();

            if (isNew)
                Global.FactionsEquipments.Add(factionEquipment);

            await player.WriteLog(LogType.Staff, $"Gravar Equipamento | {Functions.Serialize(factionEquipment)}", null);
            player.SendNotification(NotificationType.Success, $"Equipamento {(isNew ? "criado" : "editado")}.");
            UpdateFactionEquipments();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionsEquipmentsItemsShow))]
    public static void StaffFactionsEquipmentsItemsShow(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var factionEquipment = Global.FactionsEquipments.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (factionEquipment is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.Emit("StaffFactionEquipmentItem:Show", GetFactionEquipmentsItemsJson(factionEquipment), Functions.GetItemsTemplatesResponse(), idString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionEquipmentItemSave))]
    public async Task StaffFactionEquipmentItemSave(Player playerParam, string factionEquipmentItemIdString, string factionEquipmentIdString,
        string itemTemplateIdString, int quantity)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == itemTemplateIdString.ToGuid());
            if (itemTemplate is null)
            {
                player.SendNotification(NotificationType.Error, $"Template {itemTemplateIdString} não existe.");
                return;
            }

            if (quantity <= 0)
            {
                player.SendNotification(NotificationType.Error, "Quantidade deve ser maior que 0.");
                return;
            }

            var factionEquipment = Global.FactionsEquipments.FirstOrDefault(x => x.Id == factionEquipmentIdString.ToGuid());
            if (factionEquipment is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var factionEquipmentItem = new FactionEquipmentItem();
            var factionEquipmentItemId = factionEquipmentItemIdString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(factionEquipmentItemIdString);
            if (isNew)
            {
                factionEquipmentItem.SetFactionEquipmentId(factionEquipment.Id);
            }
            else
            {
                factionEquipmentItem = factionEquipment.Items!.FirstOrDefault(x => x.Id == factionEquipmentItemId);
                if (factionEquipmentItem is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }
            }

            factionEquipmentItem.Create(itemTemplate.Id, 0, quantity, null);
            if (!factionEquipmentItem.GetIsStack())
                factionEquipmentItem.SetQuantity(1);

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.FactionsEquipmentsItems.AddAsync(factionEquipmentItem);
            else
                context.FactionsEquipmentsItems.Update(factionEquipmentItem);

            await context.SaveChangesAsync();

            if (isNew && !factionEquipment.Items!.Contains(factionEquipmentItem))
                factionEquipment.Items.Add(factionEquipmentItem);

            await player.WriteLog(LogType.Staff, $"Gravar Item Equipamento Facção | {Functions.Serialize(factionEquipmentItem)}", null);
            player.SendNotification(NotificationType.Success, $"Item {(isNew ? "criado" : "editado")}.");
            StaffFactionsEquipmentsItemsShow(player, factionEquipment.Id.ToString());
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionEquipmentItemRemove))]
    public async Task StaffFactionEquipmentItemRemove(Player playerParam, string factionEquipmentItemIdString, string factionEquipmentIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.FactionsStorages))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var factionEquipment = Global.FactionsEquipments.FirstOrDefault(x => x.Id == factionEquipmentIdString.ToGuid());
            if (factionEquipment is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var factionEquipmentItem = factionEquipment.Items!.FirstOrDefault(x => x.Id == factionEquipmentItemIdString.ToGuid());
            if (factionEquipmentItem is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.FactionsEquipmentsItems.Remove(factionEquipmentItem);
            await context.SaveChangesAsync();

            factionEquipment.Items!.Remove(factionEquipmentItem);

            await player.WriteLog(LogType.Staff, $"Remover Item Equipamento Facção | {Functions.Serialize(factionEquipmentItem)}", null);
            player.SendNotification(NotificationType.Success, "Item excluído.");
            StaffFactionsEquipmentsItemsShow(player, factionEquipment.Id.ToString());
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetFactionsEquipmentsJson()
    {
        return Functions.Serialize(Global.FactionsEquipments
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.PropertyOrVehicle,
                x.SWAT,
                x.UPR,
                FactionName = Global.Factions.FirstOrDefault(y => y.Id == x.FactionId)!.Name,
            }));
    }

    private static string GetFactionEquipmentsItemsJson(FactionEquipment factionEquipment)
    {
        return Functions.Serialize(factionEquipment.Items!
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.ItemTemplateId,
                Name = x.GetName(),
                x.Quantity,
            }));
    }
}