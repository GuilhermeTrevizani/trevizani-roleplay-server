namespace TrevizaniRoleplay.Domain.Entities;

public class CharacterVehicle : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid VehicleId { get; private set; }

    public Character? Character { get; private set; }
    public Vehicle? Vehicle { get; private set; }

    public void Create(Guid characterId, Guid vehicleId)
    {
        CharacterId = characterId;
        VehicleId = vehicleId;
    }
}