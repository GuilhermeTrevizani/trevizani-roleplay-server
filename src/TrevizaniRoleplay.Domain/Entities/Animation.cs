namespace TrevizaniRoleplay.Domain.Entities;

public class Animation : BaseEntity
{
    public string Display { get; private set; } = string.Empty;
    public string Dictionary { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Flag { get; private set; }
    public bool OnlyInVehicle { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Scenario { get; private set; } = string.Empty;

    public void Create(string display, string dictionary, string name, int flag,
        bool onlyInVehicle, string category, string scenario)
    {
        Display = display;
        Dictionary = dictionary;
        Name = name;
        Flag = flag;
        OnlyInVehicle = onlyInVehicle;
        Category = category;
        Scenario = scenario;
    }

    public void Update(string display, string dictionary, string name, int flag,
        bool onlyInVehicle, string category, string scenario)
    {
        Display = display;
        Dictionary = dictionary;
        Name = name;
        Flag = flag;
        OnlyInVehicle = onlyInVehicle;
        Category = category;
        Scenario = scenario;
    }
}