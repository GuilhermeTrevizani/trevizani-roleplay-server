namespace TrevizaniRoleplay.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; internal set; } = Guid.NewGuid();
    public DateTime RegisterDate { get; private set; } = DateTime.Now;
}