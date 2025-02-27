using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneCall : BaseEntity
{
    public uint Origin { get; private set; }
    public uint Number { get; private set; }
    public DateTime? EndDate { get; private set; }
    public PhoneCallType Type { get; private set; } = PhoneCallType.Lost;

    public void Create(uint origin, uint number)
    {
        Origin = origin;
        Number = number;
    }

    public void SetType(PhoneCallType type)
    {
        Type = type;
    }

    public void End()
    {
        EndDate = DateTime.Now;
    }
}