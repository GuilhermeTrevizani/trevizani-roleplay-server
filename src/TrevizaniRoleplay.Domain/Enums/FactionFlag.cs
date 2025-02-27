using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum FactionFlag : byte
{
    [Display(Name = Globalization.INVITE_MEMBER)]
    InviteMember = 1,

    [Display(Name = Globalization.EDIT_MEMBER)]
    EditMember = 2,

    [Display(Name = Globalization.REMOVE_MEMBER)]
    RemoveMember = 3,

    [Display(Name = Globalization.BLOCK_CHAT)]
    BlockChat = 4,

    [Display(Name = Globalization.GOVERNMENT_ADVERTISEMENT)]
    GovernmentAdvertisement = 5,

    [Display(Name = Globalization.HQ)]
    HQ = 6,

    [Display(Name = Globalization.STORAGE)]
    Storage = 7,

    [Display(Name = Globalization.REMOVE_ALL_BARRIERS)]
    RemoveAllBarriers = 8,

    [Display(Name = Globalization.UNIFORM)]
    Uniform = 9,

    [Display(Name = Globalization.SWAT)]
    SWAT = 10,

    [Display(Name = Globalization.UPR)]
    UPR = 11,

    [Display(Name = Globalization.WEAPON_LICENSE)]
    WeaponLicense = 12,

    [Display(Name = Globalization.FIRE_MANAGER)]
    FireManager = 13,

    [Display(Name = Globalization.RESPAWN_VEHICLES)]
    RespawnVehicles = 14,

    [Display(Name = Globalization.MANAGE_VEHICLES)]
    ManageVehicles = 15,

    [Display(Name = Globalization.FURNITURES)]
    Furnitures = 16,

    [Display(Name = Globalization.CLOSE_UNIT)]
    CloseUnit = 17,
}