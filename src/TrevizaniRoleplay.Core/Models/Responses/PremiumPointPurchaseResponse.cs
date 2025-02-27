namespace TrevizaniRoleplay.Core.Models.Responses;

public class PremiumPointPurchaseResponse
{
    public string UserOrigin { get; set; } = string.Empty;
    public string UserTarget { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Value { get; set; }
    public DateTime RegisterDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
}