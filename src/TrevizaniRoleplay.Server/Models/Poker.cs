namespace TrevizaniRoleplay.Server.Models;

public class Poker
{
    public Guid OwnerCharacterId { get; set; }
    public int InitialValue { get; set; }
    public int SmallBlind { get; set; }
}