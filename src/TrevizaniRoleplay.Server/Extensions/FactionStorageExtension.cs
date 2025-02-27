using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Extensions;

public static class FactionStorageExtension
{
    public static void CreateIdentifier(this FactionStorage factionStorage)
    {
        RemoveIdentifier(factionStorage);

        Functions.RunOnMainThread(() =>
        {
            var pos = new Vector3(factionStorage.PosX, factionStorage.PosY, factionStorage.PosZ - 0.95f);

            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, factionStorage.Dimension);
            marker.FactionStorageId = factionStorage.Id;

            var colShape = Functions.CreateColShapeCylinder(pos, 1, 1.5f, factionStorage.Dimension);
            colShape.Description = $"[ARMAZENAMENTO] {{#FFFFFF}}Use /farmazenamento.";
            colShape.FactionStorageId = factionStorage.Id;
        });
    }

    public static void RemoveIdentifier(this FactionStorage factionStorage)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.FactionStorageId == factionStorage.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.FactionStorageId == factionStorage.Id);
            colShape?.Delete();
        });
    }
}