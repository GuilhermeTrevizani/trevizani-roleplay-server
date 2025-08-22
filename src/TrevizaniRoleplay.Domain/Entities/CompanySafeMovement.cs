using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class CompanySafeMovement : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid CharacterId { get; private set; }
    public FinancialTransactionType Type { get; private set; }
    public uint Value { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public Company? Company { get; private set; }
    public Character? Character { get; private set; }

    public void Create(Guid companyId, Guid characterId, FinancialTransactionType type, uint value, string description)
    {
        CompanyId = companyId;
        CharacterId = characterId;
        Type = type;
        Value = value;
        Description = description;
    }
}