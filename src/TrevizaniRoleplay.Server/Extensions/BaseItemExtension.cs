using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Extensions;

public static class BaseItemExtension
{
    public static ItemTemplate GetItemTemplate(this BaseItem baseItem)
    {
        return Global.ItemsTemplates.FirstOrDefault(x => x.Id == baseItem.ItemTemplateId)!;
    }

    public static string GetName(this BaseItem baseItem)
    {
        return GetItemTemplate(baseItem).Name;
    }

    public static float GetWeight(this BaseItem baseItem)
    {
        return GetItemTemplate(baseItem).Weight;
    }

    public static string GetObjectName(this BaseItem baseItem)
    {
        return GetItemTemplate(baseItem).ObjectModel;
    }

    public static string GetImage(this BaseItem baseItem)
    {
        return GetItemTemplate(baseItem).Image;
    }

    public static ItemCategory GetCategory(this BaseItem baseItem)
    {
        return GetItemTemplate(baseItem).Category;
    }

    public static uint GetItemType(this BaseItem baseItem)
    {
        return GetItemTemplate(baseItem).Type;
    }

    public static string GetExtra(this BaseItem baseItem)
    {
        if (string.IsNullOrWhiteSpace(baseItem.Extra))
            return string.Empty;

        var category = baseItem.GetItemTemplate().Category;
        if (category == ItemCategory.Weapon)
        {
            var extra = Functions.Deserialize<WeaponItem>(baseItem.Extra);
            if (extra.SerialNumber is null)
                return string.Empty;

            return $"Número de Série: <strong>{extra.SerialNumber}</strong>";
        }

        if (category == ItemCategory.PropertyKey || category == ItemCategory.VehicleKey)
            return $"Fechadura: <strong>{baseItem.Subtype}</strong>";

        if (category == ItemCategory.WalkieTalkie)
        {
            var extra = Functions.Deserialize<WalkieTalkieItem>(baseItem.Extra);
            return $"Canal 1: <strong>{extra.Channel1}</strong><br/>Canal 2: <strong>{extra.Channel2}</strong><br/>Canal 3: <strong>{extra.Channel3}</strong><br/>Canal 4: <strong>{extra.Channel4}</strong><br/>Canal 5: <strong>{extra.Channel5}</strong>";
        }

        if (category == ItemCategory.Cellphone)
            return $"Número: <strong>{baseItem.Subtype}</strong>";

        if (Functions.CheckIfIsBulletShell(category))
            return $"Temperatura: <strong>{Functions.GetBulletShellTemperature(baseItem.Extra)}</strong>";

        if (category == ItemCategory.WeaponComponent)
        {
            var extra = Functions.Deserialize<WeaponComponentItem>(baseItem.Extra);
            if (extra.SerialNumber is null)
                return string.Empty;

            return $"Número de Série: <strong>{extra.SerialNumber}</strong>";
        }

        return string.Empty;
    }

    public static bool GetIsStack(this BaseItem baseItem)
    {
        var category = baseItem.GetCategory();
        return category == ItemCategory.Money || category == ItemCategory.Drug
            || category == ItemCategory.VehiclePart || category == ItemCategory.PickLock
            || category == ItemCategory.Food || category == ItemCategory.Display
            || category == ItemCategory.Bandage || Functions.CheckIfIsAmmo(category);
    }

    public static bool GetIsUsable(this BaseItem baseItem)
    {
        var category = baseItem.GetCategory();
        return category == ItemCategory.Drug || category == ItemCategory.Weapon
            || category == ItemCategory.WalkieTalkie || category == ItemCategory.Cellphone
            || category == ItemCategory.Food || category == ItemCategory.FishingRod
            || category == ItemCategory.Bandage || Functions.CheckIfIsAmmo(category)
            || category == ItemCategory.WeaponComponent;
    }
}