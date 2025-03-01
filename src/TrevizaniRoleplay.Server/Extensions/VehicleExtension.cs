using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Extensions;

public static class VehicleExtension
{
    public static int GetMaxFuel(this Domain.Entities.Vehicle vehicle)
    {
        return 100;
    }

    public static async Task<MyVehicle> Spawnar(this Domain.Entities.Vehicle vehicle, MyPlayer? player)
    {
        var veh = Functions.CreateVehicle(vehicle.Model,
            new(vehicle.PosX, vehicle.PosY, vehicle.PosZ),
            new(vehicle.RotR, vehicle.RotP, vehicle.RotY),
            player is not null ? MyVehicleSpawnType.Normal : MyVehicleSpawnType.Rent);
        veh.SetDimension(vehicle.Dimension);
        veh.VehicleDB = vehicle;
        Functions.RunOnMainThread(() =>
        {
            veh.NumberPlate = veh.VehicleDB.Plate;
        });
        veh.SetLocked(true);

        if (veh.HasFuelTank)
            veh.SetSharedDataEx(Constants.VEHICLE_META_DATA_FUEL, veh.FuelHUD);

        veh.SetDefaultMods();

        veh.Damages = Functions.Deserialize<List<VehicleDamage>>(vehicle.DamagesJSON);

        if (player is not null)
        {
            Functions.RunOnMainThread(() =>
            {
                NAPI.Vehicle.SetVehicleEngineHealth(veh, veh.VehicleDB.EngineHealth);
                NAPI.Vehicle.SetVehicleBodyHealth(veh, veh.VehicleDB.BodyHealth);
            });
        }

        veh.Timer.Elapsed += (o, e) =>
        {
            try
            {
                Functions.ConsoleLog($"Vehicle Timer {veh.Identifier}");
                if (veh.RentExpirationDate.HasValue)
                {
                    if (veh.RentExpirationDate.Value < DateTime.Now)
                    {
                        veh.SetEngineStatus(false);
                        veh.NameInCharge = string.Empty;
                        veh.RentExpirationDate = null;
                        if (veh.Driver is MyPlayer driver)
                            driver.SendMessage(MessageType.Error, "O aluguel do veículo expirou. Use /valugar para alugar novamente por uma hora. Se você sair do veículo, ele será levado para a central.");
                    }
                }

                if (veh.GetEngineStatus() && veh.VehicleDB.Fuel > 0 && veh.HasFuelTank)
                {
                    veh.SetFuel(veh.VehicleDB.Fuel - 1);
                    if (veh.VehicleDB.Fuel == 0)
                        veh.SetEngineStatus(false);
                }
            }
            catch (Exception ex)
            {
                Functions.GetException(ex);
            }
        };
        veh.Timer.Start();

        if (player is not null)
            await player.WriteLog(LogType.SpawnVehicle, $"{vehicle.Id} - {vehicle.Model} - {vehicle.Plate}", null);

        return veh;
    }
}