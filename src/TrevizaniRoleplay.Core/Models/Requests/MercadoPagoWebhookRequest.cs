namespace TrevizaniRoleplay.Core.Models.Requests;

public class MercadoPagoWebhookRequest
{
    public long Id { get; set; }
    public string Topic { get; set; } = string.Empty;
}