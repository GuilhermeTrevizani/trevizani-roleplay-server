using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class ItemCategoryExtensions
{
    public static string GetDescription(this ItemCategory itemCategory)
    {
        return itemCategory switch
        {
            ItemCategory.Weapon => Resources.Weapon,
            ItemCategory.Money => Resources.Money,
            ItemCategory.Boombox => Resources.Boombox,
            ItemCategory.WalkieTalkie => Resources.WalkieTalkie,
            ItemCategory.Cellphone => Resources.Cellphone,
            ItemCategory.Drug => Resources.Drug,
            ItemCategory.Microphone => Resources.Microphone,
            ItemCategory.VehiclePart => Resources.VehiclePart,
            ItemCategory.PickLock => Resources.PickLock,
            ItemCategory.Food => Resources.Food,
            ItemCategory.Display => Resources.Display,
            ItemCategory.FishingRod => Resources.FishingRod,
            ItemCategory.BloodSample => Resources.BloodSample,
            ItemCategory.Bandage => Resources.Bandage,
            ItemCategory.WeaponComponent => Resources.WeaponComponent,
            ItemCategory.PistolAmmo => Resources.PistolAmmo,
            ItemCategory.ShotgunAmmo => Resources.ShotgunAmmo,
            ItemCategory.AssaultRifleAmmo => Resources.AssaultRifleAmmo,
            ItemCategory.LightMachineGunAmmo => Resources.LightMachineGunAmmo,
            ItemCategory.SniperRifleAmmo => Resources.SniperRifleAmmo,
            ItemCategory.SubMachineGunAmmo => Resources.SubMachineGunAmmo,
            ItemCategory.PistolBulletShell => Resources.PistolBulletShell,
            ItemCategory.ShotgunBulletShell => Resources.ShotgunBulletShell,
            ItemCategory.AssaultRifleBulletShell => Resources.AssaultRifleBulletShell,
            ItemCategory.LightMachineGunBulletShell => Resources.LightMachineGunBulletShell,
            ItemCategory.SniperRifleBulletShell => Resources.SniperRifleBulletShell,
            ItemCategory.SubMachineGunBulletShell => Resources.SubMachineGunBulletShell,
            _ => itemCategory.ToString(),
        };
    }
}