using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class CrackDenScript : Script
{
    [RemoteEvent(nameof(CrackDenSellItem))]
    public async Task CrackDenSellItem(Player playerParam, string idString, int quantity)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (quantity <= 0)
            {
                player.SendNotification(NotificationType.Error, "Quantidade deve ser maior que 0.");
                return;
            }

            var id = idString.ToGuid();
            var item = Global.CrackDensItems.FirstOrDefault(x => x.Id == id);
            if (item is null)
                return;

            var name = Global.ItemsTemplates.FirstOrDefault(y => y.Id == item.ItemTemplateId)!.Name;

            var crackDen = Global.CrackDens.FirstOrDefault(x => x.Id == item.CrackDenId);
            if (crackDen is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (crackDen.CooldownDate > DateTime.Now)
            {
                player.SendNotification(NotificationType.Error, $"A boca de fumo está com o cooldown ativo. Será liberada novamente {crackDen.CooldownDate}.");
                return;
            }

            if (!player.Items.Any(x => x.ItemTemplateId == item.ItemTemplateId && x.Quantity >= quantity))
            {
                player.SendNotification(NotificationType.Error, $"Você não possui {quantity}x {name}.");
                return;
            }

            var allowedQuantity = crackDen.CooldownQuantityLimit - crackDen.Quantity;
            if (crackDen.Quantity + quantity > crackDen.CooldownQuantityLimit)
            {
                player.SendNotification(NotificationType.Error, $"A quantidade selecionada para venda ultrapassa o limite da boca de fumo. Quantidade restante: {allowedQuantity:N0}.");
                return;
            }

            var nowDate = DateTime.Now;

            var initialDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                Global.Parameter.InitialTimeCrackDen, 0, 0);

            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                Global.Parameter.EndTimeCrackDen, 0, 0);

            if (Global.Parameter.EndTimeCrackDen < Global.Parameter.InitialTimeCrackDen && nowDate.Hour >= Global.Parameter.InitialTimeCrackDen)
                endDate = endDate.AddDays(1);
            else
                initialDate = initialDate.AddDays(-1);

            if (nowDate < initialDate || nowDate > endDate)
            {
                player.SendNotification(NotificationType.Error, $"Você está fora do horário de funcionamento das bocas de fumo ({Global.Parameter.InitialTimeCrackDen} - {Global.Parameter.EndTimeCrackDen}).");
                return;
            }

            var value = item.Value * quantity;

            var res = await player.GiveMoney(value);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            await player.RemoveStackedItem(item.ItemTemplateId, quantity);

            crackDen.AddQuantity(quantity);

            var context = Functions.GetDatabaseContext();
            context.CrackDens.Update(crackDen);
            await context.SaveChangesAsync();

            var crackDenSell = new CrackDenSell();
            crackDenSell.Create(item.CrackDenId, player.Character.Id, item.ItemTemplateId, quantity, item.Value);
            await context.CrackDensSells.AddAsync(crackDenSell);
            await context.SaveChangesAsync();

            player.SendNotification(NotificationType.Success, $"Você vendeu {quantity}x {name} por ${value:N0}.");
            await player.WriteLog(LogType.Drug, $"Vender Boca de Fumo {item.CrackDenId} {name} {item.ItemTemplateId} {quantity}", null);

            ShowCrackDen(player, crackDen.Id);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CrackDenShowSales))]
    public async Task CrackDenShowSales(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            await player.WriteLog(LogType.ViewCrackDenSales, idString, null);

            var context = Functions.GetDatabaseContext();
            var sales = await context.CrackDensSells
                .Where(x => x.CrackDenId == id)
                .Include(x => x.ItemTemplate)
                .Include(x => x.Character)
                    .ThenInclude(x => x!.User)
                .OrderByDescending(x => x.RegisterDate)
                .Take(50)
                .Select(x => new
                {
                    Date = x.RegisterDate,
                    Character = $"{x.Character!.Name} ({x.Character.User!.Name})",
                    Item = x.ItemTemplate!.Name,
                    x.Quantity,
                    x.Value,
                    Total = x.Quantity * x.Value,
                })
                .ToListAsync();

            player.Emit("ViewCrackDenSales", $"50 últimas vendas da Boca de Fumo {id}", Functions.Serialize(sales));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["bocafumo"], "Geral", "Usa uma boca de fumo")]
    public static void CMD_bocafumo(MyPlayer player)
    {
        var crackDen = Global.CrackDens.FirstOrDefault(x =>
            player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE
            && x.Dimension == player.GetDimension());
        if (crackDen is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhuma boca de fumo.");
            return;
        }

        ShowCrackDen(player, crackDen.Id);
    }

    private static void ShowCrackDen(MyPlayer player, Guid crackDenId)
    {
        player.Emit("ShowCrackDen",
            Functions.Serialize(Global.CrackDensItems.Where(x => x.CrackDenId == crackDenId).Select(x => new
            {
                x.Id,
                Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)!.Name,
                Price = x.Value,
            })), crackDenId.ToString());
    }
}