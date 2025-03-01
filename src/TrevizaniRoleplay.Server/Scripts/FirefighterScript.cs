using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class FirefighterScript : Script
{
    [Command("curar", "/curar (ID ou nome)")]
    public static void CMD_curar(MyPlayer player, string idOrName)
    {
        if (player.Faction?.Type != FactionType.Firefighter || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção médica ou não está em serviço.");
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

        if (target.Character.Wound >= CharacterWound.PK)
        {
            player.SendMessage(MessageType.Error, "Jogador morreu e perdeu a memória.");
            return;
        }

        if (!target.Wounded)
        {
            player.SendMessage(MessageType.Error, "Jogador não está ferido.");
            return;
        }

        target.Heal();
        player.SendMessage(MessageType.Success, $"Você curou {target.ICName}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} curou você.");
    }
}