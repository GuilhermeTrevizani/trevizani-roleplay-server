using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class TruckerScript : Script
{
    [Command("tpda")]
    public static void CMD_tpda(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.Trucker || !player.OnDuty)
        {
            player.SendNotification(NotificationType.Error, "Você não é um caminhoneiro ou não está em serviço.");
            return;
        }

        if (Global.TruckerLocations.Count == 0)
        {
            player.SendNotification(NotificationType.Error, "Não há rotas criadas.");
            return;
        }

        player.Emit("TruckerLocations", Functions.Serialize(Global.TruckerLocations.OrderByDescending(x => x.RegisterDate).Select(x => new
        {
            x.Name,
            x.DeliveryValue,
            AllowedVehicles = string.Join(", ", x.GetAllowedVehicles()),
            x.PosX,
            x.PosY,
        })));
    }

    [Command("carregarcaixas")]
    public static async Task CMD_carregarcaixas(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.Trucker || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não é um caminhoneiro ou não está em serviço.");
            return;
        }

        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotTheDriverOfTheVehicle);
            return;
        }

        if (!veh.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, Resources.YouDoNotHaveAccessToTheVehicle);
            return;
        }

        if (veh.CollectSpots.Count > 0)
        {
            player.SendMessage(MessageType.Error, "O veículo já está carregado.");
            return;
        }

        var truckerLocation = Global.TruckerLocations.FirstOrDefault(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (truckerLocation is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhuma rota.");
            return;
        }

        if (!truckerLocation.GetAllowedVehicles().Contains(veh.VehicleDB.Model.ToUpper()))
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo permitido para esta rota.");
            return;
        }

        player.ToggleGameControls(false);
        player.SendMessage(MessageType.Success, $"Aguarde {truckerLocation.LoadWaitTime} segundo{(truckerLocation.LoadWaitTime != 1 ? "s" : string.Empty)}. Pressione DELETE para cancelar a ação.");
        player.CancellationTokenSourceAcao?.Cancel();
        player.CancellationTokenSourceAcao = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(truckerLocation.LoadWaitTime), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            foreach (var delivery in Global.TruckerLocationsDeliveries.Where(x => x.TruckerLocationId == truckerLocation.Id))
            {
                var newSpot = new Spot();
                newSpot.Create(SpotType.GarbageCollector, delivery.PosX, delivery.PosY, delivery.PosZ, 0);
                newSpot.CreateBlipAndMarkerForClient(player, new(newSpot.PosX, newSpot.PosY, newSpot.PosZ - 0.95f),
                    1, 2, 0.5f, "Ponto de Coleta",
                    Constants.MARKER_TYPE_HALO, 5, Global.MainRgba);
                veh.CollectSpots.Add(newSpot);
            }

            veh.TruckerLocation = truckerLocation;

            player.SendMessage(MessageType.Success, "Você carregou seu veículo. Ao chegar em um ponto de entrega use /entregarcaixas. Você somente receberá o seu pagamento se concluir todas as entregas.");
            player.ToggleGameControls(true);
            player.CancellationTokenSourceAcao = null;
        });
    }

    [Command("cancelarcaixas")]
    public static void CMD_cancelarcaixas(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.Trucker || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não é um caminhoneiro ou não está em serviço.");
            return;
        }

        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotTheDriverOfTheVehicle);
            return;
        }

        if (!veh.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, Resources.YouDoNotHaveAccessToTheVehicle);
            return;
        }

        if (veh.CollectSpots.Count == 0 || veh.TruckerLocation == null)
        {
            player.SendMessage(MessageType.Error, "O veículo não está carregado.");
            return;
        }

        if (player.GetPosition().DistanceTo(new(veh.TruckerLocation.PosX, veh.TruckerLocation.PosY, veh.TruckerLocation.PosZ)) > Constants.RP_DISTANCE)
        {
            player.SendMessage(MessageType.Error, "Você não está no local que carregou o veículo.");
            return;
        }

        foreach (var collectSpot in veh.CollectSpots)
            collectSpot.RemoveIdentifier();
        veh.CollectSpots = [];
        veh.TruckerLocation = null;
        player.ExtraPayment = 0;

        player.SendMessage(MessageType.Success, "Você descarregou seu veículo.");
    }

    [Command("entregarcaixas")]
    public static async Task CMD_entregarcaixas(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.Trucker || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não é um caminhoneiro ou não está em serviço.");
            return;
        }

        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotTheDriverOfTheVehicle);
            return;
        }

        if (!veh.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, Resources.YouDoNotHaveAccessToTheVehicle);
            return;
        }

        if (veh.CollectSpots.Count == 0 || veh.TruckerLocation == null)
        {
            player.SendMessage(MessageType.Error, "O veículo não está carregado.");
            return;
        }

        var spot = veh.CollectSpots.FirstOrDefault(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (spot == null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum ponto de entrega.");
            return;
        }

        player.ToggleGameControls(false);
        player.SendMessage(MessageType.Success, $"Aguarde {veh.TruckerLocation.UnloadWaitTime} segundo{(veh.TruckerLocation.UnloadWaitTime != 1 ? "s" : string.Empty)}. Pressione DELETE para cancelar a ação.");
        player.CancellationTokenSourceAcao?.Cancel();
        player.CancellationTokenSourceAcao = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(veh.TruckerLocation.UnloadWaitTime), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            spot.RemoveIdentifier();
            veh.CollectSpots.Remove(spot);

            var multiplier = Global.Drugs.FirstOrDefault(x => x.ItemTemplateId == player.Character.DrugItemTemplateId)?.TruckerMultiplier ?? 1;

            player.ExtraPayment += Convert.ToInt32(Math.Abs(veh.TruckerLocation.DeliveryValue * multiplier));

            player.SendMessage(MessageType.Success, $"Você realizou uma entrega.");

            if (veh.CollectSpots.Count == 0)
            {
                player.Character.AddExtraPayment(player.ExtraPayment);
                veh.TruckerLocation = null;
                player.SendMessage(MessageType.Success, $"Você realizou todas as entregas e a rota foi concluída. ${player.ExtraPayment:N0} foram adicionados no seu próximo pagamento.");
                player.ExtraPayment = 0;
            }

            player.ToggleGameControls(true);
            player.CancellationTokenSourceAcao = null;
        });
    }
}