using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class SmugglerScript : Script
{
    [Command("contrabando", "/contrabando (quantidade)")]
    public async Task CMD_contrabando(MyPlayer player, int quantity)
    {
        if (quantity <= 0)
        {
            player.SendMessage(MessageType.Error, "Quantidade deve ser maior que 0.");
            return;
        }

        var item = player.Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.VehiclePart);
        if (item is null || item.Quantity < quantity)
        {
            player.SendMessage(MessageType.Error, "Você não possui essa quantidade de peças de veículo.");
            return;
        }

        var smuggler = Global.Smugglers
            .FirstOrDefault(x => x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE
                && x.IsSpawned());
        if (smuggler is null)
        {
            player.SendMessage(MessageType.Error, "Você não está perto de nenhum contrabandista.");
            return;
        }

        if (smuggler.GetAllowedCharacters().Any() && !smuggler.GetAllowedCharacters().Contains(player.Character.Name))
        {
            player.SendMessage(MessageType.None, "Contrabandista diz: Nem te conheço... Tô caindo fora.");
            smuggler.RemoveIdentifier();
            return;
        }

        var value = quantity * smuggler.Value;
        var res = await player.GiveMoney(value);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendMessage(MessageType.Error, res);
            return;
        }

        await player.RemoveStackedItem(item.ItemTemplateId, quantity);

        smuggler.AddQuantity(quantity);

        player.SendMessage(MessageType.None, "Contrabandista diz: Sempre bom fazer negócio contigo meu camarada. Tô indo nessa.");

        if (smuggler.Quantity >= smuggler.CooldownQuantityLimit)
        {
            smuggler.SetCooldown();
            player.SendMessage(MessageType.None, "Contrabandista diz: Agora vou dar um tempo e deixar as coisas esfriarem.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        context.Smugglers.Update(smuggler);
        await context.SaveChangesAsync();
        await player.WriteLog(LogType.Smuggler, $"{item.GetName()} {quantity}x ${smuggler.Value:N0}", null);
        smuggler.RemoveIdentifier();
        player.SendMessage(MessageType.Success, $"Você vendeu {quantity}x {item.GetName()} para um contrabandista por ${smuggler.Value:N0} a unidade totalizando ${value:N0}.");
    }
}