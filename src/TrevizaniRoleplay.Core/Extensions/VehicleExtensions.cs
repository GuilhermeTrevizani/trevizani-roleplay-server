using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Core.Extensions;

public static class VehicleExtensions
{
    public static string GetInsuranceInfo(this Vehicle vehicle)
    {
        var insurance = Resources.No;
        if (vehicle.ExemptInsurance)
            insurance = Resources.Exempt;
        else if (vehicle.InsuranceDate > DateTime.Now)
            insurance = $"{Resources.Until} {vehicle.InsuranceDate}";
        return insurance;
    }
}