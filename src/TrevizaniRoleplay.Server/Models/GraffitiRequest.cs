using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Server.Models;

public class GraffitiRequest
{
    public string Text { get; set; } = string.Empty;
    public int Size { get; set; }
    public GraffitiFont Font { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotR { get; set; }
    public float RotP { get; set; }
    public float RotY { get; set; }
    public byte ColorR { get; set; }
    public byte ColorG { get; set; }
    public byte ColorB { get; set; }
    public byte ColorA { get; set; }
}