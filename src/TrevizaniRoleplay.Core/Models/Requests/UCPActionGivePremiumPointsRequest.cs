namespace TrevizaniRoleplay.Core.Models.Requests;

public class UCPActionGivePremiumPointsRequest
{
    public Guid PremiumPointPurchaseId { get; set; }
    public int Quantity { get; set; }
}