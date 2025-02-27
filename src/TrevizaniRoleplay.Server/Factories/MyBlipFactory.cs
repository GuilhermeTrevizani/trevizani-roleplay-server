using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyBlipFactory : Script
{
    public MyBlipFactory()
    {
        RAGE.Entities.Blips.CreateEntity = Create;
    }

    protected MyBlip Create(NetHandle netHandle)
    {
        var entity = (MyBlip?)Activator.CreateInstance(typeof(MyBlip), netHandle)
            ?? throw new Exception("Error at BlipFactory : entity is null");

        return entity!;
    }
}