using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffGiveItemScript : Script
{
    [Command("daritem")]
    public static void CMD_daritem(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.GiveItem))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Globalization.NEED_ADMIN_DUTY);
            return;
        }

        player.Emit("Staff:GiveItem", Functions.GetItemsTemplatesResponse());
    }

    [RemoteEvent(nameof(StaffGiveItem))]
    public async Task StaffGiveItem(Player playerParam, string itemTemplateIdString, int quantity, int targetId, string reason)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.OnAdminDuty)
            {
                player.SendNotification(NotificationType.Error, Globalization.NEED_ADMIN_DUTY);
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == targetId);
            if (target is null)
            {
                player.SendNotification(NotificationType.Error, "Jogador inválido.");
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

            if (string.IsNullOrWhiteSpace(reason))
            {
                player.SendNotification(NotificationType.Error, "Motivo não foi informado.");
                return;
            }

            var characterItem = new CharacterItem();
            characterItem.Create(itemTemplate.Id, 0, quantity, null);
            if (!characterItem.GetIsStack())
                characterItem.SetQuantity(1);
            var res = await target.GiveItem(characterItem);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            await player.WriteLog(LogType.GiveItem, $"{characterItem.Quantity}x {characterItem.GetName()} | Motivo: {reason} | {Functions.Serialize(characterItem)}", target);
            await Functions.SendServerMessage($"{player.User.Name} deu {characterItem.Quantity}x {characterItem.GetName()} para {target.Character.Name}. Motivo: {reason}", UserStaff.JuniorServerAdmin, false);
            player.SendNotification(NotificationType.Success, $"Você deu {characterItem.Quantity}x {characterItem.GetName()} para {target.Character.Name}.");
            target.SendMessage(MessageType.Success, $"{player.User.Name} deu {characterItem.Quantity}x {characterItem.GetName()} para você.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}