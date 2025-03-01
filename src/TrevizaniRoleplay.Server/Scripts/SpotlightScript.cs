using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class SpotlightScript : Script
{
    [RemoteEvent(nameof(SpotlightAdd))]
    public static void SpotlightAdd(Player playerParam, Vector3 position, Vector3 direction,
        float distance, float brightness, float hardness, float radius, float falloff,
        bool helicoptero)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!helicoptero)
            {
                var veh = (MyVehicle)player.Vehicle;
                if (!(veh?.SpotlightActive ?? false))
                    return;
            }

            var helilight = Global.Spotlights.FirstOrDefault(x => x.Id == player.Vehicle.Id);
            if (helilight is null)
            {
                helilight = new Spotlight
                {
                    Id = player.Vehicle.Id,
                    Position = position,
                    Direction = direction,
                    Player = player.SessionId,
                    Distance = distance,
                    Brightness = brightness,
                    Hardness = hardness,
                    Radius = radius,
                    Falloff = falloff,
                };
                Global.Spotlights.Add(helilight);
            }
            else
            {
                if (helilight.Player != player.SessionId)
                {
                    player.SendNotification(NotificationType.Error, "Outro jogador está com a luz do helicóptero ativa.");
                    player.Emit("Spotlight:Cancel");
                    return;
                }

                helilight.Position = position;
                helilight.Direction = direction;
            }

            NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Spotlight:Add", helilight.Id, helilight.Position, helilight.Direction,
                helilight.Distance, helilight.Brightness, helilight.Hardness, helilight.Radius, helilight.Falloff);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SpotlightRemove))]
    public static void SpotlightRemove(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var spotlight = Global.Spotlights.FirstOrDefault(x => x.Id == player.Vehicle.Id && x.Player == player.SessionId);
            if (spotlight is not null)
            {
                Global.Spotlights.Remove(spotlight);
                NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Spotlight:Remove", spotlight.Id);
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(HelicamToggle))]
    public static void HelicamToggle(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!(player.Faction?.Government ?? false) || !player.OnDuty)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção governamental ou não está em serviço.");
                return;
            }

            if (player.Vehicle is not MyVehicle vehicle)
            {
                player.SendNotification(NotificationType.Error, "Você não está em um veículo.");
                return;
            }

            var model = vehicle.VehicleDB.Model.ToLower();
            if ((player.VehicleSeat != Constants.VEHICLE_SEAT_DRIVER && player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_FRONT_RIGHT)
                || (model != VehicleModel.Polmav.ToString().ToLower()
                    && model != VehicleModelMods.AS332.ToString().ToLower()
                    && model != VehicleModelMods.AS350.ToString().ToLower()
                    && model != VehicleModelMods.LGUARDMAV.ToString().ToLower()
                    && model != VehicleModelMods.AW139.ToString().ToLower())
            )
            {
                player.SendMessage(MessageType.Error, "Você não está nos assentos dianteiros de um helicóptero apropriado.");
                return;
            }

            player.Emit("Helicam:Toggle");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}