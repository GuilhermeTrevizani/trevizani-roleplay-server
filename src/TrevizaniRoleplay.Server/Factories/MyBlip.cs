using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyBlip(NetHandle netHandle) : Blip(netHandle)
{
    public Guid? BlipId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? JobId { get; set; }
}