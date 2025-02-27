using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class DebugScript : Script
{
    [Command("x")]
    public static void CMD_x(MyPlayer player)
    {
        if (player.User.Staff < Domain.Enums.UserStaff.JuniorServerAdmin)
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        Global.AudioSpots.RemoveAll(x => x is null);
        Global.AudioSpots.Where(x => x.Source.Contains("sms")).ToList().ForEach(x => x.RemoveAllClients());
        player.SendMessage(MessageType.Success, "/x");
    }

    [Command("x2", "/x2 (component) (drawable) (texture)")]
    public static void CMD_x2(MyPlayer player, int component, int drawable, int texture)
    {
        if (player.User.Staff < Domain.Enums.UserStaff.HeadServerDeveloper)
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        player.SetClothes(component, drawable, texture);

        player.SendMessage(MessageType.Success, $"/x2 {component} {drawable} {texture}");
    }

    [Command("x3", "/x3 (component) (drawable) (texture)")]
    public static void CMD_x3(MyPlayer player, int component, int drawable, int texture)
    {
        if (player.User.Staff < Domain.Enums.UserStaff.HeadServerDeveloper)
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        player.SetAccessories(component, drawable, texture);

        player.SendMessage(MessageType.Success, $"/x3 {component} {drawable} {texture}");
    }

    [Command("x4", "/x4 (collection) (overlay)")]
    public static void CMD_x4(MyPlayer player, string collection, string overlay)
    {
        if (player.User.Staff < Domain.Enums.UserStaff.HeadServerDeveloper)
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        var collectionHash = Functions.Hash(collection);
        var overlayHash = Functions.Hash(overlay);
        var msg = $"/x4 {collection} {overlay} {collectionHash} {overlayHash}";
        player.SetDecoration(new Decoration()
        {
            Collection = collectionHash,
            Overlay = overlayHash,
        });
        player.SendMessage(MessageType.None, msg);
        Functions.ConsoleLog(msg);
    }
}