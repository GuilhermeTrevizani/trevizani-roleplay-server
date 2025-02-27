using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyPlayerFactory : Script
{
    public MyPlayerFactory()
    {
        RAGE.Entities.Players.CreateEntity = Create;
    }

    protected MyPlayer Create(NetHandle netHandle)
    {
        var entity = (MyPlayer?)Activator.CreateInstance(typeof(MyPlayer), netHandle)
            ?? throw new Exception("Error at MyPlayerFactory : entity is null");

        return entity!;
    }
}