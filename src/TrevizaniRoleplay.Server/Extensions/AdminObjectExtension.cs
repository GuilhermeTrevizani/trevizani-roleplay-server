namespace TrevizaniRoleplay.Server.Extensions;

public static class AdminObjectExtension
{
    public static void CreateObject(this AdminObject adminObject)
    {
        DeleteObject(adminObject);

        Functions.RunOnMainThread(() =>
        {
            var myObject = Functions.CreateObject(adminObject.Model,
            new(adminObject.PosX, adminObject.PosY, adminObject.PosZ),
            new(adminObject.RotR, adminObject.RotP, adminObject.RotY), adminObject.Dimension, true, true);

            myObject.AdminObjectId = adminObject.Id;
        });
    }

    public static void DeleteObject(this AdminObject adminObject)
    {
        Functions.RunOnMainThread(() =>
        {
            var myObject = Global.Objects.FirstOrDefault(x => x.AdminObjectId == adminObject.Id);
            myObject?.DestroyObject();
        });
    }
}