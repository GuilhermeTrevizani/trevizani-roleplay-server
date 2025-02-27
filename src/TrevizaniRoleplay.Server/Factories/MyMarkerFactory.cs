using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyMarkerFactory : Script
{
    public MyMarkerFactory()
    {
        RAGE.Entities.Markers.CreateEntity = Create;
    }

    protected MyMarker Create(NetHandle netHandle)
    {
        var entity = (MyMarker?)Activator.CreateInstance(typeof(MyMarker), netHandle)
            ?? throw new Exception("Error at MyMarkerFactory : entity is null");

        return entity!;
    }
}