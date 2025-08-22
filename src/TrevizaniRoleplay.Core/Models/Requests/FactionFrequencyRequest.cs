namespace TrevizaniRoleplay.Core.Models.Requests;

public class FactionFrequencyRequest
{
    public Guid? Id { get; set; }
    public Guid FactionId { get; set; }
    public int Frequency { get; set; }
    public string Name { get; set; } = default!;
}