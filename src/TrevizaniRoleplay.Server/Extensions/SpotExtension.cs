using GTANetworkAPI;
using TrevizaniRoleplay.Server.Factories;

namespace TrevizaniRoleplay.Server.Extensions;

public static class SpotExtension
{
    public static void CreateIdentifier(this Spot spot)
    {
        RemoveIdentifier(spot);

        string name;
        string description;

        switch (spot.Type)
        {
            case SpotType.Bank:
                name = "BANCO";
                description = Resources.PressYToInteract;
                break;
            case SpotType.Company:
                name = "EMPRESA";
                description = Resources.PressYToInteract;
                break;
            case SpotType.ClothesStore:
                name = "LOJA DE ROUPAS";
                description = Resources.PressYToInteract;
                break;
            case SpotType.FactionVehicleSpawn:
                name = "SPAWN DE VEÍCULOS DA FACÇÃO";
                description = Resources.PressYToInteract;
                break;
            case SpotType.VehicleSeizure:
                name = "APREENSÃO DE VEÍCULOS";
                description = "Use /apreender.";
                break;
            case SpotType.VehicleRelease:
                name = "LIBERAÇÃO DE VEÍCULOS";
                description = "Use /vlista para liberar um veículo.";
                break;
            case SpotType.BarberShop:
                name = "BARBEARIA";
                description = Resources.PressYToInteract;
                break;
            case SpotType.DMV:
                name = "DMV";
                description = Resources.PressYToInteract;
                break;
            case SpotType.HealMe:
                name = "TRATAMENTO DE FERIDOS";
                description = Resources.PressYToInteract;
                break;
            case SpotType.TattooShop:
                name = "ESTÚDIO DE TATUAGENS";
                description = Resources.PressYToInteract;
                break;
            case SpotType.PlasticSurgery:
                name = "CIRURGIA PLÁSTICA";
                description = Resources.PressYToInteract;
                break;
            case SpotType.ForensicLaboratory:
                name = Resources.ForensicLaboratory;
                description = Resources.PressYToInteract;
                break;
            case SpotType.Morgue:
                name = Resources.Morgue;
                description = Resources.PressYToInteract;
                break;
            default:
                return;
        }

        Functions.RunOnMainThread(() =>
        {
            var pos = spot.GetPosition();
            pos.Z -= 0.95f;

            var marker = Functions.CreateMarker(Constants.MARKER_TYPE_HALO, pos, Constants.MARKER_SCALE, Global.MainRgba, spot.Dimension);
            marker.SpotId = spot.Id;

            var colShape = Functions.CreateColShapeCylinder(pos, 1, 1.5f, spot.Dimension);
            colShape.Description = $"[{name.ToUpper()}] {{#FFFFFF}}{description}";
            colShape.SpotId = spot.Id;
        });
    }

    public static void RemoveIdentifier(this Spot spot)
    {
        Functions.RunOnMainThread(() =>
        {
            var marker = Global.Markers.FirstOrDefault(x => x.SpotId == spot.Id);
            marker?.Delete();

            var colShape = Global.ColShapes.FirstOrDefault(x => x.SpotId == spot.Id);
            colShape?.Delete();

            NAPI.ClientEventThreadSafe.TriggerClientEventForAll("RemoveSpot", spot.Id.ToString());
        });
    }

    public static void CreateBlipAndMarkerForClient(this Spot spot, MyPlayer player, Vector3 position,
        int blipSprite, int blipColor, float blipScale, string blipName,
        int markerType, float markerScale, Color markerColor)
    {
        player.Emit("AddSpot", spot.Id.ToString(), position,
            blipSprite, blipColor, blipScale, blipName,
            markerType, markerScale, new List<int> { markerColor.Red, markerColor.Green, markerColor.Blue, markerColor.Alpha });
    }

    public static Vector3 GetPosition(this Spot spot)
        => new(spot.PosX, spot.PosY, spot.PosZ);
}