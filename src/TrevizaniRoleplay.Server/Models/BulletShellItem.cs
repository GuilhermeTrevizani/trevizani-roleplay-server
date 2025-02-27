namespace TrevizaniRoleplay.Server.Models;

public class BulletShellItem
{
    public DateTime Date { get; set; } = DateTime.Now;
    public Guid WeaponItemId { get; set; } = Guid.Empty;
}