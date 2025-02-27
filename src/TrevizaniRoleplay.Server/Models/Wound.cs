namespace TrevizaniRoleplay.Server.Models;

public class Wound
{
    public DateTime Date { get; set; } = DateTime.Now;
    public int Damage { get; set; }
    public string Weapon { get; set; } = string.Empty;
    public string? Attacker { get; set; }
    public string BodyPart { get; set; } = string.Empty;
    public float Distance { get; set; }
}