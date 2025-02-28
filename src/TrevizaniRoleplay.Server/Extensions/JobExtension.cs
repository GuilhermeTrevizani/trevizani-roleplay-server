using GTANetworkAPI;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Extensions;

public static class JobExtension
{
    public static void CreateIdentifier(this Job job)
    {
        RemoveIdentifier(job);

        if (job.PosX == 0)
            return;

        Functions.RunOnMainThread(() =>
        {
            var position = new Vector3(job.PosX, job.PosY, job.PosZ - 0.95f);

            var blip = Functions.CreateBlip(position, job.BlipType, job.BlipColor, job.BlipName, 0.8f, true);
            blip.JobId = job.Id;

            var name = job.CharacterJob.GetDescription().ToUpper();

            var colShape = Functions.CreateColShapeCylinder(position, 1, 1.5f, 0);
            colShape.Description = $"[EMPREGO DE {name}] {{#FFFFFF}}Use /emprego para se tornar um {name.ToLower()}.";
            colShape.JobId = job.Id;

            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, position, Constants.MARKER_SCALE, Global.MainRgba, 0);
            marker.JobId = job.Id;

            if (string.IsNullOrWhiteSpace(job.VehicleRentModel) || job.VehicleRentValue <= 0)
                return;

            var positionRent = new Vector3(job.VehicleRentPosX, job.VehicleRentPosY, job.VehicleRentPosZ - 0.3f);

            var colShapeRent = Functions.CreateColShapeCylinder(positionRent, 1, 1.5f, 0);
            colShapeRent.Description = $"[VEÍCULOS DE {name}] {{#FFFFFF}}Use /valugar para alugar um veículo por ${job.VehicleRentValue:N0}.";
            colShapeRent.JobId = job.Id;

            var markerRent = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, positionRent, Constants.MARKER_SCALE, Global.MainRgba, 0);
            markerRent.JobId = job.Id;
        });
    }

    private static void RemoveIdentifier(this Job job)
    {
        Functions.RunOnMainThread(() =>
        {
            var markers = Global.Markers.Where(x => x.JobId == job.Id);
            foreach (var marker in markers)
                marker.Delete();

            var colShapes = Global.ColShapes.Where(x => x.JobId == job.Id);
            foreach (var colShape in colShapes)
                colShape.Delete();

            var blip = Global.MyBlips.FirstOrDefault(x => x.JobId == job.Id);
            blip?.Delete();
        });
    }
}