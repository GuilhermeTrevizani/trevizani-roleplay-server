using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Services;

public static class GlobalFunctions
{
    public static bool CheckIfVehicleExists(string model)
    {
        return Enum.TryParse(model, true, out VehicleModel _)
            || Enum.TryParse(model, true, out VehicleModelMods _);
    }

    public static bool CheckIfIsAmmo(ItemCategory itemCategory)
    {
        return itemCategory == ItemCategory.PistolAmmo || itemCategory == ItemCategory.ShotgunAmmo
            || itemCategory == ItemCategory.AssaultRifleAmmo || itemCategory == ItemCategory.LightMachineGunAmmo
            || itemCategory == ItemCategory.SniperRifleAmmo || itemCategory == ItemCategory.SubMachineGunAmmo;
    }

    public static bool CheckIfIsBulletShell(ItemCategory itemCategory)
    {
        return itemCategory == ItemCategory.PistolBulletShell
            || itemCategory == ItemCategory.ShotgunBulletShell || itemCategory == ItemCategory.AssaultRifleBulletShell
            || itemCategory == ItemCategory.LightMachineGunBulletShell || itemCategory == ItemCategory.SniperRifleBulletShell
            || itemCategory == ItemCategory.SubMachineGunBulletShell;
    }

    public static bool CheckIfIsStack(ItemCategory itemCategory)
    {
        return itemCategory == ItemCategory.Money || itemCategory == ItemCategory.Drug
            || itemCategory == ItemCategory.VehiclePart || itemCategory == ItemCategory.PickLock
            || itemCategory == ItemCategory.Food || itemCategory == ItemCategory.Display
            || itemCategory == ItemCategory.Bandage || CheckIfIsAmmo(itemCategory);
    }

    public static bool IsValidImageUrl(string url)
    {
        return url.StartsWith("https://i.imgur.com/");
    }

    public static string GetWeaponName(uint type)
    {
        return ((WeaponModel)type).ToString();
    }

    public static uint GetWeaponType(string name)
    {
        Enum.TryParse(name, true, out WeaponModel type);
        return (uint)type;
    }

    public static bool CheckIfWeaponExists(string model)
    {
        return Enum.TryParse(model, true, out WeaponModel _);
    }
}