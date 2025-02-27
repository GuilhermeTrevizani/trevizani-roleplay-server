namespace TrevizaniRoleplay.Server.Models;

public class DrugRequest
{
    public Guid Id { get; set; }
    public byte ThresoldDeath { get; set; }
    public int Health { get; set; }
    public float GarbageCollectorMultiplier { get; set; }
    public float TruckerMultiplier { get; set; }
    public int MinutesDuration { get; set; }
    public string Warn { get; set; } = string.Empty;
    public string ShakeGameplayCamName { get; set; } = string.Empty;
    public float ShakeGameplayCamIntensity { get; set; }
    public string TimecycModifier { get; set; } = string.Empty;
    public string AnimpostFXName { get; set; } = string.Empty;
}