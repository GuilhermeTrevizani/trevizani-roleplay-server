using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyColShape(NetHandle netHandle) : ColShape(netHandle)
{
    public string? Description { get; set; }
    public Guid? PoliceOfficerCharacterId { get; set; }
    public int? MaxSpeed { get; set; }
    public Guid? InfoId { get; set; }
    public Guid? TruckerLocationId { get; set; }
    public Guid? SpotId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? CrackDenId { get; set; }
    public Guid? FactionStorageId { get; set; }
    public Guid? DealershipId { get; set; }
    public Guid? SmugglerId { get; set; }
    public MyObject? Object { get; set; }
    public Guid? JobId { get; set; }
    public Guid? GraffitiId { get; set; }
}