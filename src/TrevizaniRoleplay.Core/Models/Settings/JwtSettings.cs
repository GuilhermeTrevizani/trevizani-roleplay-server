namespace TrevizaniRoleplay.Core.Models.Settings;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public int DaysToExpire { get; set; }
}