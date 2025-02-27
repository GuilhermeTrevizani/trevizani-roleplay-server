namespace TrevizaniRoleplay.Core.Models.Requests;

public class UCPActionCreateCharacterRequest
{
    public bool SendStaffNotification { get; set; }
    public bool Namechange { get; set; }
    public Guid? OldCharacterId { get; set; }
    public Guid? NewCharacterId { get; set; }
}