using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Graffiti : BaseEntity
{
    public string Text { get; private set; } = string.Empty;
    public int Size { get; private set; }
    public GraffitiFont Font { get; private set; }
    public Guid CharacterId { get; private set; }
    public uint Dimension { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public float RotR { get; private set; }
    public float RotP { get; private set; }
    public float RotY { get; private set; }
    public byte ColorR { get; private set; }
    public byte ColorG { get; private set; }
    public byte ColorB { get; private set; }
    public byte ColorA { get; private set; }
    public DateTime ExpirationDate { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    public void Create(Guid characterId, string text, int size, GraffitiFont font,
        uint dimension, float posX, float posY, float posZ, float rotR, float rotP, float rotY,
        byte colorR, byte colorG, byte colorB, byte colorA, int days
        )
    {
        CharacterId = characterId;
        Text = text;
        Size = size;
        Font = font;
        Dimension = dimension;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        RotR = rotR;
        RotP = rotP;
        RotY = rotY;
        ColorR = colorR;
        ColorG = colorG;
        ColorB = colorB;
        ColorA = colorA;
        ExpirationDate = DateTime.Now.AddDays(days);
    }
}