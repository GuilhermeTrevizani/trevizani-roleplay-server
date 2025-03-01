using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class CompanyTypeExtensions
{
    public static string GetDescription(this CompanyType companyType)
    {
        return companyType switch
        {
            CompanyType.ConvenienceStore => Resources.ConvenienceStore,
            CompanyType.MechanicWorkshop => Resources.MechanicWorkshop,
            CompanyType.Other => Resources.Other,
            CompanyType.Fishmonger => Resources.Fishmonger,
            CompanyType.WeaponStore => Resources.WeaponStore,
            _ => companyType.ToString(),
        };
    }
}