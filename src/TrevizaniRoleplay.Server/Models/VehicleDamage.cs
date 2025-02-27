namespace TrevizaniRoleplay.Server.Models;

public class VehicleDamage
{
    public DateTime Date { get; set; } = DateTime.Now;
    public float BodyHealthDamage { get; set; }
    public float EngineHealthDamage { get; set; }
    public string Weapon { get; set; } = string.Empty;
    public string? Attacker { get; set; }
    public float Distance { get; set; }
}