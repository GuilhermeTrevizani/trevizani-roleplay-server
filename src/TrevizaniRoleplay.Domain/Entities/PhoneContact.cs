namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneContact : BaseEntity
{
    public uint Origin { get; private set; }
    public uint Number { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool Favorite { get; private set; }
    public bool Blocked { get; private set; }

    public void Create(uint origin, uint number, string name)
    {
        Origin = origin;
        Number = number;
        Name = name;
    }

    public void Update(string name, bool favorite, bool blocked)
    {
        Name = name;
        Favorite = favorite;
        Blocked = blocked;
    }
}