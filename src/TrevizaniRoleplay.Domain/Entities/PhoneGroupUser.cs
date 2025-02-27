using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneGroupUser : BaseEntity
{
    public Guid PhoneGroupId { get; private set; }
    public uint Number { get; private set; }
    public PhoneGroupUserPermission Permission { get; private set; } = PhoneGroupUserPermission.User;

    public PhoneGroup? PhoneGroup { get; private set; }

    public void Create(uint number, PhoneGroupUserPermission permission)
    {
        Number = number;
        Permission = permission;
    }

    public void Create(Guid phoneGroupId, uint number, PhoneGroupUserPermission permission)
    {
        PhoneGroupId = phoneGroupId;
        Number = number;
        Permission = permission;
    }

    public void SetPermission(PhoneGroupUserPermission permission)
    {
        Permission = permission;
    }
}