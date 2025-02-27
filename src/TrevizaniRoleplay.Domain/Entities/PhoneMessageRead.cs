namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneMessageRead : BaseEntity
{
    public Guid PhoneMessageId { get; private set; }
    public uint Number { get; private set; }

    public PhoneMessage? PhoneMessage { get; private set; }

    public void Create(Guid phoneMessageId, uint number)
    {
        PhoneMessageId = phoneMessageId;
        Number = number;
    }
}