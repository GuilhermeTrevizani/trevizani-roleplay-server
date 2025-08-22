using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class JobScript : Script
{
    [Command("sairemprego")]
    public async Task CMD_sairemprego(MyPlayer player)
    {
        if (player.Character.Job == CharacterJob.Unemployed || player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não tem um emprego ou está em serviço.");
            return;
        }

        var job = Global.Jobs.FirstOrDefault(x => x.CharacterJob == player.Character.Job)!;
        if (player.GetPosition().DistanceTo(new(job.PosX, job.PosY, job.PosZ)) > Constants.RP_DISTANCE)
        {
            player.SendMessage(MessageType.Error, "Você não está onde você pegou esse emprego.");
            return;
        }

        player.Character.QuitJob();
        player.ExtraPayment = 0;
        foreach (var collectSpot in player.CollectSpots)
            collectSpot.RemoveIdentifier();
        player.CollectSpots = [];
        await player.WriteLog(LogType.Job, "/sairemprego", null);
        player.SendMessage(MessageType.Success, "Você saiu do seu emprego.");
    }

    [Command("emprego")]
    public async Task CMD_emprego(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.Unemployed)
        {
            player.SendMessage(MessageType.Error, "Você já tem um emprego.");
            return;
        }

        if (player.Faction?.HasDuty ?? false)
        {
            player.SendMessage(MessageType.Error, "Você não pode pegar um emprego pois está em uma facção governamental.");
            return;
        }

        var job = Global.Jobs.FirstOrDefault(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (job is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum local de emprego.");
            return;
        }

        player.Character.SetJob(job.CharacterJob);
        await player.WriteLog(LogType.Job, $"/emprego {player.Character.Job}", null);
        player.SendMessage(MessageType.Success, $"Você pegou o emprego {player.Character.Job.GetDescription()}.");
    }

    [Command("chamadas")]
    public static void CMD_chamadas(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.TaxiDriver && player.Character.Job != CharacterJob.Mechanic || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em serviço como taxista ou mecânico.");
            return;
        }

        if (player.Character.Job == CharacterJob.TaxiDriver)
        {
            if (player.Vehicle?.Model != (uint)VehicleModel.Taxi)
            {
                player.SendMessage(MessageType.Error, "Você não está em um taxi.");
                return;
            }
        }

        var chamadas = Global.SpawnedPlayers.Where(x => x.WaitingServiceType == player.Character.Job).OrderBy(x => x.Character.Id).ToList();
        if (chamadas.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Não há nenhuma chamada.");
            return;
        }

        player.SendMessage(MessageType.Title, "Chamadas Aguardando");
        foreach (var c in chamadas)
            player.SendMessage(MessageType.None, $"Chamada #{c.SessionId}");
    }

    [Command("atcha", "/atcha (chamada)")]
    public static void CMD_atcha(MyPlayer player, int chamada)
    {
        if (player.Character.Job != CharacterJob.TaxiDriver && player.Character.Job != CharacterJob.Mechanic || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em serviço como taxista ou mecânico.");
            return;
        }

        if (player.Character.Job == CharacterJob.TaxiDriver)
        {
            if (player.Vehicle?.Model != (uint)VehicleModel.Taxi)
            {
                player.SendMessage(MessageType.Error, "Você não está em um taxi.");
                return;
            }
        }

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == chamada && x.WaitingServiceType == player.Character.Job);
        if (target == null)
        {
            player.SendMessage(MessageType.Error, "Não há nenhuma chamada com esse código.");
            return;
        }

        target.WaitingServiceType = CharacterJob.Unemployed;
        player.SetWaypoint(target.GetPosition().X, target.GetPosition().Y);
        player.SendMessage(MessageType.Success, $"Você está atendendo a chamada {chamada} e a localização do solicitante foi marcada em seu GPS.");
        if (player.Character.Job == CharacterJob.TaxiDriver)
            target.SendMessage(MessageType.None, $"[CELULAR] SMS de {target.GetCellphoneContactName(Constants.TAXI_NUMBER)}: Nosso taxista {player.Character.Name} está atendendo sua chamada. Placa: {player.Vehicle.NumberPlate}. Celular: {player.Character.Cellphone}.", Constants.CELLPHONE_MAIN_COLOR);
        else if (player.Character.Job == CharacterJob.Mechanic)
            target.SendMessage(MessageType.None, $"[CELULAR] SMS de {target.GetCellphoneContactName(Constants.MECHANIC_NUMBER)}: Nosso mecânico {player.Character.Name} está atendendo sua chamada. Celular: {player.Character.Cellphone}.", Constants.CELLPHONE_MAIN_COLOR);
    }

    [Command("duty", Aliases = ["trabalho"])]
    public async Task CMD_duty(MyPlayer player)
    {
        if (player.Faction?.HasDuty ?? false)
        {
            if (!player.ValidPed)
            {
                player.SendMessage(MessageType.Error, Resources.YouDontHaveAValidSkin);
                return;
            }

            var context = Functions.GetDatabaseContext();
            if (player.OnDuty)
            {
                if (player.FactionDutySession is not null)
                {
                    player.FactionDutySession.End();
                    context.Sessions.Update(player.FactionDutySession);
                    player.FactionDutySession = null;
                }
            }
            else
            {
                player.FactionDutySession = new();
                player.FactionDutySession.Create(player.Character.Id, SessionType.FactionDuty, player.RealIp, player.RealSocialClubName);
                await context.Sessions.AddAsync(player.FactionDutySession);
            }
            await context.SaveChangesAsync();

            player.OnDuty = !player.OnDuty;

            if (player.OnDuty)
            {
                if (player.Faction.Type == FactionType.Police)
                    player.SetArmor(100);
            }
            else
            {
                player.LeaveDuty();
            }

            player.SetOutfit();
            player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} {(player.OnDuty ? "entrou em" : "saiu de")} serviço.");
            return;
        }

        if (player.Character.Job != CharacterJob.Unemployed)
        {
            if (player.Character.Job == CharacterJob.GarbageCollector)
            {
                var job = Global.Jobs.FirstOrDefault(x => x.CharacterJob == CharacterJob.GarbageCollector)!;
                if (player.GetPosition().DistanceTo(new(job.PosX, job.PosY, job.PosZ)) > Constants.RP_DISTANCE)
                {
                    player.SendMessage(MessageType.Error, "Você não está próximo do local de emprego de lixeiro.");
                    return;
                }

                player.OnDuty = !player.OnDuty;
                if (player.OnDuty)
                {
                    foreach (var spot in Global.Spots
                        .Where(x => x.Type == SpotType.GarbageCollector)
                        .OrderBy(x => Guid.NewGuid())
                        .Take(20))
                    {
                        var newSpot = new Spot();
                        newSpot.Create(spot.Type, spot.PosX, spot.PosY, spot.PosZ, 0);
                        newSpot.CreateBlipAndMarkerForClient(player, new(newSpot.PosX, newSpot.PosY, newSpot.PosZ - 0.95f),
                            1, 2, 0.5f, "Ponto de Coleta",
                            Constants.MARKER_TYPE_HALO, 1.5f, Global.MainRgba);
                        player.CollectSpots.Add(newSpot);
                    }

                    player.SendMessage(MessageType.Success, $"Você entrou em serviço.");
                    player.SendMessage(MessageType.None, $"Use {{{Constants.MAIN_COLOR}}}/pegarlixo {{#FFFFFF}}para pegar um saco de lixo em uma lixeira e {{{Constants.MAIN_COLOR}}}/colocarlixo {{#FFFFFF}}para colocá-lo no caminhão.");
                    player.SendMessage(MessageType.None, $"No seu GPS foram marcados {{{Constants.MAIN_COLOR}}}{player.CollectSpots.Count} {{#FFFFFF}}pontos de coleta. Você receberá {{{Constants.MAIN_COLOR}}}${Global.Parameter.ExtraPaymentGarbagemanValue:N0} {{#FFFFFF}}por cada ponto completado.");
                    player.SendMessage(MessageType.None, $"Após concluir quantos pontos desejar, retorne e saia de serviço para concluir.");
                }
                else
                {
                    var collectedSpots = 20 - player.CollectSpots.Count;
                    foreach (var collectSpot in player.CollectSpots)
                        collectSpot.RemoveIdentifier();
                    player.CollectSpots = [];

                    player.Character.AddExtraPayment(player.ExtraPayment);
                    if (player.ExtraPayment > 0)
                        player.SendMessage(MessageType.None, $"Você realizou {{{Constants.MAIN_COLOR}}}{collectedSpots} {{#FFFFFF}}coleta{(collectedSpots > 1 ? "s" : string.Empty)} e {{{Constants.MAIN_COLOR}}}${player.ExtraPayment:N0} {{#FFFFFF}}foram adicionados no seu próximo pagamento.");

                    player.ExtraPayment = 0;
                    player.SendMessage(MessageType.Success, $"Você saiu de serviço.");
                }

                return;
            }
            player.OnDuty = !player.OnDuty;
            player.SendMessage(MessageType.Success, $"Você {(player.OnDuty ? "entrou em" : "saiu de")} serviço.");
            return;
        }

        var companySpot = Global.Spots
            .Where(x => x.Dimension == player.GetDimension()
                && x.Type == SpotType.Company
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
        if (companySpot is not null)
        {
            var company = Global.Companies
                .Where(x => x.Type == CompanyType.ConvenienceStore
                    && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= 50)
                .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
            if (company is null && player.GetDimension() != 0)
            {
                var companyProperty = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
                company = Global.Companies.FirstOrDefault(x => x.Id == companyProperty?.CompanyId && x.Type == CompanyType.ConvenienceStore);
            }

            if (company is null)
            {
                player.SendMessage(MessageType.Error, "Nenhuma loja de conveniência encontrada em 50 metros. Por favor, reporte o bug.");
                return;
            }

            if (Global.Companies.Any(x => x.Id != company.Id && x.EmployeeOnDuty == player.Character.Id))
            {
                player.SendMessage(MessageType.Error, "Você já está em serviço em outra loja de conveniência.");
                return;
            }

            if (!player.OnDuty && company.EmployeeOnDuty.HasValue)
            {
                player.SendMessage(MessageType.Error, $"{company.Name} já possui um funcionário em serviço.");
                return;
            }

            player.OnDuty = !player.OnDuty;
            company.SetEmployeeOnDuty(player.OnDuty ? player.Character.Id : null);
            company.ToggleOpen();
            player.SendMessage(MessageType.Success, $"Você {(player.OnDuty ? "entrou em" : "saiu de")} serviço.");
            return;
        }

        player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
    }
}