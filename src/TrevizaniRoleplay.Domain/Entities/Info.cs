using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class Info : BaseEntity
{
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public uint Dimension { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool Image { get; private set; }
    public Guid CharacterId { get; private set; }

    [JsonIgnore]
    public Character? Character { get; set; }

    public void Create(float posX, float posY, float posZ, uint dimension, int days, Guid characterId, string message, bool image)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Dimension = dimension;
        ExpirationDate = DateTime.Now.AddDays(days);
        CharacterId = characterId;
        Message = message;
        Image = image;
    }
}