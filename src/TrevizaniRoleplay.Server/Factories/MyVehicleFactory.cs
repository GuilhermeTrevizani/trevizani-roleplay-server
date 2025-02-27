using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyVehicleFactory : Script
{
    public MyVehicleFactory()
    {
        RAGE.Entities.Vehicles.CreateEntity = Create;
    }

    protected MyVehicle Create(NetHandle netHandle)
    {
        var entity = (MyVehicle?)Activator.CreateInstance(typeof(MyVehicle), netHandle)
            ?? throw new Exception("Error at MyVehicleFactory : entity is null");

        return entity!;
    }
}