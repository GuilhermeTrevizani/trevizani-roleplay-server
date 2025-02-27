using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Server.Extensions;

public static class PropertyEntranceExtension
{
    public static Property GetProperty(this PropertyEntrance propertyEntrance)
        => Global.Properties.FirstOrDefault(x => x.Id == propertyEntrance.PropertyId)!;

    public static Vector3 GetEntrancePosition(this PropertyEntrance property)
        => new(property.EntrancePosX, property.EntrancePosY, property.EntrancePosZ);

    public static Vector3 GetEntranceRotation(this PropertyEntrance property)
        => new(property.EntranceRotR, property.EntranceRotP, property.EntranceRotY);

    public static Vector3 GetExitPosition(this PropertyEntrance property)
        => new(property.ExitPosX, property.ExitPosY, property.ExitPosZ);

    public static Vector3 GetExitRotation(this PropertyEntrance property)
        => new(property.ExitRotR, property.ExitRotP, property.ExitRotY);
}