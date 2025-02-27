using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class FactionRank : BaseEntity
{
    public Guid FactionId { get; private set; }
    public int Position { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Salary { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid factionId, int position, string name, int salary)
    {
        FactionId = factionId;
        Position = position;
        Name = name;
        Salary = salary;
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void Update(int salary)
    {
        Salary = salary;
    }

    public void SetPosition(int position)
    {
        Position = position;
    }
}