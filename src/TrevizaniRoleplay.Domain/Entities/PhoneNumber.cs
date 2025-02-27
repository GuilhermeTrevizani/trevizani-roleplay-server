namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneNumber : BaseEntity
{
    public uint Number { get; private set; }

    public void Create(uint number)
    {
        Number = number;
    }
}