using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Extensions;

public static class InfoExtension
{
    public static void CreateIdentifier(this Info info)
    {
        RemoveIdentifier(info);

        Functions.RunOnMainThread(() =>
        {
            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_QUESTION, new Vector3(info.PosX, info.PosY, info.PosZ),
            0.2f, new Color(226, 203, 135, 170), info.Dimension);
            marker.InfoId = info.Id;

            var colShape = Functions.CreateColShapeCylinder(new Vector3(info.PosX, info.PosY, info.PosZ), 1, 1.5f, info.Dimension);
            colShape.Description = $"[INFO] {{#FFFFFF}}{(info.Image ? "Pressione Y para visualizar a imagem." : info.Message)}";
            colShape.InfoId = info.Id;
        });
    }

    public static void RemoveIdentifier(this Info info)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.InfoId == info.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.InfoId == info.Id);
            colShape?.Delete();
        });
    }
}