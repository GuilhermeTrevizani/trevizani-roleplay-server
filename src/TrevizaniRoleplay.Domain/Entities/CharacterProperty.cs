namespace TrevizaniRoleplay.Domain.Entities;

public class CharacterProperty : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid PropertyId { get; private set; }

    public Character? Character { get; private set; }
    public Property? Property { get; private set; }

    public void Create(Guid characterId, Guid propertyId)
    {
        CharacterId = characterId;
        PropertyId = propertyId;
    }
}