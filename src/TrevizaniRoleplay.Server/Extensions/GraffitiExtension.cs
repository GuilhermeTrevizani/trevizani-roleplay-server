using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Extensions;

public static class GraffitiExtension
{
    public static void CreateIdentifier(this Graffiti graffiti)
    {
        RemoveIdentifier(graffiti);

        Functions.RunOnMainThread(() =>
        {
            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_QUESTION, new Vector3(graffiti.PosX, graffiti.PosY, graffiti.PosZ),
            0.2f, new Color(226, 203, 135, 170), graffiti.Dimension);
            marker.GraffitiId = graffiti.Id;

            var colShape = Functions.CreateColShapeCylinder(new Vector3(graffiti.PosX, graffiti.PosY, graffiti.PosZ), 5, 1.5f, graffiti.Dimension);
            colShape.Description = $"[INFO] {{#FFFFFF}}Você pode visualizar um grafite: {graffiti.Text}";
            colShape.GraffitiId = graffiti.Id;
        });
    }

    public static void RemoveIdentifier(this Graffiti graffiti)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.GraffitiId == graffiti.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.GraffitiId == graffiti.Id);
            colShape?.Delete();
        });
    }
}