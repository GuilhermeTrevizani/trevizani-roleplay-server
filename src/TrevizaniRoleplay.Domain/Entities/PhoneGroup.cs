namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneGroup : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public ICollection<PhoneGroupUser>? Users { get; private set; }

    public void Create(string name, ICollection<PhoneGroupUser> users)
    {
        Name = name;
        Users = users;
    }
}