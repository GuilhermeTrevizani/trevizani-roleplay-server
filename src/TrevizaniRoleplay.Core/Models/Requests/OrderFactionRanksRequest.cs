namespace TrevizaniRoleplay.Core.Models.Requests;

public class OrderFactionRanksRequest
{
    public Guid FactionId { get; set; }
    public IEnumerable<Rank> Ranks { get; set; } = [];

    public class Rank
    {
        public Guid Id { get; set; }
        public int Position { get; set; }
    }
}