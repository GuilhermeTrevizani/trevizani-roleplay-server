namespace TrevizaniRoleplay.Core.Models.Settings;

public class MercadoPagoSettings
{
    public string PublicKey { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string BackUrl { get; set; } = string.Empty;
}