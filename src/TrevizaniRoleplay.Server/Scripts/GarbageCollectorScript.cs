using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class GarbageCollectorScript : Script
{
    [Command("pegarlixo")]
    public static void CMD_pegarlixo(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.GarbageCollector || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não é um lixeiro ou não está em serviço.");
            return;
        }

        if (player.CollectingSpot is not null)
        {
            player.SendMessage(MessageType.Error, "Você está segurando um saco de lixo.");
            return;
        }

        var ponto = player.CollectSpots
           .Where(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE)
           .OrderBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)))
           .FirstOrDefault();
        if (ponto == null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum ponto de coleta.");
            return;
        }

        player.AttachObject(Constants.GARBAGE_OBJECT_MODEL, 57005, new(0.1f, -0.1f, -0.4f), new(0, 0, 0));
        player.CollectingSpot = ponto;
        player.SendMessageToNearbyPlayers($"pega um saco de lixo.", MessageCategory.Ame);
    }

    [Command("colocarlixo")]
    public static void CMD_colocarlixo(MyPlayer player)
    {
        if (player.Character.Job != CharacterJob.GarbageCollector || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não é um lixeiro ou não está em serviço.");
            return;
        }

        if (player.CollectingSpot is null)
        {
            player.SendMessage(MessageType.Error, "Você não está segurando um saco de lixo.");
            return;
        }

        var veh = Global.Vehicles
            .Where(x => (x.Model == (uint)VehicleModel.Trash || x.Model == (uint)VehicleModel.Trash2)
            && player.GetPosition().DistanceTo(x.GetPosition()) <= 15)
            .OrderBy(x => player.GetPosition().DistanceTo(x.GetPosition()))
            .FirstOrDefault();
        if (veh == null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum caminhão de lixo.");
            return;
        }

        player.Emit("Server:VerificarSoltarSacoLixo", veh);
    }

    [RemoteEvent(nameof(SoltarSacoLixo))]
    public void SoltarSacoLixo(Player playerParam, float x, float y, float z)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.CollectingSpot is null)
            {
                player.SendMessage(MessageType.Error, "Você não está segurando um saco de lixo");
                return;
            }

            if (player.GetPosition().DistanceTo(new(x, y, z)) > 2)
            {
                player.SendMessage(MessageType.Error, "Você não está na parte de trás de um caminhão de lixo.");
                return;
            }

            player.DetachObject(Constants.GARBAGE_OBJECT_MODEL);
            player.CollectingSpot.RemoveIdentifier();
            player.CollectSpots.Remove(player.CollectingSpot);
            player.CollectingSpot = null;
            player.SendMessageToNearbyPlayers($"coloca um saco de lixo no caminhão.", MessageCategory.Ame);

            var multiplier = Global.Drugs.FirstOrDefault(x => x.ItemTemplateId == player.Character.DrugItemTemplateId)?.TruckerMultiplier ?? 1;

            var extraPayment = Convert.ToInt32(Math.Abs(Global.Parameter.ExtraPaymentGarbagemanValue * multiplier));
            player.ExtraPayment += extraPayment;

            if (player.CollectSpots.Count == 0)
            {
                player.SendMessage(MessageType.Success, "Você realizou todas as coletas. Retorne ao centro de reciclagem e saia de serviço.");
                return;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}