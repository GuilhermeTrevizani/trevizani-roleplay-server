using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Wanted : BaseEntity
{
    public Guid PoliceOfficerCharacterId { get; private set; }
    public Guid? WantedCharacterId { get; private set; }
    public Guid? WantedVehicleId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime? DeletedDate { get; private set; }
    public Guid? PoliceOfficerDeletedCharacterId { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerCharacter { get; private set; }

    [JsonIgnore]
    public Character? WantedCharacter { get; private set; }

    [JsonIgnore]
    public Vehicle? WantedVehicle { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerDeletedCharacter { get; private set; }

    public void CreateByCharacter(Guid policeOfficerCharacterId, Guid wantedCharacterId, string reason)
    {
        PoliceOfficerCharacterId = policeOfficerCharacterId;
        WantedCharacterId = wantedCharacterId;
        Reason = reason;
    }

    public void CreateByVehicle(Guid policeOfficerCharacterId, Guid wantedVehicleId, string reason)
    {
        PoliceOfficerCharacterId = policeOfficerCharacterId;
        WantedVehicleId = wantedVehicleId;
        Reason = reason;
    }

    public void Delete(Guid policeOfficerDeletedCharacterId)
    {
        PoliceOfficerDeletedCharacterId = policeOfficerDeletedCharacterId;
        DeletedDate = DateTime.Now;
    }
}