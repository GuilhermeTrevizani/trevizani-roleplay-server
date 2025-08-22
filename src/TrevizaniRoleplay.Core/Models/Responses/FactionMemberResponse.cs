namespace TrevizaniRoleplay.Core.Models.Responses;

public class FactionMemberResponse
{
    public string RankName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string User { get; set; } = default!;
    public DateTime LastAccessDate { get; set; }
    public bool IsOnline { get; set; }
}