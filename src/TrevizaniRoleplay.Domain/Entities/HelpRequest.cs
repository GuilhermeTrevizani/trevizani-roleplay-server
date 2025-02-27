using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class HelpRequest : BaseEntity
{
    public string Message { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime? AnswerDate { get; private set; }
    public Guid? StaffUserId { get; private set; }
    public HelpRequestType Type { get; private set; }

    [JsonIgnore]
    public User? User { get; private set; }

    [JsonIgnore]
    public User? StaffUser { get; private set; }

    [NotMapped]
    public int CharacterSessionId { get; private set; }

    [NotMapped]
    public string CharacterName { get; private set; } = string.Empty;

    [NotMapped]
    public string UserName { get; private set; } = string.Empty;

    public void Create(string message, Guid userId, int characterSessionId, string characterName, string userName, HelpRequestType type)
    {
        Message = message;
        UserId = userId;
        CharacterSessionId = characterSessionId;
        CharacterName = characterName;
        UserName = userName;
        Type = type;
    }

    public void Answer(Guid? staffUserId)
    {
        AnswerDate = DateTime.Now;
        StaffUserId = staffUserId;
    }

    public void SetType(HelpRequestType type)
    {
        Type = type;
    }
}