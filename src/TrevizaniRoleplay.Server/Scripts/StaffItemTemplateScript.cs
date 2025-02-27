using GTANetworkAPI;
using TrevizaniRoleplay.Core.Extesions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffItemTemplateScript : Script
{
    [Command("itens")]
    public static void CMD_itens(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Items))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        player.Emit("StaffItemTemplate:Show", GetCategoriesJson(), GetItemsTemplatesJson());
    }

    [RemoteEvent(nameof(StaffItemTemplateSave))]
    public async Task StaffItemTemplateSave(Player playerParam, string idString, int category, string typeString,
        string name, float weight, string image, string objectModel)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Items))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (!Enum.IsDefined(typeof(ItemCategory), Convert.ToByte(category)))
            {
                player.SendNotification(NotificationType.Error, $"Categoria {category} não existe.");
                return;
            }

            if (name.Length < 1 || name.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Imagem deve ter entre 1 e 50 caracteres.");
                return;
            }

            if (image.Length < 1 || image.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Imagem deve ter entre 1 e 50 caracteres.");
                return;
            }

            objectModel ??= string.Empty;
            if (objectModel.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Objeto deve ter entre até 50 caracteres.");
                return;
            }

            if (!Functions.IsValidImageUrl(image))
            {
                player.SendNotification(NotificationType.Error, "Imagem inválida.");
                return;
            }

            if (weight <= 0)
            {
                player.SendNotification(NotificationType.Error, "Peso deve ser maior que 0.");
                return;
            }

            _ = uint.TryParse(typeString, out uint type);

            var itemCategory = (ItemCategory)category;
            if (itemCategory == ItemCategory.Weapon)
            {
                type = Functions.GetWeaponType(typeString);
                if (type == 0)
                {
                    player.SendNotification(NotificationType.Error, $"Arma {typeString} não existe.");
                    return;
                }
            }
            else if (itemCategory == ItemCategory.Boombox)
            {
                if (type <= 0)
                {
                    player.SendNotification(NotificationType.Error, "Tipo deve ser maior que 0.");
                    return;
                }
            }
            else if (itemCategory == ItemCategory.WeaponComponent)
            {
                if (type <= 0)
                {
                    player.SendNotification(NotificationType.Error, "Tipo deve ser maior que 0.");
                    return;
                }
            }
            else
            {
                if (type != 0)
                {
                    player.SendNotification(NotificationType.Error, "Tipo deve ser 0.");
                    return;
                }
            }

            var id = idString.ToGuid();
            if (Global.ItemsTemplates.Any(x => x.Id != id && x.Name.ToLower() == name.ToLower()))
            {
                player.SendNotification(NotificationType.Error, $"Já existe o item {name}.");
                return;
            }

            var isNew = string.IsNullOrWhiteSpace(idString);
            var itemTemplate = new ItemTemplate();
            if (isNew)
            {
                if (itemCategory == ItemCategory.Money || itemCategory == ItemCategory.PropertyKey
                    || itemCategory == ItemCategory.VehicleKey || itemCategory == ItemCategory.VehiclePart
                    || itemCategory == ItemCategory.BloodSample || Functions.CheckIfIsAmmo(itemCategory))
                {
                    player.SendNotification(NotificationType.Error, $"{itemCategory.GetDisplay()} só pode ter um item.");
                    return;
                }

                itemTemplate.Create(itemCategory, type, name, weight, image, objectModel);
            }
            else
            {
                itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == id);
                if (itemTemplate is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                itemTemplate.Update(type, name, weight, image, objectModel);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.ItemsTemplates.AddAsync(itemTemplate);
            else
                context.ItemsTemplates.Update(itemTemplate);

            await context.SaveChangesAsync();

            if (isNew)
                Global.ItemsTemplates.Add(itemTemplate);

            await player.WriteLog(LogType.Staff, $"Gravar Item | {Functions.Serialize(itemTemplate)}", null);
            player.SendNotification(NotificationType.Success, $"Item {(isNew ? "criado" : "editado")}.");
            UpdateItemsTemplates();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateItemsTemplates()
    {
        var json = GetItemsTemplatesJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Items)))
            target.Emit("StaffItemTemplate:Update", GetCategoriesJson(), json);
    }

    private static string GetItemsTemplatesJson()
    {
        static string GetType(ItemTemplate itemTemplate)
        {
            if (itemTemplate.Category == ItemCategory.Weapon)
                return Functions.GetWeaponName(itemTemplate.Type);

            if (itemTemplate.Category == ItemCategory.Boombox
                || itemTemplate.Category == ItemCategory.WeaponComponent)
                return itemTemplate.Type.ToString();

            return string.Empty;
        }

        return Functions.Serialize(Global.ItemsTemplates
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Category,
                CategoryDisplay = x.Category.GetDisplay(),
                Type = GetType(x),
                x.Name,
                x.Weight,
                x.Image,
                x.ObjectModel,
            }));
    }

    private static string GetCategoriesJson()
    {
        return Functions.Serialize(
            Enum.GetValues<ItemCategory>()
            .Select(category => new
            {
                Id = (int)category,
                Name = category.GetDisplay(),
                HasType = category == ItemCategory.Weapon || category == ItemCategory.Boombox || category == ItemCategory.WeaponComponent,
            })
            .OrderBy(x => x.Name)
        );
    }
}