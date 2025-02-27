namespace TrevizaniRoleplay.Core.Models.Responses;

public class StafferResponse
{
    public string Staff { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int HelpRequestsAnswersQuantity { get; set; }
    public int CharacterApplicationsQuantity { get; set; }
    public int StaffDutyTime { get; set; }
    public int ConnectedTime { get; set; }
    public IEnumerable<string> Flags { get; set; } = [];
    public DateTime LastAccessDate { get; set; }
}