using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Extensions;

public static class CrackDenExtension
{
    public static void CreateIdentifier(this CrackDen crackDen)
    {
        RemoveIdentifier(crackDen);

        Functions.RunOnMainThread(() =>
        {
            var pos = new Vector3(crackDen.PosX, crackDen.PosY, crackDen.PosZ - 0.95f);

            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, crackDen.Dimension);
            marker.CrackDenId = crackDen.Id;

            var colShape = Functions.CreateColShapeCylinder(pos, 1, 1.5f, crackDen.Dimension);
            colShape.Description = $"[BOCA DE FUMO] {{#FFFFFF}}Use /bocafumo.";
            colShape.CrackDenId = crackDen.Id;
        });
    }

    public static void RemoveIdentifier(this CrackDen crackDen)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.CrackDenId == crackDen.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.CrackDenId == crackDen.Id);
            colShape?.Delete();
        });
    }
}