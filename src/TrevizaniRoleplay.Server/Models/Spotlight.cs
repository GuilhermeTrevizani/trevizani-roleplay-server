using GTANetworkAPI;

namespace TrevizaniRoleplay.Server.Models;

public class Spotlight
{
    public uint Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public int Player { get; set; }
    public float Distance { get; set; }
    public float Brightness { get; set; }
    public float Hardness { get; set; }
    public float Radius { get; set; }
    public float Falloff { get; set; }
}