using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PoliceSpikeStripScript : Script
{
    [Command("vpegarpregos")]
    public static void CMD_vpegarpregos(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        if (player.HasSpikeStrip)
        {
            player.SendMessage(MessageType.Error, "Você já possui um tapete de pregos.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x => x.GetDimension() == player.GetDimension()
            && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null || vehicle.DoorsStates[5])
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um veículo com o porta-malas aberto.");
            return;
        }

        if (!vehicle.HasSpikeStrip)
        {
            player.SendMessage(MessageType.Error, "O veículo não possui um tapete de pregos.");
            return;
        }

        vehicle.HasSpikeStrip = false;
        player.HasSpikeStrip = true;
        player.SendMessageToNearbyPlayers($"pega um tapete de pregos do porta-malas.", MessageCategory.Ame);
    }

    [Command("colocarpregos", "/colocarpregos (tamanho (1-3))")]
    public async Task CMD_colocarpregos(MyPlayer player, byte size)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        if (!player.HasSpikeStrip)
        {
            player.SendMessage(MessageType.Error, "Você não tem um tapete de pregos.");
            return;
        }

        if (size < 1 || size > 3)
        {
            player.SendMessage(MessageType.Error, $"Tamanho deve ser entre 1 e 3.");
            return;
        }

        var distance = 2.5f;
        var position = player.GetPosition();
        var rot = player.GetRotation();

        var newPos = new Vector3(position.X + Math.Sin(-rot.Z * Math.PI / 180) * distance, position.Y + Math.Cos(-rot.Z * Math.PI / 180) * distance, position.Z - 0.90);
        Functions.CreateObject(Constants.SPIKE_STRIP_OBJECT_MODEL, newPos, rot, player.GetDimension(), true, false);

        if (size >= 2)
        {
            var newPos2 = new Vector3(position.X + Math.Sin(-rot.Z * Math.PI / 180) * (distance * 2.45), position.Y + Math.Cos(-rot.Z * Math.PI / 180) * (distance * 2.45), position.Z - 0.90);
            Functions.CreateObject(Constants.SPIKE_STRIP_OBJECT_MODEL, newPos2, rot, player.GetDimension(), true, false);
        }

        if (size >= 3)
        {
            var newPos3 = new Vector3(position.X + Math.Sin(-rot.Z * Math.PI / 180) * (distance * 3.9), position.Y + Math.Cos(-rot.Z * Math.PI / 180) * (distance * 3.9), position.Z - 0.90);
            Functions.CreateObject(Constants.SPIKE_STRIP_OBJECT_MODEL, newPos3, rot, player.GetDimension(), true, false);
        }

        player.HasSpikeStrip = false;
        player.SendMessageToNearbyPlayers($"coloca um tapete de pregos no chão.", MessageCategory.Ame);
        await player.WriteLog(LogType.Faction, $"/colocarpregos {size} {position}", null);
    }

    [Command("pegarpregos")]
    public static void CMD_pegarpregos(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        var spikesStrip = Global.Objects.Where(x => x.GetDimension() == player.GetDimension()
            && x.GetPosition().DistanceTo(player.GetPosition()) <= 15
            && x.GetModel() == Functions.Hash(Constants.SPIKE_STRIP_OBJECT_MODEL))
            .ToList();
        if (spikesStrip.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um tapete de pregos.");
            return;
        }

        foreach (var spikeStrip in spikesStrip)
            spikeStrip.DestroyObject();
        player.HasSpikeStrip = true;
        player.SendMessageToNearbyPlayers($"pega um tapete de pregos do chão.", MessageCategory.Ame);
    }

    [Command("vcolocarpregos")]
    public static void CMD_vcolocarpregos(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        if (!player.HasSpikeStrip)
        {
            player.SendMessage(MessageType.Error, "Você não tem um tapete de pregos.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x => x.GetDimension() == player.GetDimension()
            && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null || vehicle.DoorsStates[5])
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um veículo com o porta-malas aberto.");
            return;
        }

        if (vehicle.HasSpikeStrip)
        {
            player.SendMessage(MessageType.Error, "O veículo já possui um tapete de pregos.");
            return;
        }

        vehicle.HasSpikeStrip = true;
        player.HasSpikeStrip = false;
        player.SendMessageToNearbyPlayers($"coloca um tapete de pregos no porta-malas.", MessageCategory.Ame);
    }
}