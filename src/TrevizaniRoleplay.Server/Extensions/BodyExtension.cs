using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Extensions;

public static class BodyExtension
{
    public static void CreateIdentifier(this Body body)
    {
        RemoveIdentifier(body);

        var ped = Functions.CreatePed(body.Model, new(body.PosX, body.PosY, body.PosZ), new(0, 0, 0), body.Dimension, body);
        ped.BodyId = body.Id;
    }

    public static void RemoveIdentifier(this Body body)
    {
        var ped = Global.Peds.FirstOrDefault(x => x.BodyId == body.Id);
        ped?.RemoveAllClients();
    }

    public static void ShowInventory(this Body body, MyPlayer player, bool update)
    {
        player.ShowInventory(InventoryShowType.Body,
            "Corpo",
            Functions.Serialize(
                body.Items!.Select(x => new
                {
                    x.Id,
                    Image = x.GetImage(),
                    Name = x.GetName(),
                    x.Quantity,
                    x.Slot,
                    Extra = x.GetExtra(),
                    Weight = x.GetWeight(),
                    IsStack = x.GetIsStack(),
                })
        ), update, body.Id);
    }

    public static bool IsInformationAvailable(this Body body)
    {
        var minHours = 6;
        var hours = (DateTime.Now - (body.MorgueDate ?? DateTime.Now)).TotalHours;
        return hours >= minHours;
    }
}