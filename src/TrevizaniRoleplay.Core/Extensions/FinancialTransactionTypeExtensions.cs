using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class FinancialTransactionTypeExtensions
{
    public static string GetDescription(this FinancialTransactionType factionType)
    {
        return factionType switch
        {
            FinancialTransactionType.Deposit => Resources.Deposit,
            FinancialTransactionType.Withdraw => Resources.Withdraw,
            _ => factionType.ToString(),
        };
    }
}