using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class PremiumPointPurchase : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int Value { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string PreferenceId { get; private set; } = string.Empty;
    public DateTime? PaymentDate { get; private set; }
    public Guid TargetUserId { get; private set; }

    [JsonIgnore]
    public User? User { get; private set; }

    [JsonIgnore]
    public User? TargetUser { get; private set; }

    public void Create(Guid userId, string name, int quantity, int value, string preferenceId, Guid targetUserId)
    {
        UserId = userId;
        Name = name;
        Quantity = quantity;
        Value = value;
        PreferenceId = preferenceId;
        TargetUserId = targetUserId;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        if (PaymentDate is null && status.ToLower() == "approved")
            PaymentDate = DateTime.Now;
    }
}