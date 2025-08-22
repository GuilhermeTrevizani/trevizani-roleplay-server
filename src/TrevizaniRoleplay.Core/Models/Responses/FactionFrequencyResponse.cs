namespace TrevizaniRoleplay.Core.Models.Responses;

public class FactionFrequencyResponse
{
    public Guid Id { get; set; }
    public int Frequency { get; set; }
    public string Name { get; set; } = default!;
}