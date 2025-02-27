namespace TrevizaniRoleplay.Server.Models;

public class AttachedObject
{
    public required string Model { get; set; }
    public required int BoneId { get; set; }
    public required float PosX { get; set; }
    public required float PosY { get; set; }
    public required float PosZ { get; set; }
    public required float RotX { get; set; }
    public required float RotY { get; set; }
    public required float RotZ { get; set; }
}