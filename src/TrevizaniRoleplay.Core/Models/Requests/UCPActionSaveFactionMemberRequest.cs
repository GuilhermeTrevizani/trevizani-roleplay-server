using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Requests;

public class UCPActionSaveFactionMemberRequest
{
    public Guid Id { get; set; }
    public Guid FactionRankId { get; set; }
    public IEnumerable<FactionFlag> Flags { get; set; } = [];
}