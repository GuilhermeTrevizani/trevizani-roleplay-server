using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class CompanyFlagExtensions
{
    public static string GetDescription(this CompanyFlag companyFlag)
    {
        return companyFlag switch
        {
            CompanyFlag.InviteEmployee => Resources.InviteEmployee,
            CompanyFlag.EditEmployee => Resources.EditEmployee,
            CompanyFlag.RemoveEmployee => Resources.RemoveEmployee,
            CompanyFlag.Open => Resources.Open,
            CompanyFlag.ManageItems => Resources.ManageItems,
            CompanyFlag.Safe => Resources.Safe,
            CompanyFlag.Furnitures => Resources.Furnitures,
            _ => companyFlag.ToString(),
        };
    }
}