using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Drug : BaseEntity
{
    public Guid ItemTemplateId { get; private set; }
    public byte ThresoldDeath { get; private set; }
    public int Health { get; private set; }
    public float GarbageCollectorMultiplier { get; private set; }
    public float TruckerMultiplier { get; private set; }
    public int MinutesDuration { get; private set; }
    public string Warn { get; private set; } = string.Empty;
    public string ShakeGameplayCamName { get; private set; } = string.Empty;
    public float ShakeGameplayCamIntensity { get; private set; }
    public string TimecycModifier { get; private set; } = string.Empty;
    public string AnimpostFXName { get; private set; } = string.Empty;

    [JsonIgnore]
    public ItemTemplate? ItemTemplate { get; private set; }

    public void Create(Guid itemTemplateId)
    {
        ItemTemplateId = itemTemplateId;
    }

    public void Update(byte thresoldDeath, int health, float garbageCollectorMultiplier, float truckerMultiplier,
        int minutesDuration, string warn, string shakeGameplayCamName, float shakeGameplayCamIntensity, string timecycModifier,
        string animpostFXName)
    {
        ThresoldDeath = thresoldDeath;
        Health = health;
        GarbageCollectorMultiplier = garbageCollectorMultiplier;
        TruckerMultiplier = truckerMultiplier;
        MinutesDuration = minutesDuration;
        Warn = warn;
        ShakeGameplayCamName = shakeGameplayCamName;
        ShakeGameplayCamIntensity = shakeGameplayCamIntensity;
        TimecycModifier = timecycModifier;
        AnimpostFXName = animpostFXName;
    }
}