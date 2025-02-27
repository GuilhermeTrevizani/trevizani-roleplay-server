using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Door : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public long Hash { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public Guid? FactionId { get; private set; }
    public bool Locked { get; private set; } = true;
    public Guid? CompanyId { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public Company? Company { get; private set; }

    public void Create(string name, long hash, float posX, float posY, float posZ, Guid? factionId, Guid? companyId, bool locked)
    {
        Name = name;
        Hash = hash;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        FactionId = factionId;
        CompanyId = companyId;
        Locked = locked;
    }

    public void Update(string name, long hash, float posX, float posY, float posZ, Guid? factionId, Guid? companyId, bool locked)
    {
        Name = name;
        Hash = hash;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        FactionId = factionId;
        CompanyId = companyId;
        Locked = locked;
    }

    public void SetLocked(bool locked)
    {
        Locked = locked;
    }
}