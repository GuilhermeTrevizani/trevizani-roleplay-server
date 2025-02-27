using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffVehicleScript : Script
{
    [Command("veiculos")]
    public async Task CMD_veiculos(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Vehicles))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        var modelsJson = Functions.Serialize(Enum.GetValues<VehicleModelMods>()
            .Select(x => x.ToString())
            .Order());

        player.Emit("StaffVehicle:Show", await GetVehiclesJson(), modelsJson);
    }

    [Command("atunar")]
    public static void CMD_atunar(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Vehicles))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Globalization.NEED_ADMIN_DUTY);
            return;
        }

        Functions.CMDTuning(player, null, null, true);
    }

    private async Task UpdateVehicles()
    {
        var modelsJson = Functions.Serialize(Enum.GetValues<VehicleModelMods>()
            .Select(x => x.ToString())
            .Order());
        var vehiclesJson = await GetVehiclesJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Vehicles)))
            target.Emit("StaffVehicle:Update", vehiclesJson, modelsJson);
    }

    [RemoteEvent(nameof(StaffVehicleSave))]
    public async Task StaffVehicleSave(Player playerParam, string model, string factionName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Vehicles))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (!Functions.CheckIfVehicleExists(model))
            {
                player.SendNotification(NotificationType.Error, $"Modelo {model} não existe.");
                return;
            }

            var faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == factionName?.ToLower());
            if (faction is null)
            {
                player.SendNotification(NotificationType.Error, $"Facção {factionName} não encontrada.");
                return;
            }

            if (!faction.HasVehicles)
            {
                player.SendNotification(NotificationType.Error, $"Facção {factionName} não possui flag de veículos.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var vehicle = new Domain.Entities.Vehicle();
            vehicle.Create(model, await Functions.GenerateVehiclePlate(faction.HasDuty), 0, 0, 0, 0, 0, 0);
            vehicle.ChangePosition(player.GetPosition().X, player.GetPosition().Y, player.GetPosition().Z,
                player.GetRotation().X, player.GetRotation().Y, player.GetRotation().Z, 0);
            vehicle.SetFuel(vehicle.GetMaxFuel());
            vehicle.SetFaction(faction!.Id);
            await context.Vehicles.AddAsync(vehicle);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Criar Veículo | {Functions.Serialize(vehicle)}", null);
            player.SendNotification(NotificationType.Success, "Veículo criado.");
            await UpdateVehicles();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffVehicleRemove))]
    public async Task StaffVehicleRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Vehicles))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            if (Global.Vehicles.Any(x => x.VehicleDB.Id == id))
            {
                player.SendNotification(NotificationType.Error, "Veículo está spawnado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == id);
            if (vehicle is not null)
            {
                vehicle.SetSold();
                context.Vehicles.Update(vehicle);
                await context.SaveChangesAsync();
                await player.WriteLog(LogType.Staff, $"Remover Veículo | {Functions.Serialize(vehicle)}", null);
            }

            player.SendNotification(NotificationType.Success, "Veículo excluído.");
            await UpdateVehicles();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task<string> GetVehiclesJson()
    {
        var context = Functions.GetDatabaseContext();
        var vehicles = await context.Vehicles
            .Include(x => x.Faction)
            .Where(x => !x.Sold && x.FactionId.HasValue)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Model,
                x.Plate,
                x.Description,
                Faction = x.Faction!.Name,
            })
            .ToListAsync();
        return Functions.Serialize(vehicles);
    }
}