using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Extensions;

public static class TruckerLocationExtension
{
    public static List<string> GetAllowedVehicles(this TruckerLocation truckerLocation)
        => Functions.Deserialize<List<string>>(truckerLocation.AllowedVehiclesJSON);

    public static void CreateIdentifier(this TruckerLocation truckerLocation)
    {
        RemoveIdentifier(truckerLocation);

        Functions.RunOnMainThread(() =>
        {
            var pos = new Vector3(truckerLocation.PosX, truckerLocation.PosY, truckerLocation.PosZ - 0.95f);

            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, 0);
            marker.TruckerLocationId = truckerLocation.Id;

            var colShape = Functions.CreateColShapeCylinder(pos, 1, 1.5f, 0);
            colShape.Description = $"[{truckerLocation.Name}] {{#FFFFFF}}Use /carregarcaixas ou /cancelarcaixas.";
            colShape.TruckerLocationId = truckerLocation.Id;
        });
    }

    public static void RemoveIdentifier(this TruckerLocation truckerLocation)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.TruckerLocationId == truckerLocation.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.TruckerLocationId == truckerLocation.Id);
            colShape?.Delete();
        });
    }
}