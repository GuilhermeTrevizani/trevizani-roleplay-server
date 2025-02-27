namespace TrevizaniRoleplay.Core.Models.Requests;

public class CreatePremiumRequest
{
    public int Quantity { get; set; }
    public string UserName { get; set; } = string.Empty;
}