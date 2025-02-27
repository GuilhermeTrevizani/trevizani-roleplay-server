using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffDealershipScript : Script
{
    [Command("concessionarias")]
    public static void CMD_concessionarias(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.ServerManager)
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        player.Emit("StaffDealership:Show", GetDealershipsJson());
    }

    [RemoteEvent(nameof(StaffDealershipGoto))]
    public static void StaffDealershipGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.ServerManager)
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            var dealership = Global.Dealerships.FirstOrDefault(x => x.Id == id);
            if (dealership is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.SetPosition(new(dealership.PosX, dealership.PosY, dealership.PosZ), 0, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffDealershipSave))]
    public async Task StaffDealershipSave(Player playerParam, string idString, string name, Vector3 pos, Vector3 vehiclePos, Vector3 vehicleRot)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.ServerManager)
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                player.SendNotification(NotificationType.Error, "Nome é obrigatório.");
                return;
            }

            if (name.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Nome deve ter até 50 caracteres.");
                return;
            }

            var dealership = new Dealership();
            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            if (isNew)
            {
                dealership.Create(name,
                    pos.X, pos.Y, pos.Z,
                    vehiclePos.X, vehiclePos.Y, vehiclePos.Z,
                    vehicleRot.X, vehicleRot.Y, vehicleRot.Z);
            }
            else
            {
                dealership = Global.Dealerships.FirstOrDefault(x => x.Id == id);
                if (dealership is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                dealership.Update(name,
                    pos.X, pos.Y, pos.Z,
                    vehiclePos.X, vehiclePos.Y, vehiclePos.Z,
                    vehicleRot.X, vehicleRot.Y, vehicleRot.Z);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Dealerships.AddAsync(dealership);
            else
                context.Dealerships.Update(dealership);

            await context.SaveChangesAsync();

            dealership.CreateIdentifier();

            if (isNew)
                Global.Dealerships.Add(dealership);

            await player.WriteLog(LogType.Staff, $"Gravar Concessionária | {Functions.Serialize(dealership)}", null);
            player.SendNotification(NotificationType.Success, $"Concessionária {(isNew ? "criada" : "editada")}.");
            UpdateDealerships();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffDealershipRemove))]
    public async Task StaffDealershipRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.ServerManager)
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            var dealership = Global.Dealerships.FirstOrDefault(x => x.Id == id);
            if (dealership is not null)
            {
                var context = Functions.GetDatabaseContext();
                context.Dealerships.Remove(dealership);
                var vehicles = Global.DealershipsVehicles.Where(x => x.DealershipId == id);
                if (vehicles.Any())
                    context.DealershipsVehicles.RemoveRange(vehicles);
                await context.SaveChangesAsync();
                Global.Dealerships.Remove(dealership);
                Global.DealershipsVehicles.RemoveAll(x => x.DealershipId == id);
                dealership.RemoveIdentifier();
                await player.WriteLog(LogType.Staff, $"Remover Concessionária | {Functions.Serialize(dealership)}", null);
            }

            player.SendNotification(NotificationType.Success, "Concessionária excluída.");
            UpdateDealerships();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateDealerships()
    {
        var json = GetDealershipsJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.ServerManager))
            target.Emit("StaffDealership:Update", json);
    }

    private static string GetDealershipsJson()
    {
        return Functions.Serialize(Global.Dealerships
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.PosX,
                x.PosY,
                x.PosZ,
                x.VehiclePosX,
                x.VehiclePosY,
                x.VehiclePosZ,
                x.VehicleRotR,
                x.VehicleRotP,
                x.VehicleRotY,
            }));
    }

    [RemoteEvent(nameof(StaffDealershipsVehiclesShow))]
    public static void StaffDealershipsVehiclesShow(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.ServerManager)
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            player.Emit("StaffDealershipVehicle:Show", idString, GetDealershipsVehiclesJson(id!.Value));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffDealershipVehicleSave))]
    public async Task StaffDealershipVehicleSave(Player playerParam, string dealershipIdString, string dealershipVehicleIdString, string model, int value)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.ServerManager)
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                player.SendNotification(NotificationType.Error, "Modelo é obrigatório.");
                return;
            }

            if (!Functions.CheckIfVehicleExists(model))
            {
                player.SendNotification(NotificationType.Error, "Modelo não existe.");
                return;
            }

            if (value <= 0)
            {
                player.SendNotification(NotificationType.Error, "Valor deve ser maior que 0.");
                return;
            }

            var dealershipVehicleId = dealershipVehicleIdString.ToGuid();
            if (Global.DealershipsVehicles.Any(x => x.Id != dealershipVehicleId && x.Model.ToLower() == model.ToLower()))
            {
                player.SendNotification(NotificationType.Error, "Modelo já existe.");
                return;
            }

            var dealershipId = dealershipIdString.ToGuid()!.Value;
            var dealershipVehicle = new DealershipVehicle();
            var isNew = string.IsNullOrWhiteSpace(dealershipVehicleIdString);
            if (isNew)
            {
                dealershipVehicle.Create(dealershipId, model, value);
            }
            else
            {
                dealershipVehicle = Global.DealershipsVehicles.FirstOrDefault(x => x.Id == dealershipVehicleId);
                if (dealershipVehicle is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                dealershipVehicle.Update(model, value);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.DealershipsVehicles.AddAsync(dealershipVehicle);
            else
                context.DealershipsVehicles.Update(dealershipVehicle);

            await context.SaveChangesAsync();

            if (isNew)
                Global.DealershipsVehicles.Add(dealershipVehicle);

            await player.WriteLog(LogType.Staff, $"Gravar Veículo Concessionária | {Functions.Serialize(dealershipVehicle)}", null);
            player.SendNotification(NotificationType.Success, $"Veículo {(isNew ? "criado" : "editado")}.");
            UpdateDealershipsVehicles(dealershipId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffDealershipVehicleRemove))]
    public async Task StaffDealershipVehicleRemove(Player playerParam, string dealershipIdString, string dealershipVehicleIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.ServerManager)
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var dealershipId = dealershipIdString.ToGuid()!.Value;
            var id = dealershipVehicleIdString.ToGuid();
            var dealershipVehicle = Global.DealershipsVehicles.FirstOrDefault(x => x.Id == id);
            if (dealershipVehicle is not null)
            {
                var context = Functions.GetDatabaseContext();
                context.DealershipsVehicles.Remove(dealershipVehicle);
                await context.SaveChangesAsync();
                Global.DealershipsVehicles.Remove(dealershipVehicle);
                await player.WriteLog(LogType.Staff, $"Remover Veículo Concessionária | {Functions.Serialize(dealershipVehicle)}", null);
            }

            player.SendNotification(NotificationType.Success, "Veículo excluído.");
            UpdateDealershipsVehicles(dealershipId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateDealershipsVehicles(Guid dealershipId)
    {
        var json = GetDealershipsVehiclesJson(dealershipId);
        foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.ServerManager))
            target.Emit("StaffDealershipVehicle:Update", json);
    }

    private static string GetDealershipsVehiclesJson(Guid dealershipId)
    {
        return Functions.Serialize(Global.DealershipsVehicles.Where(x => x.DealershipId == dealershipId)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Model,
                x.Value,
            }));
    }
}