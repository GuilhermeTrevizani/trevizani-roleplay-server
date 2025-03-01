namespace TrevizaniRoleplay.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime RegisterDate { get; private set; } = DateTime.Now;
}