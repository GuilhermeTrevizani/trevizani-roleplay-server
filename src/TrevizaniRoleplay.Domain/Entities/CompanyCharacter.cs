using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class CompanyCharacter : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid CharacterId { get; private set; }
    public string FlagsJSON { get; private set; } = "[]";

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public Company? Company { get; private set; }

    public void Create(Guid companyId, Guid characterId)
    {
        CompanyId = companyId;
        CharacterId = characterId;
    }

    public void SetFlagsJSON(string flagsJSON)
    {
        FlagsJSON = flagsJSON;
    }
}