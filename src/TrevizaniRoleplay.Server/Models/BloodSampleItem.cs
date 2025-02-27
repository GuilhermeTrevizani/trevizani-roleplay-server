using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Server.Models;

public class BloodSampleItem
{
    public DateTime Date { get; set; } = DateTime.Now;
    public CharacterBloodType BloodType { get; set; }
    public Guid CharacterId { get; set; }
}