using GTANetworkAPI;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Extensions;

public static class ItemExtension
{
    public static void CreateObject(this Item item)
    {
        Functions.RunOnMainThread(() =>
        {
            var myObject = Functions.CreateObject(item.GetObjectName(),
                new(item.PosX, item.PosY, item.PosZ),
                new(item.RotR, item.RotP, item.RotY), item.Dimension, true, false);

            myObject.ItemId = item.Id;

            if (GlobalFunctions.CheckIfIsBulletShell(item.GetCategory()))
            {
                var marker = Functions.CreateMarker(Constants.MARKER_TYPE_QUESTION,
                    new(item.PosX, item.PosY, item.PosZ + 0.1f),
                    0.05f,
                    Global.MainRgba,
                    item.Dimension);
                marker.ItemId = item.Id;
            }

            UpdateGroundItems(item);
        });
    }

    public static void DeleteObject(this Item item)
    {
        Functions.RunOnMainThread(() =>
        {
            var myObject = Global.Objects.FirstOrDefault(x => x.ItemId == item.Id);
            myObject?.DestroyObject();

            var audioSpot = Global.AudioSpots.FirstOrDefault(x => x?.ItemId == item.Id);
            audioSpot?.RemoveAllClients();

            var marker = Global.Markers.FirstOrDefault(x => x.ItemId == item.Id);
            marker?.Delete();

            Global.Items.Remove(item);
            UpdateGroundItems(item);
        });
    }

    private static void UpdateGroundItems(this Item item)
    {
        var position = new Vector3(item.PosX, item.PosY, item.PosZ);
        foreach (var player in Global.SpawnedPlayers.Where(x => x.GetDimension() == item.Dimension && position.DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE))
            player.ShowInventory(update: true);
    }

    public static AudioSpot? GetAudioSpot(this Item item)
        => Global.AudioSpots.FirstOrDefault(x => x?.ItemId == item.Id);
}