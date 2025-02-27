using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyObjectFactory : Script
{
    public MyObjectFactory()
    {
        RAGE.Entities.Objects.CreateEntity = Create;
    }

    protected MyObject Create(NetHandle netHandle)
    {
        var entity = (MyObject?)Activator.CreateInstance(typeof(MyObject), netHandle)
            ?? throw new Exception("Error at MyObjectFactory : entity is null");

        return entity!;
    }
}