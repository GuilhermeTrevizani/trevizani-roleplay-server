namespace TrevizaniRoleplay.Server.Extensions;

public static class BlipExtension
{
    public static void CreateIdentifier(this Blip blip)
    {
        RemoveIdentifier(blip);

        Functions.RunOnMainThread(() =>
        {
            var myBlip = Functions.CreateBlip(new(blip.PosX, blip.PosY, blip.PosZ), blip.Type, blip.Color, blip.Name, 0.8f, true);
            myBlip.BlipId = blip.Id;
        });
    }

    public static void RemoveIdentifier(this Blip blip)
    {
        Functions.RunOnMainThread(() =>
        {
            var myBlip = Global.MyBlips.FirstOrDefault(x => x.BlipId == blip.Id);
            myBlip?.Delete();
        });
    }
}