using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Extensions;

public static class PropertyFurnitureExtension
{
    public static string GetModelName(this PropertyFurniture propertyFurniture)
        => GetFurniture(propertyFurniture)?.Name ?? string.Empty;

    public static Furniture? GetFurniture(this PropertyFurniture propertyFurniture)
        => Global.Furnitures.FirstOrDefault(x => x.Model.ToLower() == propertyFurniture.Model.ToLower());

    public static void CreateObject(this PropertyFurniture propertyFurniture)
    {
        Functions.RunOnMainThread(() =>
        {
            var myObject = Functions.CreateObject(
                propertyFurniture.Model,
                new(propertyFurniture.PosX, propertyFurniture.PosY, propertyFurniture.PosZ),
                new(propertyFurniture.RotR, propertyFurniture.RotP, propertyFurniture.RotY),
                propertyFurniture.Interior ? propertyFurniture.Property!.Number : propertyFurniture.Property!.EntranceDimension,
                true, true, propertyFurniture.Id,
                Global.Furnitures.FirstOrDefault(x => x.Model.ToLower() == propertyFurniture.Model.ToLower())?.Door ?? false,
                propertyFurniture.Locked);

            myObject.PropertyFurnitureId = propertyFurniture.Id;
        });
    }

    public static void DeleteObject(this PropertyFurniture propertyFurniture)
    {
        Functions.RunOnMainThread(() =>
        {
            var myObject = Global.Objects.FirstOrDefault(x => x.PropertyFurnitureId == propertyFurniture.Id);
            myObject?.DestroyObject();
        });
    }

    public static Vector3 GetPosition(this PropertyFurniture propertyFurniture)
        => new(propertyFurniture.PosX, propertyFurniture.PosY, propertyFurniture.PosZ);
}