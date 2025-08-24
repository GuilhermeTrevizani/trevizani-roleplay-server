using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Requests;

public class SaveFactionMemberRequest
{
    public Guid FactionId { get; set; }
    public Guid Id { get; set; }
    public Guid FactionRankId { get; set; }
    public IEnumerable<FactionFlag> Flags { get; set; } = [];
}