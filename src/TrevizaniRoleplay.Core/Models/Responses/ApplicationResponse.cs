namespace TrevizaniRoleplay.Core.Models.Responses;

public class ApplicationResponse
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
}