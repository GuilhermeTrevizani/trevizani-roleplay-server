using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class DealershipVehicle : BaseEntity
{
    public Guid DealershipId { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public int Value { get; private set; }

    [JsonIgnore]
    public Dealership? Dealership { get; private set; }

    public void Create(Guid dealershipId, string model, int value)
    {
        DealershipId = dealershipId;
        Model = model;
        Value = value;
    }

    public void Update(string model, int value)
    {
        Model = model;
        Value = value;
    }
}