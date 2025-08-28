using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class InventoryScript : Script
{
    [Command(["inventario", "inv"], "Geral", "Abre o inventário")]
    public static void CMD_inventario(MyPlayer player) => player.ShowInventory();

    [RemoteEvent(nameof(ShowInventory))]
    public static void ShowInventory(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            CMD_inventario(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CloseInventory))]
    public static void CloseInventory(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.CloseInventory();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MoveItem))]
    public async Task MoveItem(Player playerParam, string idString, bool leftOrigin, bool leftTarget, int targetSlot, int quantity)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid() ?? Guid.Empty;
            if (id == Guid.Empty)
                return;

            var slot = Convert.ToByte(targetSlot);
            if (slot <= 0)
                return;

            if (quantity <= 0)
                return;

            if (player.IsActionsBlocked())
            {
                player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (leftOrigin == leftTarget)
            {
                if (leftOrigin)
                    await MoveLeftItem(player, id, slot);
                else if (player.InventoryShowType != InventoryShowType.Default)
                    await MoveRightItem(player, id, slot);
            }
            else
            {
                if (leftOrigin)
                {
                    if (player.InventoryShowType == InventoryShowType.Default)
                        DropItem(player, id, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Vehicle)
                        await StoreVehicleItem(player, id, slot, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Property)
                        await StorePropertyItem(player, id, slot, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Body)
                        StoreBodyItem(player);
                    else
                        UpdateCurrentInventory(player);
                }
                else
                {
                    if (player.IsInVehicle && player.InventoryShowType != InventoryShowType.Vehicle)
                    {
                        player.SendNotification(NotificationType.Error, "Você não pode pegar um item dentro de um veículo.");
                        UpdateCurrentInventory(player);
                        return;
                    }

                    if (player.InventoryShowType == InventoryShowType.Inspect)
                        await TakePlayerItem(player, id, slot, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Vehicle)
                        await GetVehicleItem(player, id, slot, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Property)
                        await GetPropertyItem(player, id, slot, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Default)
                        await GetGroundItem(player, id, slot, quantity);
                    else if (player.InventoryShowType == InventoryShowType.Body)
                        await GetBodyItem(player, id, slot, quantity);
                    else
                        UpdateCurrentInventory(player);
                }
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateCurrentInventory(MyPlayer player)
    {
        if (player.InventoryShowType == InventoryShowType.Default)
        {
            player.ShowInventory(update: true);
        }
        else if (player.InventoryShowType == InventoryShowType.Inspect)
        {

        }
        else if (player.InventoryShowType == InventoryShowType.Property)
        {
            var property = Global.Properties.FirstOrDefault(x => x.Id == player.InventoryRightTargetId);
            property?.ShowInventory(player, true);
        }
        else if (player.InventoryShowType == InventoryShowType.Vehicle)
        {
            var vehicle = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == player.InventoryRightTargetId);
            vehicle?.ShowInventory(player, true);

        }
        else if (player.InventoryShowType == InventoryShowType.Body)
        {
            var body = Global.Bodies.FirstOrDefault(x => x.Id == player.InventoryRightTargetId);
            body?.ShowInventory(player, true);
        }
    }

    private async Task MoveLeftItem(MyPlayer player, Guid id, byte slot)
    {
        var item = player.Items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (player.Items.Any(x => x.Slot == slot))
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        item.SetSlot(slot);

        var context = Functions.GetDatabaseContext();
        context.CharactersItems.Update(item);
        await context.SaveChangesAsync();
        UpdateCurrentInventory(player);
    }

    private async Task MoveRightItem(MyPlayer player, Guid id, byte slot)
    {
        if (player.InventoryShowType == InventoryShowType.Property)
        {
            var prop = Global.Properties.FirstOrDefault(x => x.Id == player.InventoryRightTargetId);
            if (prop is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            var item = prop.Items!.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            if (prop.Items!.Any(x => x.Slot == slot))
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            item.SetSlot(slot);

            var context = Functions.GetDatabaseContext();
            context.PropertiesItems.Update(item);
            await context.SaveChangesAsync();

            UpdateCurrentInventory(player);
        }
        else if (player.InventoryShowType == InventoryShowType.Vehicle)
        {
            var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == player.InventoryRightTargetId);
            if (veh is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            var item = veh.VehicleDB.Items!.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            if (veh.VehicleDB.Items!.Any(x => x.Slot == slot))
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            item.SetSlot(slot);

            var context = Functions.GetDatabaseContext();
            context.VehiclesItems.Update(item);
            await context.SaveChangesAsync();

            UpdateCurrentInventory(player);
        }
        else
        {
            player.SendNotification(NotificationType.Error, "Não é possível mover esse item dessa forma.");
            UpdateCurrentInventory(player);
        }
    }

    private static void DropItem(MyPlayer player, Guid id, int quantity)
    {
        var item = player.Items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (!Functions.CanDropItem(player.Faction, item)
            || string.IsNullOrWhiteSpace(item.GetObjectName())
            || item.GetCategory() == ItemCategory.BloodSample
            || GlobalFunctions.CheckIfIsBulletShell(item.GetCategory()))
        {
            player.SendNotification(NotificationType.Error, "Você não pode largar este item.");
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendNotification(NotificationType.Error, "Você não pode largar um item dentro de um veículo.");
            UpdateCurrentInventory(player);
            return;
        }

        player.DropItem = item;
        player.DropItemQuantity = quantity;
        player.CloseInventory();
        player.Emit("DropObject", item.GetObjectName(), 0);
    }

    [RemoteEvent(nameof(CancelDropItem))]
    public static void CancelDropItem(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.DropItem = null;
            player.DropItemQuantity = 0;
            player.SendNotification(NotificationType.Success, "Você cancelou o drop do item.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmDropItem))]
    public async Task ConfirmDropItem(Player playerParam, Vector3 position, Vector3 rotation)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsActionsBlocked())
            {
                player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.DropItem is null || !player.Items.Any(x => x.Id == player.DropItem.Id))
                return;

            if (position.X == 0 && position.Y == 0)
            {
                player.SendMessage(MessageType.Error, "Não foi possível recuperar a posição do item.");
                return;
            }

            var item = new Item();
            item.Create(player.DropItem.ItemTemplateId, player.DropItem.Subtype, player.DropItemQuantity, player.DropItem.Extra);
            item.SetPosition(player.GetDimension(), position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z);

            var context = Functions.GetDatabaseContext();
            await context.Items.AddAsync(item);
            await context.SaveChangesAsync();

            if (player.DropItem.GetIsStack())
                await player.RemoveStackedItem(player.DropItem.ItemTemplateId, player.DropItemQuantity);
            else
                await player.RemoveItem(player.DropItem);

            Global.Items.Add(item);
            item.CreateObject();

            player.DropItem = null;
            player.DropItemQuantity = 0;

            await player.WriteLog(LogType.DropItem, $"{item.Quantity}x {item.GetName()} | {Functions.Serialize(item)}", null);
            player.SendMessageToNearbyPlayers("larga algo no chão.", MessageCategory.Ame);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task StoreVehicleItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var item = player.Items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (!Functions.CanDropItem(player.Faction, item))
        {
            player.SendNotification(NotificationType.Error, "Você não pode armazenar este item.");
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == player.InventoryRightTargetId);
        if (veh is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (veh.VehicleDB.Items!.Count(x => x.Slot > 0)
            + ((!item.GetIsStack() || !veh.VehicleDB.Items!.Any(x => x.ItemTemplateId == item.ItemTemplateId)) ? 1 : 0)
            > Constants.MAX_INVENTORY_SLOTS)
        {
            player.SendNotification(NotificationType.Error, $"Não é possível prosseguir pois os novos itens ultrapassarão a quantidade de slots do armazenamento ({Constants.MAX_INVENTORY_SLOTS}).");
            UpdateCurrentInventory(player);
            return;
        }

        if (veh.VehicleDB.Items!.Any(x => x.Slot == slot))
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        var context = Functions.GetDatabaseContext();
        VehicleItem? it = null;
        if (item.GetIsStack())
        {
            it = veh.VehicleDB.Items!.FirstOrDefault(x => x.ItemTemplateId == item.ItemTemplateId);
            if (it is not null)
            {
                it.SetQuantity(it.Quantity + quantity);
                context.VehiclesItems.Update(it);
            }
        }

        if (it is null)
        {
            it = new VehicleItem();
            it.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
            it.SetVehicleId(player.InventoryRightTargetId!.Value);
            it.SetSlot(slot);

            await context.VehiclesItems.AddAsync(it);

            if (!veh.VehicleDB.Items!.Contains(it))
                veh.VehicleDB.Items.Add(it);
        }

        await context.SaveChangesAsync();
        if (item.GetIsStack())
            await player.RemoveStackedItem(item.ItemTemplateId, quantity);
        else
            await player.RemoveItem(item);
        await player.WriteLog(LogType.PutVehicleItem, $"{veh.Identifier} {quantity}x {item.GetName()} | {Functions.Serialize(item)}", null);

        player.SendMessageToNearbyPlayers("coloca algo no veículo.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você armazenou {quantity:N0}x {item.GetName()} no veículo {veh.VehicleDB.Model.ToUpper()} {veh.VehicleDB.Plate.ToUpper()}.");

        UpdateCurrentInventory(player);
    }

    private async Task StorePropertyItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var item = player.Items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (!Functions.CanDropItem(player.Faction, item))
        {
            player.SendNotification(NotificationType.Error, "Você não pode armazenar este item.");
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        var prop = Global.Properties.FirstOrDefault(x => x.Id == player.InventoryRightTargetId);
        if (prop is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (prop.Items!.Count(x => x.Slot > 0)
            + ((!item.GetIsStack() || !prop.Items!.Any(x => x.ItemTemplateId == item.ItemTemplateId)) ? 1 : 0)
            > Constants.MAX_PROPERTY_INVENTORY_SLOTS)
        {
            player.SendNotification(NotificationType.Error, $"Não é possível prosseguir pois os novos itens ultrapassarão a quantidade de slots do armazenamento ({Constants.MAX_INVENTORY_SLOTS}).");
            UpdateCurrentInventory(player);
            return;
        }

        if (prop.Items!.Any(x => x.Slot == slot))
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        var context = Functions.GetDatabaseContext();
        PropertyItem? it = null;
        if (item.GetIsStack())
        {
            it = prop.Items!.FirstOrDefault(x => x.ItemTemplateId == item.ItemTemplateId);
            if (it is not null)
            {
                it.SetQuantity(it.Quantity + quantity);
                context.PropertiesItems.Update(it);
            }
        }

        if (it is null)
        {
            it = new PropertyItem();
            it.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
            it.SetPropertyId(player.InventoryRightTargetId!.Value);
            it.SetSlot(slot);

            await context.PropertiesItems.AddAsync(it);

            if (!prop.Items!.Contains(it))
                prop.Items.Add(it);
        }

        await context.SaveChangesAsync();
        if (item.GetIsStack())
            await player.RemoveStackedItem(item.ItemTemplateId, quantity);
        else
            await player.RemoveItem(item);
        await player.WriteLog(LogType.PutPropertyItem, $"{prop.FormatedAddress} ({prop.Number}) {quantity}x {item.GetName()} | {Functions.Serialize(item)}", null);

        player.SendMessageToNearbyPlayers("coloca algo na propriedade.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você armazenou {quantity}x {item.GetName()} em {prop.FormatedAddress}.");

        UpdateCurrentInventory(player);
    }

    private static void StoreBodyItem(MyPlayer player)
    {
        player.SendNotification(NotificationType.Error, "Não é possível colocar itens em um corpo.");
        UpdateCurrentInventory(player);
    }

    private async Task GetVehicleItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == player.InventoryRightTargetId);
        if (veh is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        var item = veh.VehicleDB.Items!.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        var itemTarget = new CharacterItem();
        itemTarget.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
        itemTarget.SetSlot(slot);

        var context = Functions.GetDatabaseContext();
        var res = await player.GiveItem(itemTarget);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendNotification(NotificationType.Error, res);
            UpdateCurrentInventory(player);
            return;
        }

        item.SetQuantity(item.Quantity - quantity);
        if (item.Quantity == 0)
        {
            context.VehiclesItems.Remove(item);
            await context.SaveChangesAsync();
            veh.VehicleDB.Items!.Remove(item);
        }

        player.SendMessageToNearbyPlayers("pega algo do veículo.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você pegou {quantity:N0}x {item.GetName()} do veículo {veh.VehicleDB.Model.ToUpper()} {veh.VehicleDB.Plate.ToUpper()}.");

        await player.WriteLog(LogType.GetVehicleItem, $"{veh.Identifier} {quantity}x {item.GetName()} | {Functions.Serialize(item)}", null);

        UpdateCurrentInventory(player);
    }

    private async Task GetPropertyItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var prop = Global.Properties.FirstOrDefault(x => x.Id == player.InventoryRightTargetId);
        if (prop is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        var item = prop.Items!.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        var itemTarget = new CharacterItem();
        itemTarget.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
        itemTarget.SetSlot(slot);

        var res = await player.GiveItem(itemTarget);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendNotification(NotificationType.Error, res);
            UpdateCurrentInventory(player);
            return;
        }

        item.SetQuantity(item.Quantity - quantity);
        if (item.Quantity == 0)
        {
            var context = Functions.GetDatabaseContext();
            context.PropertiesItems.Remove(item);
            await context.SaveChangesAsync();
            prop.Items!.Remove(item);
        }

        player.SendMessageToNearbyPlayers("pega algo da propriedade.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você pegou {quantity:N0}x {item.GetName()} de {prop.FormatedAddress}.");

        await player.WriteLog(LogType.GetPropertyItem, $"{prop.FormatedAddress} ({prop.Number}) {quantity}x {item.GetName()} | {Functions.Serialize(item)}", null);

        UpdateCurrentInventory(player);
    }

    private async Task GetGroundItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var item = Global.Items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está próximo deste item.");
            UpdateCurrentInventory(player);
            return;
        }

        if (item.ItemTemplateId == Constants.BLOOD_SAMPLE_ITEM_TEMPLATE_ID.ToGuid())
        {
            if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial ou não está em serviço.");
                UpdateCurrentInventory(player);
                return;
            }
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        if (GlobalFunctions.CheckIfIsBulletShell(item.GetCategory()))
        {
            var temperature = Functions.GetBulletShellTemperature(item.Extra!);
            if (temperature == Resources.Hot)
            {
                player.Health -= 2;
                player.SendMessageToNearbyPlayers("se queima ao tentar pegar uma cápsula quente.", MessageCategory.Ame);
                player.SendNotification(NotificationType.Error, "Você se queimou ao tentar pegar uma cápsula quente.");
                UpdateCurrentInventory(player);
                return;
            }
        }

        var itemTarget = new CharacterItem();
        itemTarget.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
        itemTarget.SetSlot(slot);

        var context = Functions.GetDatabaseContext();
        var res = await player.GiveItem(itemTarget);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendNotification(NotificationType.Error, res);
            return;
        }

        item.SetQuantity(item.Quantity - quantity);
        if (item.Quantity == 0)
        {
            item.DeleteObject();
            context.Items.Remove(item);
        }
        else
        {
            context.Items.Update(item);
        }
        await context.SaveChangesAsync();

        player.SendMessageToNearbyPlayers("pega algo do chão.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você pegou {quantity}x {item.GetName()} do chão.");
        await player.WriteLog(LogType.GetGroundItem, $"{quantity}x {item.GetName()} | {Functions.Serialize(itemTarget)}", null);
    }

    private async Task GetBodyItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var body = Global.Bodies.FirstOrDefault(x => x.Id == player.InventoryRightTargetId);
        if (body is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        var item = body.Items!.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        var itemTarget = new CharacterItem();
        itemTarget.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
        itemTarget.SetSlot(slot);

        var res = await player.GiveItem(itemTarget);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendNotification(NotificationType.Error, res);
            UpdateCurrentInventory(player);
            return;
        }

        item.SetQuantity(item.Quantity - quantity);
        if (item.Quantity == 0)
        {
            var context = Functions.GetDatabaseContext();
            context.BodiesItems.Remove(item);
            await context.SaveChangesAsync();
            body.Items!.Remove(item);
        }

        player.SendMessageToNearbyPlayers("pega algo do corpo.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você pegou {quantity:N0}x {item.GetName()} do corpo.");

        await player.WriteLog(LogType.GetBodyItem, $"{body.Id} {body.Name} {quantity}x {itemTarget.GetName()} | {Functions.Serialize(itemTarget)}", null);

        UpdateCurrentInventory(player);
    }

    private async Task TakePlayerItem(MyPlayer player, Guid id, byte slot, int quantity)
    {
        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == player.InventoryRightTargetId);
        if (target is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        var item = target.Items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
            UpdateCurrentInventory(player);
            return;
        }

        if (!Functions.CanDropItem(target.Faction, item))
        {
            player.SendNotification(NotificationType.Error, "Você não pode pegar este item.");
            UpdateCurrentInventory(player);
            return;
        }

        if (quantity > item.Quantity)
        {
            player.SendMessage(MessageType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
            UpdateCurrentInventory(player);
            return;
        }

        if (player.GetPosition().DistanceTo(target.GetPosition()) > Constants.RP_DISTANCE || player.GetDimension() != target.GetDimension())
        {
            player.SendNotification(NotificationType.Error, "Jogador não está próximo de você.");
            UpdateCurrentInventory(player);
            return;
        }

        var itemTarget = new CharacterItem();
        itemTarget.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);
        itemTarget.SetSlot(slot);

        var res = await player.GiveItem(itemTarget);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendNotification(NotificationType.Error, res);
            UpdateCurrentInventory(player);
            return;
        }

        if (item.GetIsStack())
            await target.RemoveStackedItem(item.ItemTemplateId, quantity);
        else
            await target.RemoveItem(item);

        await player.WriteLog(LogType.StealItem, $"{quantity}x {itemTarget.GetName()} | {Functions.Serialize(itemTarget)}", target);

        player.SendMessageToNearbyPlayers($"pega algo de {target.ICName}.", MessageCategory.Ame);
        player.SendNotification(NotificationType.Success, $"Você pegou {quantity:N0}x {itemTarget.GetName()} de {target.ICName}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} pegou {quantity:N0}x {itemTarget.GetName()} de você.");
    }

    [RemoteEvent(nameof(InventoryGetNearbyCharacters))]
    public static void InventoryGetNearbyCharacters(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var characters = Global.SpawnedPlayers
                .Where(x => player.CheckIfTargetIsCloseIC(x, Constants.RP_DISTANCE) && x != player)
                .OrderBy(x => x.GetPosition().DistanceTo(player.GetPosition()))
                .Take(5)
                .Select(x => new
                {
                    x.SessionId,
                    x.ICName,
                });
            if (!characters.Any())
            {
                player.SendNotification(NotificationType.Error, "Não há nenhum jogador perto de você.");
                return;
            }

            player.Emit(Constants.INVENTORY_PAGE_GET_NEARBY_CHARACTERS, Functions.Serialize(characters));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(GiveItem))]
    public async Task GiveItem(Player playerParam, string id, int quantity, int targetSessionId)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsActionsBlocked())
            {
                player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                UpdateCurrentInventory(player);
                return;
            }

            var item = player.Items.FirstOrDefault(x => x.Id == new Guid(id));
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                UpdateCurrentInventory(player);
                return;
            }

            if (!Functions.CanDropItem(player.Faction, item))
            {
                player.SendNotification(NotificationType.Error, "Você não pode entregar este item.");
                UpdateCurrentInventory(player);
                return;
            }

            if (quantity <= 0 || quantity > item.Quantity)
            {
                player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
                UpdateCurrentInventory(player);
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == targetSessionId && x.Character.Id != player.Character.Id);
            if (target is null)
            {
                player.SendNotification(NotificationType.Error, "Jogador inválido.");
                UpdateCurrentInventory(player);
                return;
            }

            if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
            {
                player.SendNotification(NotificationType.Error, "Jogador não está próximo de você.");
                UpdateCurrentInventory(player);
                return;
            }

            var itemTarget = new CharacterItem();
            itemTarget.Create(item.ItemTemplateId, item.Subtype, quantity, item.Extra);

            var res = await target.GiveItem(itemTarget);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                UpdateCurrentInventory(player);
                return;
            }

            if (item.GetIsStack())
                await player.RemoveStackedItem(item.ItemTemplateId, quantity);
            else
                await player.RemoveItem(item);

            await player.WriteLog(LogType.DeliverItem, $"{quantity}x {itemTarget.GetName()} | {Functions.Serialize(itemTarget)}", target);

            player.SendMessageToNearbyPlayers($"entrega algo para {target.ICName}.", MessageCategory.Ame);
            var msgSucesso = $"Você entregou {quantity:N0}x {itemTarget.GetName()} para {target.ICName}.";
            player.SendNotification(NotificationType.Success, msgSucesso);

            if (itemTarget.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID))
                target.SendMessage(MessageType.Success, $"{player.ICName} entregou para você ${quantity:N0}.");
            else
                target.SendMessage(MessageType.Success, $"{player.ICName} entregou para você {quantity:N0}x {itemTarget.GetName()}.");

            UpdateCurrentInventory(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(UseItem))]
    public async Task UseItem(Player playerParam, string id, int quantity)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsActionsBlocked())
            {
                player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            var item = player.Items.FirstOrDefault(x => x.Id == new Guid(id));
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (quantity <= 0 || quantity > item.Quantity)
            {
                player.SendNotification(NotificationType.Error, $"Quantidade deve ser entre 1 e {item.Quantity}.");
                return;
            }

            if (!item.GetIsUsable())
            {
                player.SendNotification(NotificationType.Error, "Item não é usável.");
                return;
            }

            var category = item.GetCategory();
            if (category == ItemCategory.Drug)
            {
                await UseDrug(player, item, quantity);
            }
            else if (category == ItemCategory.Weapon)
            {
                if (item.GetItemType() == (uint)WeaponModel.JerryCan)
                    await UseJerryCan(player, item);
                else
                    UseWeapon(player, item);
            }
            else if (category == ItemCategory.WalkieTalkie)
            {
                item.SetInUse(!item.InUse);
                if (player.Items.Count(x => x.GetCategory() == category && x.InUse) > 1)
                {
                    player.SendNotification(NotificationType.Error, "Você não pode usar dois rádio comunicadores ao mesmo tempo.");
                    item.SetInUse(false);
                    return;
                }

                player.WalkieTalkieItem = item.InUse ? Functions.Deserialize<WalkieTalkieItem>(item.Extra!) : new();
            }
            else if (category == ItemCategory.Cellphone)
            {
                item.SetInUse(!item.InUse);
                if (player.Items.Count(x => x.GetCategory() == category && x.InUse) > 1)
                {
                    player.SendNotification(NotificationType.Error, "Você não pode usar dois celulares ao mesmo tempo.");
                    item.SetInUse(false);
                    return;
                }

                if (item.InUse)
                {
                    player.Character.SetCellphone(item.Subtype);
                    player.CellphoneItem = Functions.Deserialize<CellphoneItem>(item.Extra!);
                }
                else
                {
                    player.Character.SetCellphone(0);
                    player.CellphoneItem = new CellphoneItem();
                    await player.EndCellphoneCall();
                }
                await player.ConfigureCellphone();
            }
            else if (category == ItemCategory.Food)
            {
                if (player.Wounds.Any(x => (DateTime.Now - x.Date).TotalMinutes <= 15))
                {
                    player.SendNotification(NotificationType.Error, "Você possui ferimentos nos últimos 15 minutos, portanto a comida não surtirá efeito.");
                }
                else
                {
                    if (player.GetHealth() < player.MaxHealth)
                        player.SetHealth(Math.Min(75, player.GetHealth() + 5 * quantity));
                }

                await player.RemoveStackedItem(item.ItemTemplateId, quantity);
                await player.WriteLog(LogType.UseItem, $"{item.Id} {item.ItemTemplateId} {item.GetName()} {quantity}", null);
                player.SendNotification(NotificationType.Success, $"Você consumiu {quantity}x {item.GetName()}.");
                player.SendMessageToNearbyPlayers("consome algo.", MessageCategory.Ame);
            }
            else if (category == ItemCategory.FishingRod)
            {
                if (player.Fishing)
                {
                    player.SendNotification(NotificationType.Error, "Você já está pescando.");
                    return;
                }

                player.Emit("Fishing:Start");
                player.CloseInventory();
                return;
            }
            else if (category == ItemCategory.Bandage)
            {
                quantity = 1;

                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.GetDimension() == player.GetDimension()
                    && x.Character.Bleeding
                    && x.GetPosition().DistanceTo(player.GetPosition()) <= Constants.RP_DISTANCE);
                if (target is null)
                {
                    player.SendNotification(NotificationType.Error, "Você não está próximo de alguém que está sangrando.");
                    return;
                }

                target.StopBleeding();
                await player.RemoveStackedItem(item.ItemTemplateId, quantity);
                await player.WriteLog(LogType.UseItem, $"{item.Id} {item.ItemTemplateId} {item.GetName()}", null);
                player.SendMessage(MessageType.Success, $"Você usou a bandagem e estancou o sangramento de {target.ICName}.");
                target.SendMessage(MessageType.Success, $"{player.ICName} usou a bandagem e estancou seu sangramento.");
            }
            else if (GlobalFunctions.CheckIfIsAmmo(category))
            {
                if (!item.InUse && player.FactionEquippedWeapons.Count > 0)
                {
                    player.SendNotification(NotificationType.Error, "Você não pode equipar este item pois está com armas de serviço.");
                    return;
                }

                var weapon = player.Items.FirstOrDefault(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
                    && Functions.GetAmmoItemTemplateIdByWeapon(x.GetItemType()) == item.ItemTemplateId);
                if (weapon is null)
                {
                    player.SendNotification(NotificationType.Error, "Você não possui uma arma equipada para esse tipo de munição.");
                    return;
                }

                item.SetInUse(!item.InUse);
                player.SetWeaponAmmo((WeaponHash)weapon.GetItemType(), Convert.ToUInt16(item.InUse ? item.Quantity : 0));
            }
            else if (category == ItemCategory.WeaponComponent)
            {
                if (!item.InUse && player.FactionEquippedWeapons.Count > 0)
                {
                    player.SendNotification(NotificationType.Error, "Você não pode equipar este item pois está com armas de serviço.");
                    return;
                }

                var weapons = player.Items.Where(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
                    && Functions.GetComponentsItemsTemplatesByWeapon(x.GetItemType()).Contains(item.ItemTemplateId))
                    .ToList();
                if (weapons.Count == 0)
                {
                    player.SendNotification(NotificationType.Error, "Você não possui uma arma equipada para esse componente.");
                    return;
                }

                if (weapons.Count > 1)
                {
                    player.Emit("InventoryPage:UseItemSelectServer", item.Id.ToString(), Functions.Serialize(
                        weapons.Select(x => new
                        {
                            Value = x.Id,
                            Label = x.GetName()
                        })
                        .OrderBy(x => x.Label)));
                    return;
                }

                var weapon = weapons.FirstOrDefault()!;

                item.SetInUse(!item.InUse);
                if (item.InUse)
                    player.AddWeaponComponent(weapon.GetItemType(), item.GetItemType());
                else
                    player.RemoveWeaponComponent(weapon.GetItemType(), item.GetItemType());
            }
            else
            {
                player.SendNotification(NotificationType.Error, "Item não possui ação de uso programada. Por favor, reporte o bug.");
            }
            UpdateCurrentInventory(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task UseDrug(MyPlayer player, CharacterItem item, int quantity)
    {
        player.CloseInventory();

        if (player.Character.DrugItemTemplateId.HasValue && player.Character.DrugItemTemplateId != item.ItemTemplateId)
        {
            player.SendMessage(MessageType.Error, $"Você está sob efeito de {Global.ItemsTemplates.FirstOrDefault(x => x.Id == player.Character.DrugItemTemplateId)?.Name}. Não é possível usar {item.GetName()}.");
            return;
        }

        var drug = Global.Drugs.FirstOrDefault(x => x.ItemTemplateId == item.ItemTemplateId);
        if (drug is null)
        {
            player.SendMessage(MessageType.Error, "Droga não configurada. Por favor, reporte o bug.");
            return;
        }

        var thresoldDeath = drug.ThresoldDeath * quantity;
        var minutesDuration = drug.MinutesDuration * quantity;
        var health = drug.Health * quantity;

        await player.RemoveStackedItem(item.ItemTemplateId, quantity);

        player.Character.UseDrug(item.ItemTemplateId, thresoldDeath, minutesDuration);

        if (!string.IsNullOrWhiteSpace(drug.Warn))
            player.SendMessage(MessageType.Error, drug.Warn);

        if (health > 0)
            player.SetHealth(player.GetHealth() + health);

        player.SendMessage(MessageType.Success, $"Você usou {quantity}x {item.GetName()} e seu limiar da morte está em {player.Character.ThresoldDeath}/100.");
        player.SetupDrugTimer(true);

        if (player.Character.ThresoldDeath == 100)
        {
            player.SetHealth(0);
            player.SendMessage(MessageType.Error, "Você atingiu 100 da limiar de morte e sofreu uma overdose.");
        }
    }

    private async Task UseJerryCan(MyPlayer player, CharacterItem jerryCan)
    {
        if (player.IsInVehicle)
        {
            player.SendNotification(NotificationType.Error, "Você deve estar fora do veículo.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x => player.GetPosition().DistanceTo(x.GetPosition()) <= 5
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está próximo de nenhum veículo destrancado.");
            return;
        }

        if (vehicle.VehicleDB.Fuel == vehicle.VehicleDB.GetMaxFuel())
        {
            player.SendNotification(NotificationType.Error, "Veículo está com tanque cheio.");
            return;
        }

        vehicle.SetFuel(vehicle.VehicleDB.GetMaxFuel());

        await player.RemoveItem(jerryCan);

        player.SendNotification(NotificationType.Success, "Você usou um galão de combustível e abasteceu seu veículo.");
        player.SendMessageToNearbyPlayers("abastece o veículo usando um galão de combustível.", MessageCategory.Ame);
    }

    private static void UseWeapon(MyPlayer player, CharacterItem item)
    {
        if (!item.InUse)
        {
            if (player.FactionEquippedWeapons.Count > 0)
            {
                player.SendNotification(NotificationType.Error, "Você não pode equipar este item pois está com armas de serviço.");
                return;
            }

            var ammoItemTemplateId = Functions.GetAmmoItemTemplateIdByWeapon(item.GetItemType());
            if (ammoItemTemplateId is not null)
            {
                var weapons = Global.WeaponsInfos.Where(x => x.AmmoItemTemplateId == ammoItemTemplateId);
                if (weapons.Any(x => player.HasWeapon(GlobalFunctions.GetWeaponType(x.Name))))
                {
                    player.SendNotification(NotificationType.Error, "Você já possui uma arma com o mesmo tipo de munição equipada.");
                    return;
                }
            }

            if (!item.InUse)
            {
                var weaponInfo = Global.WeaponsInfos.FirstOrDefault(x => x.Name == GlobalFunctions.GetWeaponName(item.GetItemType()));
                if (weaponInfo?.AttachToBody == true
                    && Global.WeaponsInfos.Where(x => x.AttachToBody).Any(x => player.HasWeapon(GlobalFunctions.GetWeaponType(x.Name))))
                {
                    player.SendNotification(NotificationType.Error, "Você já possui uma arma grande equipada.");
                    return;
                }
            }
        }

        item.SetInUse(!item.InUse);
        if (item.InUse)
            player.GiveWeapon(item);
        else
            player.RemoveWeapon(item);
    }

    [RemoteEvent(nameof(InventoryUseItemSelect))]
    public static void InventoryUseItemSelect(Player playerParam, string idString, string targetIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsActionsBlocked())
            {
                player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            var item = player.Items.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var weapon = player.Items.FirstOrDefault(x => x.Id == targetIdString.ToGuid()
                && x.InUse && x.GetCategory() == ItemCategory.Weapon
                && Functions.GetComponentsItemsTemplatesByWeapon(x.GetItemType()).Contains(item.ItemTemplateId));
            if (weapon is null)
            {
                player.SendNotification(NotificationType.Error, "Você não possui essa arma equipada para esse componente.");
                return;
            }

            item.SetInUse(!item.InUse);
            if (item.InUse)
                player.AddWeaponComponent(weapon.GetItemType(), item.GetItemType());
            else
                player.RemoveWeaponComponent(weapon.GetItemType(), item.GetItemType());
            UpdateCurrentInventory(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(UpdateWeaponAmmo))]
    public async Task UpdateWeaponAmmo(Player playerParam, string weaponString, Vector3 position)
    {
        try
        {
            var weapon = Convert.ToUInt32(weaponString);
            var player = Functions.CastPlayer(playerParam);
            var weap = (WeaponModel)weapon;
            var ammo = player.GetWeaponAmmo(weapon);
            if (weap == WeaponModel.Snowballs || weap == WeaponModel.Fist)
                return;

            var weaponItem = player.Items.FirstOrDefault(x => x.InUse && x.GetCategory() == ItemCategory.Weapon && x.GetItemType() == weapon);
            if (weaponItem is null)
            {
                if (!player.FactionEquippedWeapons.Contains(weapon))
                {
                    await Functions.SendServerMessage($"{player.Character.Name} tem a arma {weap} com munição {ammo} sem um item no inventário.", UserStaff.GameAdmin, true);
                    await player.WriteLog(LogType.Hack, $"Arma sem item no inventário | {weap} | {ammo}", null);
                    await player.ListCharacters("Anti-cheat", "Arma sem item no inventário.");
                    return;
                }
            }

            if (weap == WeaponModel.FireExtinguisher)
            {
                if (ammo < 10_000)
                    player.SetWeaponAmmo(weapon, 10_000);

                foreach (var activeFire in Global.ActiveFires)
                {
                    var span = activeFire.Spans.FirstOrDefault(x => x.ParticleFire.Dimension == player.GetDimension()
                        && player.GetPosition().DistanceTo(x.ParticleFire.Position) <= 5);
                    if (span is null)
                        continue;

                    span.Life -= activeFire.Fire.FireSpanDamage;
                    if (span.Life > 0)
                        continue;

                    span.Destroy();
                    activeFire.Spans.Remove(span);

                    if (activeFire.Spans.Count == 0)
                        activeFire.Stop();
                }

                return;
            }

            if (weaponItem is null)
                return;

            var ammoItemTemplateId = Functions.GetAmmoItemTemplateIdByWeapon(weapon);
            if (ammoItemTemplateId is not null)
            {
                var ammoItem = player.Items.FirstOrDefault(x => x.InUse && x.ItemTemplateId == ammoItemTemplateId);
                if (ammoItem is null)
                {
                    if (ammo == 0)
                    {
                        await player.RemoveItem(weaponItem);
                        return;
                    }

                    await Functions.SendServerMessage($"{player.Character.Name} tem munição {ammo} da arma {weap} sem um item no inventário.", UserStaff.GameAdmin, true);
                    await player.WriteLog(LogType.Hack, $"Munição sem item no inventário | {weap} | {ammo}", null);
                    await player.ListCharacters("Anti-cheat", "Munição sem item no inventário.");
                    return;
                }

                await player.RemoveStackedItem(ammoItem.ItemTemplateId, 1, false);

                await CreateBulletShell(player, position, ammoItem, weaponItem);
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task CreateBulletShell(MyPlayer player, Vector3 position, CharacterItem ammoItem, CharacterItem weaponItem)
    {
        var bulletShellItemTemplateId = ammoItem.ItemTemplateId.ToString() switch
        {
            Constants.PISTOL_AMMO_ITEM_TEMPLATE_ID => Constants.PISTOL_BULLET_SHELL_ITEM_TEMPLATE_ID,
            Constants.SHOTGUN_AMMO_ITEM_TEMPLATE_ID => Constants.SHOTGUN_BULLET_SHELL_ITEM_TEMPLATE_ID,
            Constants.ASSAULT_RIFLE_AMMO_ITEM_TEMPLATE_ID => Constants.ASSAULT_RIFLE_BULLET_SHELL_ITEM_TEMPLATE_ID,
            Constants.LIGHT_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID => Constants.LIGHT_MACHINE_GUN_BULLET_SHELL_ITEM_TEMPLATE_ID,
            Constants.SNIPER_RIFLE_AMMO_ITEM_TEMPLATE_ID => Constants.SNIPER_RIFLE_BULLET_SHELL_ITEM_TEMPLATE_ID,
            Constants.SUB_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID => Constants.SUB_MACHINE_GUN_BULLET_SHELL_ITEM_TEMPLATE_ID,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(bulletShellItemTemplateId))
            return;

        var type = weaponItem.GetItemType();
        if (type == (uint)WeaponModel.NavyRevolver
            || type == (uint)WeaponModel.HeavyRevolver
            || type == (uint)WeaponModel.HeavyRevolverMkII
            || type == (uint)WeaponModel.DoubleActionRevolver)
            return;

        var weaponItemInfo = Functions.Deserialize<WeaponItem>(weaponItem.Extra!);

        var item = new Item();
        item.Create(new Guid(bulletShellItemTemplateId), 0, 1, Functions.Serialize(new BulletShellItem
        {
            WeaponItemId = weaponItemInfo.Id,
        }));

        item.SetPosition(player.GetDimension(), position.X, position.Y, position.Z, 0, 0, 0);

        var context = Functions.GetDatabaseContext();
        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        Global.Items.Add(item);
        item.CreateObject();
    }

    [Command(["pagar"], "Geral", "Entrega dinheiro para um personagem próximo", "(ID ou nome) (quantidade)")]
    public async Task CMD_pagar(MyPlayer player, string idOrName, int quantity)
    {
        var item = player.Items.FirstOrDefault(x => x.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID));
        if (item is null)
        {
            player.SendMessage(MessageType.Error, "Você não possui dinheiro no inventário.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        await GiveItem(player, item.Id.ToString(), quantity, target.SessionId);
    }

    [RemoteEvent(nameof(StartFishing))]
    public async Task StartFishing(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Fishing)
            {
                player.SendMessage(MessageType.Error, "Você já está pescando.");
                return;
            }

            if (!player.Items.Any(x => x.GetCategory() == ItemCategory.FishingRod))
            {
                player.SendMessage(MessageType.Error, "Você não possui uma vara de pesca.");
                return;
            }

            player.Fishing = true;
            player.AttachObject(Constants.FISHING_ROD_OBJECT_MODEL, 60309, new(0, -0.01f, 0.01f), new());
            player.PlayAnimation("amb@world_human_stand_fishing@idle_a", "idle_c", (int)AnimationFlags.Loop, true);

            var randomNumber = new Random().Next(5, 16);
            await Task.Delay(TimeSpan.FromSeconds(randomNumber));

            player.Emit("Circle:StartMinigame", 3, 0.01f, 0.5f, "fa-solid fa-fish", nameof(EndFishing));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(EndFishing))]
    public async Task EndFishing(Player playerParam, bool success)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.Fishing)
            {
                player.SendMessage(MessageType.Error, "Você não está pescando.");
                return;
            }

            if (!player.Items.Any(x => x.GetCategory() == ItemCategory.FishingRod))
            {
                player.SendMessage(MessageType.Error, "Você não possui uma vara de pesca.");
                return;
            }

            player.DetachObject(Constants.FISHING_ROD_OBJECT_MODEL);
            player.StopAnimationEx();
            player.Fishing = false;

            if (!success)
            {
                player.SendMessage(MessageType.Error, "Você perdeu o que estava na sua vara.");
                return;
            }

            string? itemTemplateName = null;
            var randomNumber = new Random().NextSingle() * 100;
            var sum = 0f;
            foreach (var chance in Global.FishingItemsChances.OrderByDescending(x => x.Percentage))
            {
                sum += chance.Percentage;
                if (randomNumber < sum)
                {
                    itemTemplateName = chance.ItemTemplateName;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(itemTemplateName))
            {
                player.SendMessage(MessageType.Error, "Probabilidades de itens na pesca não configuradas. Por favor, reporte o bug.");
                return;
            }

            var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Name.ToLower() == itemTemplateName.ToLower());
            if (itemTemplate is null)
            {
                player.SendMessage(MessageType.Error, $"Item {itemTemplateName} não encontrado. Por favor, reporte o bug.");
                return;
            }

            var characterItem = new CharacterItem();
            characterItem.Create(itemTemplate.Id, 0, 1, null);
            var res = await player.GiveItem(characterItem);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendMessage(MessageType.Error, res);
                return;
            }

            player.SendMessage(MessageType.Success, $"Você pescou {characterItem.GetName()}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["armacorpo", "armac"], "Geral", "Altera posicionamento da arma acoplada ao corpo")]
    public static void CMD_armacorpo(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        var weaponOnBody = player.Items.FirstOrDefault(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
            && Global.WeaponsInfos.FirstOrDefault(y => y.Name == GlobalFunctions.GetWeaponName(x.GetItemType()))?.AttachToBody == true);
        if (string.IsNullOrWhiteSpace(player.AttachedWeapon) || weaponOnBody is null)
        {
            player.SendMessage(MessageType.Error, "Você não está com uma arma no corpo.");
            return;
        }

        var weaponName = GlobalFunctions.GetWeaponName(weaponOnBody.GetItemType());
        var weaponBody = Functions.Deserialize<List<WeaponBody>>(player.Character.WeaponsBodyJSON)
            .FirstOrDefault(x => x.Name == weaponName);
        weaponBody = Functions.CheckDefaultWeaponBody(weaponBody);

        player.DetachObject(player.AttachedWeapon);

        player.Emit("WeaponBody:Show", weaponName, player.AttachedWeapon, Functions.Serialize(weaponBody));
    }

    [ServerEvent(Event.PlayerWeaponSwitch)]
    public static void OnPlayerWeaponChange(Player playerParam, WeaponHash oldWeapon, WeaponHash newWeapon)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (newWeapon == WeaponHash.Fireextinguisher)
                player.SetWeaponAmmo(newWeapon, 10000);
            else if (oldWeapon == WeaponHash.Snowball)
                player.RemoveWeapon(oldWeapon);

            player.CheckAttachedWeapon((uint)newWeapon);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SaveWeaponBody))]
    public static void SaveWeaponBody(Player playerParam, bool success, string weaponName, string attachedWeapon, string json)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.AttachedWeapon = attachedWeapon;

            if (success)
            {
                var weaponsBody = Functions.Deserialize<List<WeaponBody>>(player.Character.WeaponsBodyJSON);

                var weaponBody = Functions.Deserialize<WeaponBody>(json);
                weaponBody.Name = weaponName;

                var index = weaponsBody.FindIndex(x => x.Name.ToLower() == weaponName.ToLower());
                if (index != -1)
                    weaponsBody[index] = weaponBody;
                else
                    weaponsBody.Add(weaponBody);

                player.Character.SetWeaponsBodyJSON(Functions.Serialize(weaponsBody));
            }

            player.Emit("WeaponBody:CloseServer");
            player.CheckAttachedWeapon((uint)player.CurrentWeapon);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["equipar"], "Facção", "(nome [lista])", "Pega equipamentos")]
    public async Task CMD_equipar(MyPlayer player, string name)
    {
        if (!(player.Faction?.Government ?? false) || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma governamental ou não está em serviço.");
            return;
        }

        if (name.ToLower() == "lista")
        {
            player.SendMessage(MessageType.Title, "Equipamentos");
            foreach (var equipment in Global.FactionsEquipments
                .Where(x => x.FactionId == player.Character.FactionId)
                .OrderBy(x => x.Name)
                .ToList())
            {
                player.SendMessage(MessageType.None, $"{equipment.Name} ({string.Join(", ", equipment.Items!.Select(x => x.Weapon))})");
            }
            return;
        }

        var factionEquipment = Global.FactionsEquipments.FirstOrDefault(x => x.FactionId == player.Character.FactionId
            && x.Name.ToLower() == name.ToLower());
        if (factionEquipment is null)
        {
            player.SendMessage(MessageType.Error, $"Equipamento {name} não encontrado.");
            return;
        }

        if (factionEquipment.Items!.Count == 0)
        {
            player.SendMessage(MessageType.Error, $"Equipamento {name} não possui itens. Por favor, reporte o bug.");
            return;
        }

        if (factionEquipment.SWAT && !player.FactionFlags.Contains(FactionFlag.SWAT))
        {
            player.SendMessage(MessageType.Error, $"Você não possui a flag {FactionFlag.SWAT.GetDescription()};");
            return;
        }

        if (factionEquipment.UPR && !player.FactionFlags.Contains(FactionFlag.UPR))
        {
            player.SendMessage(MessageType.Error, $"Você não possui a flag {FactionFlag.UPR.GetDescription()};");
            return;
        }

        MyVehicle? vehicle = null;
        if (factionEquipment.PropertyOrVehicle)
        {
            var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
            if (property is null)
            {
                vehicle = Global.Vehicles.Where(x => x.GetDimension() == player.GetDimension()
                    && x.VehicleDB.FactionId == player.Character.FactionId
                    && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
                if (vehicle is null)
                {
                    player.SendMessage(MessageType.Error, "Você não está perto de um veículo ou em um interior de sua facção.");
                    return;
                }
            }
        }

        foreach (var characterItem in player.Items.Where(x => x.InUse && x.GetCategory() == ItemCategory.Weapon).ToList())
        {
            await UseItem(player, characterItem.Id.ToString(), characterItem.Quantity);
        }

        Functions.RunOnMainThread(() =>
        {
            player.RemoveFactionEquippedWeapons();

            foreach (var item in factionEquipment.Items)
            {
                var weapon = GlobalFunctions.GetWeaponType(item.Weapon);

                player.GiveWeapon((WeaponHash)weapon, (int)item.Ammo);

                var components = Functions.Deserialize<List<uint>>(item.ComponentsJson);
                foreach (var component in components)
                    player.AddWeaponComponent(weapon, component);

                player.FactionEquippedWeapons.Add(weapon);
            }
        });

        await player.WriteLog(LogType.Faction, $"/equipar {name}", null);
        player.SendMessage(MessageType.Success, $"Você equipou {factionEquipment.Name}.");
    }

    [Command(["celulares"], "Geral", "Lista os seus números")]
    public void CMD_celulares(MyPlayer player)
    {
        var cellphones = player.Items.Where(x => x.GetCategory() == ItemCategory.Cellphone).ToList();
        if (cellphones.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Você não possui nenhum celular no inventário.");
            return;
        }

        player.SendMessage(MessageType.Title, "Celulares");
        foreach (var cellphone in cellphones)
            player.SendMessage(MessageType.None, $"{cellphone.Subtype}{(cellphone.InUse ? " (EM USO)" : string.Empty)}");
    }
}