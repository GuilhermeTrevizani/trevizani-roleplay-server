using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffTruckerLocationScript : Script
{
    [Command("acaminhoneiro")]
    public static void CMD_acaminhoneiro(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffTruckerLocation:Show", GetTruckerLocationsJson());
    }

    [RemoteEvent(nameof(StaffTruckerLocationGoto))]
    public static void StaffTruckerLocationGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var truckerLocation = Global.TruckerLocations.FirstOrDefault(x => x.Id == id);
            if (truckerLocation is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(truckerLocation.PosX, truckerLocation.PosY, truckerLocation.PosZ), 0, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateTruckerLocations()
    {
        var json = GetTruckerLocationsJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.TruckerLocations)))
            target.Emit("StaffTruckerLocation:Update", json);
    }

    [RemoteEvent(nameof(StaffTruckerLocationRemove))]
    public async Task StaffTruckerLocationRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var truckerLocation = Global.TruckerLocations.FirstOrDefault(x => x.Id == id);
            if (truckerLocation == null)
                return;

            var context = Functions.GetDatabaseContext();
            context.TruckerLocations.Remove(truckerLocation);
            context.TruckerLocationsDeliveries.RemoveRange(Global.TruckerLocationsDeliveries.Where(x => x.TruckerLocationId == id));
            await context.SaveChangesAsync();
            Global.TruckerLocations.Remove(truckerLocation);
            Global.TruckerLocationsDeliveries.RemoveAll(x => x.TruckerLocationId == truckerLocation.Id);
            truckerLocation.RemoveIdentifier();

            await player.WriteLog(LogType.Staff, $"Remover Localização de Caminhoneiro | {Functions.Serialize(truckerLocation)}", null);
            player.SendNotification(NotificationType.Success, "Localização de caminhoneiro excluída.");
            UpdateTruckerLocations();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffTruckerLocationSave))]
    public async Task StaffTruckerLocationSave(Player playerParam, string idString, string name, Vector3 pos,
        int deliveryValue, int loadWaitTime, int unloadWaitTime, string allowedVehiclesJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (deliveryValue <= 0)
            {
                player.SendNotification(NotificationType.Error, $"Valor por Entrega deve ser maior que 0.");
                return;
            }

            if (loadWaitTime <= 0)
            {
                player.SendNotification(NotificationType.Error, $"Valor por Entrega deve ser maior que 0.");
                return;
            }

            if (unloadWaitTime <= 0)
            {
                player.SendNotification(NotificationType.Error, $"Tempo de Espera por Entrega deve ser maior que 0.");
                return;
            }

            var allowedVehicles = Functions.Deserialize<List<string>>(allowedVehiclesJSON.ToUpper());
            if (allowedVehicles.Count == 0)
            {
                player.SendNotification(NotificationType.Error, $"Veículos Permitidos devem ser informados.");
                return;
            }

            foreach (var allowedVehicle in allowedVehicles)
            {
                if (!Functions.CheckIfVehicleExists(allowedVehicle))
                {
                    player.SendNotification(NotificationType.Error, $"Veículo {allowedVehicle} não existe.");
                    return;
                }
            }

            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            var truckerLocation = new TruckerLocation();
            if (isNew)
            {
                truckerLocation.Create(name, pos.X, pos.Y, pos.Z, deliveryValue, loadWaitTime, unloadWaitTime, Functions.Serialize(allowedVehicles));
            }
            else
            {
                truckerLocation = Global.TruckerLocations.FirstOrDefault(x => x.Id == id);
                if (truckerLocation == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                truckerLocation.Update(name, pos.X, pos.Y, pos.Z, deliveryValue, loadWaitTime, unloadWaitTime, Functions.Serialize(allowedVehicles));
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.TruckerLocations.AddAsync(truckerLocation);
            else
                context.TruckerLocations.Update(truckerLocation);

            await context.SaveChangesAsync();

            if (isNew)
                Global.TruckerLocations.Add(truckerLocation);

            truckerLocation.CreateIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Localização de Caminhoneiro | {Functions.Serialize(truckerLocation)}", null);
            player.SendNotification(NotificationType.Success, $"Localização de caminhoneiro {(isNew ? "criada" : "editada")}.");
            UpdateTruckerLocations();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffTruckerLocationsDeliveriesShow))]
    public static void StaffTruckerLocationsDeliveriesShow(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            player.Emit("StaffTruckerLocationDelivery:Show", GetTruckerLocationsDeliverysJson(id!.Value), idString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffTruckerLocationDeliveryGoto))]
    public static void StaffTruckerLocationDeliveryGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var truckerLocationDelivery = Global.TruckerLocationsDeliveries.FirstOrDefault(x => x.Id == id);
            if (truckerLocationDelivery is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(truckerLocationDelivery.PosX, truckerLocationDelivery.PosY, truckerLocationDelivery.PosZ), 0, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateTruckerLocationsDeliveries(Guid truckerLocationId)
    {
        var json = GetTruckerLocationsDeliverysJson(truckerLocationId);
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.TruckerLocations)))
            target.Emit("StaffTruckerLocationDelivery:Update", json);
    }

    [RemoteEvent(nameof(StaffTruckerLocationDeliverySave))]
    public async Task StaffTruckerLocationDeliverySave(Player playerParam,
        string truckerLocationDeliveryIdString,
        string truckerLocationIdString,
        Vector3 pos)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var truckerLocationDeliveryId = truckerLocationDeliveryIdString.ToGuid();
            var truckerLocationId = truckerLocationIdString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(truckerLocationDeliveryIdString);
            var truckerLocationDelivery = new TruckerLocationDelivery();
            if (isNew)
            {
                truckerLocationDelivery.Create(truckerLocationId!.Value, pos.X, pos.Y, pos.Z);
            }
            else
            {
                truckerLocationDelivery = Global.TruckerLocationsDeliveries.FirstOrDefault(x => x.Id == truckerLocationDeliveryId);
                if (truckerLocationDelivery == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                truckerLocationDelivery.Update(truckerLocationId!.Value, pos.X, pos.Y, pos.Z);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.TruckerLocationsDeliveries.AddAsync(truckerLocationDelivery);
            else
                context.TruckerLocationsDeliveries.Update(truckerLocationDelivery);

            await context.SaveChangesAsync();

            if (isNew)
                Global.TruckerLocationsDeliveries.Add(truckerLocationDelivery);

            await player.WriteLog(LogType.Staff, $"Gravar Entrega Localização de Caminhoneiro | {Functions.Serialize(truckerLocationDelivery)}", null);
            player.SendNotification(NotificationType.Success, $"Entrega {(isNew ? "criada" : "editada")}.");
            UpdateTruckerLocationsDeliveries(truckerLocationDelivery.TruckerLocationId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffTruckerLocationDeliveryRemove))]
    public async Task StaffTruckerLocationDeliveryRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.TruckerLocations))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var truckerLocationDelivery = Global.TruckerLocationsDeliveries.FirstOrDefault(x => x.Id == id);
            if (truckerLocationDelivery == null)
                return;

            var context = Functions.GetDatabaseContext();
            context.TruckerLocationsDeliveries.Remove(truckerLocationDelivery);
            await context.SaveChangesAsync();
            Global.TruckerLocationsDeliveries.Remove(truckerLocationDelivery);

            await player.WriteLog(LogType.Staff, $"Remover Entrega da Localização de Caminhoneiro | {Functions.Serialize(truckerLocationDelivery)}", null);
            player.SendNotification(NotificationType.Success, "Entrega excluída.");
            UpdateTruckerLocationsDeliveries(truckerLocationDelivery.TruckerLocationId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    public static string GetTruckerLocationsJson()
    {
        return Functions.Serialize(Global.TruckerLocations.OrderByDescending(x => x.RegisterDate).Select(x => new
        {
            x.Id,
            x.Name,
            x.PosX,
            x.PosY,
            x.PosZ,
            x.DeliveryValue,
            x.LoadWaitTime,
            x.UnloadWaitTime,
            AllowedVehicles = x.GetAllowedVehicles(),
        }));
    }

    public static string GetTruckerLocationsDeliverysJson(Guid truckerLocationId)
    {
        return Functions.Serialize(Global.TruckerLocationsDeliveries
            .Where(x => x.TruckerLocationId == truckerLocationId)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.PosX,
                x.PosY,
                x.PosZ,
            }));
    }
}