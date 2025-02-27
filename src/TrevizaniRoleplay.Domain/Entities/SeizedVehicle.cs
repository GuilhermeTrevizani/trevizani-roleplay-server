using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class SeizedVehicle : BaseEntity
{
    public Guid VehicleId { get; private set; }
    public Guid PoliceOfficerCharacterId { get; private set; }
    public int Value { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime? PaymentDate { get; private set; }
    public Guid FactionId { get; private set; }
    public string? Description { get; private set; }
    public DateTime? DescriptionDate { get; private set; }
    public DateTime EndDate { get; private set; }

    [JsonIgnore]
    public Vehicle? Vehicle { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerCharacter { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    public void Create(Guid vehicleId, Guid policeOfficerCharacterId, int value, string reason, Guid factionId, DateTime endDate)
    {
        VehicleId = vehicleId;
        PoliceOfficerCharacterId = policeOfficerCharacterId;
        Value = value;
        Reason = reason;
        FactionId = factionId;
        EndDate = endDate;
    }

    public void Pay()
    {
        PaymentDate = DateTime.Now;
    }

    public void SetDescription(string description)
    {
        Description = description;
        DescriptionDate = DateTime.Now;
    }
}