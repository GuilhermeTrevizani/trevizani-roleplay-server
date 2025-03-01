using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Extensions;

public static class DealershipExtension
{
    public static void CreateIdentifier(this Dealership dealership)
    {
        RemoveIdentifier(dealership);

        Functions.RunOnMainThread(() =>
        {
            var pos = new Vector3(dealership.PosX, dealership.PosY, dealership.PosZ - 0.95f);

            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, 0);
            marker.DealershipId = dealership.Id;

            var colShape = Functions.CreateColShapeCylinder(pos, 1, 1.5f, 0);
            colShape.Description = $"[{dealership.Name}] {{#FFFFFF}}{Resources.PressYToInteract}";
            colShape.DealershipId = dealership.Id;
        });
    }

    public static void RemoveIdentifier(this Dealership dealership)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.DealershipId == dealership.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.DealershipId == dealership.Id);
            colShape?.Delete();
        });
    }
}