using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class FactionUniformScript : Script
{
    [Command(["adduniforme"], "Facção", "Cria um uniforme com as roupas que está vestindo", "(nome)", GreedyArg = true)]
    public async Task CMD_adduniforme(MyPlayer player, string name)
    {
        if (name.Length > 50)
        {
            player.SendMessage(MessageType.Error, "Nome deve ter no máximo 50 caracteres.");
            return;
        }

        if (!(player.Faction?.HasDuty ?? false) || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.Uniform))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (Global.FactionsUniforms.Any(x => x.FactionId == player.Character.FactionId && x.Name.ToLower() == name.ToLower()))
        {
            player.SendMessage(MessageType.Error, $"Já existe um uniforme com o nome {name}.");
            return;
        }

        var outfit = Functions.Deserialize<IEnumerable<Outfit>>(player.Character.OutfitsOnDutyJSON)
            .FirstOrDefault(x => x.Slot == player.Character.OutfitOnDuty)!;

        var factionUniform = new FactionUniform();
        factionUniform.Create(player.Faction.Id, name, Functions.Serialize(outfit), player.Character.Sex);

        var context = Functions.GetDatabaseContext();
        await context.FactionsUniforms.AddAsync(factionUniform);
        await context.SaveChangesAsync();

        Global.FactionsUniforms.Add(factionUniform);

        await player.WriteLog(LogType.Faction, $"/adduniforme {Functions.Serialize(factionUniform)}", null);
        player.SendMessage(MessageType.Success, $"Você criou o uniforme {name}.");
    }

    [Command(["deluniforme"], "Facção", "Remove um uniforme", "(nome)", GreedyArg = true)]
    public async Task CMD_deluniforme(MyPlayer player, string name)
    {
        if (!(player.Faction?.HasDuty ?? false) || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.Uniform))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var factionUniform = Global.FactionsUniforms.FirstOrDefault(x => x.FactionId == player.Character.FactionId && x.Name.ToLower() == name.ToLower());
        if (factionUniform is null)
        {
            player.SendMessage(MessageType.Error, $"Uniforme {name} não existe.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        context.FactionsUniforms.Remove(factionUniform);
        await context.SaveChangesAsync();

        Global.FactionsUniforms.Remove(factionUniform);

        await player.WriteLog(LogType.Faction, $"/deluniforme {Functions.Serialize(factionUniform)}", null);
        player.SendMessage(MessageType.Success, $"Você deletou o uniforme {name}.");
    }

    [Command(["criaruniforme"], "Facção", "Cria um uniforme através do menu de seleção", "(nome)", GreedyArg = true)]
    public static void CMD_criaruniforme(MyPlayer player, string name)
    {
        if (name.Length > 50)
        {
            player.SendMessage(MessageType.Error, "Nome deve ter no máximo 50 caracteres.");
            return;
        }

        if (!(player.Faction?.HasDuty ?? false) || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção governamental ou não está em serviço.");
            return;
        }

        if (!player.FactionFlags.Contains(FactionFlag.Uniform))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (Global.FactionsUniforms.Any(x => x.FactionId == player.Character.FactionId && x.Name.ToLower() == name.ToLower()))
        {
            player.SendMessage(MessageType.Error, $"Já existe um uniforme com o nome {name}.");
            return;
        }

        player.CreatingOutfitName = name;
        player.EditOutfits(3);
    }
}