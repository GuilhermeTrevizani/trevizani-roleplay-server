using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class ColShapeScript : Script
{
    [ServerEvent(Event.PlayerEnterColshape)]
    public static void OnColShape(ColShape colShapeParam, Player playerParam)
    {
        try
        {
            var colShape = Functions.CastColshape(colShapeParam);
            var player = Functions.CastPlayer(playerParam);

            if (colShape.Object is not null)
            {
                var model = colShape.Object.Model;
                if (model == Functions.Hash(Constants.METAL_DETECTOR_OBJECT_MODEL))
                {
                    if (player.Items.Any(x => x.GetCategory() == ItemCategory.Weapon))
                        player.SendMessageToNearbyPlayers("O detector de metal começa a apitar.", MessageCategory.NormalDo);

                    return;
                }
                else if (model == Functions.Hash(Constants.SPIKE_STRIP_OBJECT_MODEL))
                {
                    if (player.Vehicle is MyVehicle vehicleSpikeStrip && vehicleSpikeStrip.Driver == player
                        && !vehicleSpikeStrip.GetSharedDataEx<bool>("TyresBurst"))
                    {
                        vehicleSpikeStrip.SetSharedDataEx("TyresBurst", true);
                    }

                    return;
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(colShape.Description))
            {
                player.SendMessage(MessageType.None, colShape.Description, Constants.MAIN_COLOR);
                return;
            }

            if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
                return;

            if (colShape.PoliceOfficerCharacterId.HasValue && colShape.MaxSpeed.HasValue)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == colShape.PoliceOfficerCharacterId);
                target?.SendMessage(MessageType.None, $"[RADAR] {{{(vehicle.Speed > colShape.MaxSpeed ? Constants.ERROR_COLOR : Constants.SUCCESS_COLOR)}}}{vehicle.Speed} {{#FFFFFF}}MPH.");
                return;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}