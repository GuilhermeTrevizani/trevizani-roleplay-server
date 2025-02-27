using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyColShapeFactory : Script
{
    public MyColShapeFactory()
    {
        RAGE.Entities.Colshapes.CreateEntity = Create;
    }

    protected MyColShape Create(NetHandle netHandle)
    {
        var entity = (MyColShape?)Activator.CreateInstance(typeof(MyColShape), netHandle)
            ?? throw new Exception("Error at MyColShapeFactory : entity is null");

        return entity!;
    }
}