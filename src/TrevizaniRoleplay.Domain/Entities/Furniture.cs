namespace TrevizaniRoleplay.Domain.Entities;

public class Furniture : BaseEntity
{
    public string Category { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Value { get; private set; }
    public bool Door { get; private set; }
    public bool AudioOutput { get; private set; }
    public string TVTexture { get; private set; } = string.Empty;
    public string Subcategory { get; private set; } = string.Empty;
    public bool UseSlot { get; private set; }

    public void Create(string category, string name, string model, int value, bool door, bool audioOutput,
        string tvTexture, string subcategory, bool useSlot)
    {
        Category = category;
        Name = name;
        Model = model;
        Value = value;
        Door = door;
        AudioOutput = audioOutput;
        TVTexture = tvTexture;
        Subcategory = subcategory;
        UseSlot = useSlot;
    }

    public void Update(string category, string name, string model, int value, bool door, bool audioOutput,
        string tvTexture, string subcategory, bool useSlot)
    {
        Category = category;
        Name = name;
        Model = model;
        Value = value;
        Door = door;
        AudioOutput = audioOutput;
        TVTexture = tvTexture;
        Subcategory = subcategory;
        UseSlot = useSlot;
    }
}