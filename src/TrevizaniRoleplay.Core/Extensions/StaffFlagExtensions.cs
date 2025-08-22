using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class StaffFlagExtensions
{
    public static string GetDescription(this StaffFlag staffFlag)
    {
        return staffFlag switch
        {
            StaffFlag.Events => Globalization.Resources.Events,
            StaffFlag.Doors => Globalization.Resources.Doors,
            StaffFlag.Factions => Globalization.Resources.Factions,
            StaffFlag.FactionsStorages => Globalization.Resources.FactionsStorages,
            StaffFlag.Properties => Globalization.Resources.Properties,
            StaffFlag.Spots => Globalization.Resources.Spots,
            StaffFlag.Blips => Globalization.Resources.Blips,
            StaffFlag.CK => Globalization.Resources.CK,
            StaffFlag.GiveItem => Globalization.Resources.GiveItem,
            StaffFlag.CrackDens => Globalization.Resources.CrackDens,
            StaffFlag.TruckerLocations => Globalization.Resources.TruckerLocations,
            StaffFlag.Furnitures => Globalization.Resources.Furnitures,
            StaffFlag.Animations => Globalization.Resources.Animations,
            StaffFlag.Companies => Globalization.Resources.Companies,
            StaffFlag.Items => Globalization.Resources.Items,
            StaffFlag.VehicleMaintenance => Globalization.Resources.VehicleMaintenance,
            StaffFlag.Drugs => Globalization.Resources.Drugs,
            _ => staffFlag.ToString()
        };
    }
}