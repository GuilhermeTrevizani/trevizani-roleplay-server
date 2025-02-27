using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyObject(NetHandle netHandle) : GTANetworkAPI.Object(netHandle)
{
    public Guid? AdminObjectId { get; set; }
    public string TVSource { get; set; } = string.Empty;
    public float TVVolume { get; set; } = 100;
    public Guid? CharacterId { get; set; }
    public Guid? FactionId { get; set; }
    public Guid? PropertyFurnitureId { get; set; }
    public Guid? ItemId { get; set; }

    public void DestroyObject()
    {
        var colShape = Global.ColShapes.FirstOrDefault(x => x.Object == this);
        colShape?.Delete();

        Delete();
    }

    public void SetSharedDataEx(string key, object? value)
    {
        Functions.RunOnMainThread(() =>
        {
            SetSharedData(key, value);
        });
    }

    public void ResetSharedDataEx(string key)
    {
        Functions.RunOnMainThread(() =>
        {
            ResetSharedData(key);
        });
    }

    public uint GetDimension()
    {
        return Functions.RunOnMainThread(() => Dimension);
    }

    public Vector3 GetPosition()
    {
        return Functions.RunOnMainThread(() => Position);
    }

    public uint GetModel()
    {
        return Functions.RunOnMainThread(() => Model);
    }
}