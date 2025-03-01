using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class FactionFlagExtensions
{
    public static string GetDescription(this FactionFlag factionFlag)
    {
        return factionFlag switch
        {
            FactionFlag.InviteMember => Resources.InviteMember,
            FactionFlag.EditMember => Resources.EditMember,
            FactionFlag.RemoveMember => Resources.RemoveMember,
            FactionFlag.BlockChat => Resources.BlockChat,
            FactionFlag.GovernmentAdvertisement => Resources.GovernmentAdvertisement,
            FactionFlag.HQ => Resources.HQ,
            FactionFlag.Storage => Resources.Storage,
            FactionFlag.RemoveAllBarriers => Resources.RemoveAllBarriers,
            FactionFlag.Uniform => Resources.Uniform,
            FactionFlag.SWAT => Resources.SWAT,
            FactionFlag.UPR => Resources.UPR,
            FactionFlag.WeaponLicense => Resources.WeaponLicense,
            FactionFlag.FireManager => Resources.FireManager,
            FactionFlag.RespawnVehicles => Resources.RespawnVehicles,
            FactionFlag.ManageVehicles => Resources.ManageVehicles,
            FactionFlag.Furnitures => Resources.Furnitures,
            FactionFlag.CloseUnit => Resources.CloseUnit,
            _ => factionFlag.ToString(),
        };
    }
}