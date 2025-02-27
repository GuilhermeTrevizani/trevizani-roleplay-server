using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class PhoneMessage : BaseEntity
{
    public uint Origin { get; private set; }
    public uint? Number { get; private set; }
    public Guid? PhoneGroupId { get; private set; }
    public PhoneMessageType Type { get; private set; }
    public string? Message { get; private set; }
    public float? LocationX { get; private set; }
    public float? LocationY { get; private set; }

    public PhoneGroup? PhoneGroup { get; private set; }
    public ICollection<PhoneMessageRead>? Reads { get; private set; }

    public void CreateTextToContact(uint origin, uint number, string message)
    {
        Type = PhoneMessageType.Text;
        Origin = origin;
        Number = number;
        Message = message;
    }

    public void CreateTextToGroup(uint origin, Guid phoneGroupId, string message)
    {
        Type = PhoneMessageType.Text;
        Origin = origin;
        PhoneGroupId = phoneGroupId;
        Message = message;
    }

    public void CreateLocationToContact(uint origin, uint number, float locationX, float locationY)
    {
        Type = PhoneMessageType.Location;
        Origin = origin;
        Number = number;
        LocationX = locationX;
        LocationY = locationY;
    }

    public void CreateLocationToGroup(uint origin, Guid phoneGroupId, float locationX, float locationY)
    {
        Type = PhoneMessageType.Location;
        Origin = origin;
        PhoneGroupId = phoneGroupId;
        LocationX = locationX;
        LocationY = locationY;
    }
}