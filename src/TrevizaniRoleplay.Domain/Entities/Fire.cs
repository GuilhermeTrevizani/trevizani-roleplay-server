namespace TrevizaniRoleplay.Domain.Entities;

public class Fire : BaseEntity
{
    public string Description { get; private set; } = string.Empty;
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public uint Dimension { get; private set; }
    public float FireSpanLife { get; private set; }
    public int MaxFireSpan { get; private set; }
    public int SecondsNewFireSpan { get; private set; }
    public float PositionNewFireSpan { get; private set; }
    public float FireSpanDamage { get; private set; }

    public void Create(string description, float posX, float posY, float posZ, uint dimension,
        float fireSpanLife, int maxFireSpan, int secondsNewFireSpan, float positionNewFireSpan, float fireSpanDamage)
    {
        Description = description;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
        FireSpanLife = fireSpanLife;
        MaxFireSpan = maxFireSpan;
        SecondsNewFireSpan = secondsNewFireSpan;
        PositionNewFireSpan = positionNewFireSpan;
        FireSpanDamage = fireSpanDamage;
    }

    public void Update(string description, float posX, float posY, float posZ, uint dimension,
        float fireSpanLife, int maxFireSpan, int secondsNewFireSpan, float positionNewFireSpan, float fireSpanDamage)
    {
        Description = description;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
        FireSpanLife = fireSpanLife;
        MaxFireSpan = maxFireSpan;
        SecondsNewFireSpan = secondsNewFireSpan;
        PositionNewFireSpan = positionNewFireSpan;
        FireSpanDamage = fireSpanDamage;
    }
}