namespace TrevizaniRoleplay.Server.Models;

public class CellphoneItem
{
    public bool FlightMode { get; set; }
    public string Wallpaper { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RingtoneVolume { get; set; } = 100;
    public int Scale { get; set; } = 50;
}