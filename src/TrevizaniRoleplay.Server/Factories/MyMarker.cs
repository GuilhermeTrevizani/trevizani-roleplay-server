using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Factories;

public class MyMarker(NetHandle handle) : Marker(handle)
{
    public Guid? SpotId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? CrackDenId { get; set; }
    public Guid? DealershipId { get; set; }
    public Guid? FactionStorageId { get; set; }
    public Guid? InfoId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? JobId { get; set; }
    public Guid? TruckerLocationId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? GraffitiId { get; set; }
}