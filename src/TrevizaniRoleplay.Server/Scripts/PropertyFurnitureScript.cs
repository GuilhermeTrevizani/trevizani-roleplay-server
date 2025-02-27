using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PropertyFurnitureScript : Script
{
    private static bool CanEditFurniture(Property property, MyPlayer player)
    {
        if (player.OnAdminDuty && player.StaffFlags.Contains(StaffFlag.Properties))
            return true;

        if (!property.CanAccess(player))
            return false;

        if (property.FactionId.HasValue && !player.FactionFlags.Contains(FactionFlag.Furnitures))
            return false;

        if (property.CompanyId.HasValue
            && !player.CheckCompanyPermission(Global.Companies.FirstOrDefault(x => x.Id == property.CompanyId)!, CompanyFlag.Furnitures))
            return false;

        return true;
    }

    [Command("mobilias")]
    public async Task CMD_mobilias(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        property ??= Global.Properties.Where(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= Constants.PROPERTY_EXTERIOR_FURNITURE_DISTANCE
            && x.EntranceDimension == player.GetDimension())
        .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Você não está no interior ou a {Constants.PROPERTY_EXTERIOR_FURNITURE_DISTANCE} metros da entrada de uma propriedade.");
            return;
        }

        if (!CanEditFurniture(property, player))
        {
            player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
            return;
        }

        await ViewFurnitures(player, property);
    }

    [RemoteEvent(nameof(ClosePropertyFurnitures))]
    public static void ClosePropertyFurnitures(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.Invincible = false;
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task ViewFurnitures(MyPlayer player, Property property)
    {
        var interior = player.GetDimension() != 0;

        var propertyFurnituresJson = Functions.Serialize(property.Furnitures!
            .Where(x => x.Interior == interior)
            .Select(x => new
            {
                x.Id,
                Name = x.GetModelName(),
                Distance = player.GetPosition().DistanceTo(x.GetPosition()),
                Value = x.GetFurniture()?.Value ?? 0,
                UseSlot = x.GetFurniture()?.UseSlot ?? true,
            })
            .OrderBy(x => x.Distance));

        var categoriesJson = Functions.Serialize(Global.Furnitures
            .Where(x => x.Category.ToLower() != Globalization.BARRIERS)
            .GroupBy(x => x.Category)
            .Select(x => new
            {
                Name = x.Key,
                Subcategories = x
                    .GroupBy(y => y.Subcategory)
                    .Select(y => new
                    {
                        Name = y.Key
                    })
                    .OrderBy(y => y.Name)
            })
            .OrderBy(x => x.Name));

        player.Invincible = true;
        var chunks = Functions.ChunkString(propertyFurnituresJson, 20_000);
        foreach (var chunk in chunks)
        {
            var index = chunks.IndexOf(chunk);
            player.Emit("PropertyFurnitures",
                property.Id.ToString(),
                await GetMaxFurnitures(property, interior),
                categoriesJson,
                chunk,
                index,
                chunks.Count);
        }
    }

    [RemoteEvent(nameof(ListFurnitures))]
    public static void ListFurnitures(Player playerParam, string category, string subcategory)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var furnitures = Global.Furnitures
                .Where(x => x.Category.ToLower() == category.ToLower()
                    && x.Subcategory.ToLower() == subcategory.ToLower())
                .Select(x => new
                {
                    x.Id,
                    x.Category,
                    x.Subcategory,
                    x.Name,
                    x.Model,
                    x.Value,
                })
                .OrderBy(x => x.Name)
                .ToList();

            player.Emit("PropertyFurnituresPage:ListFurnituresServer", Functions.Serialize(furnitures));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SelectBuyPropertyFurniture))]
    public static void SelectBuyPropertyFurniture(Player playerParam, string propertyIdString, string furnitureIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var propertyId = new Guid(propertyIdString);
            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyId);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var furnitureId = new Guid(furnitureIdString);
            var furniture = Global.Furnitures.FirstOrDefault(x => x.Id == furnitureId);
            if (furniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.DropPropertyFurniture = new PropertyFurniture();
            player.DropPropertyFurniture.Create(property.Id, furniture.Model, player.GetDimension() != property.EntranceDimension);
            player.Emit("DropObject", player.DropPropertyFurniture.Model, 2);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(EditPropertyFurniture))]
    public static void EditPropertyFurniture(Player playerParam, string propertyIdString, string propertyFurnitureIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var propertyId = new Guid(propertyIdString);
            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyId);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var propertyFurnitureId = new Guid(propertyFurnitureIdString);
            player.DropPropertyFurniture = property.Furnitures!.FirstOrDefault(x => x.Id == propertyFurnitureId);
            if (player.DropPropertyFurniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.DropPropertyFurniture.DeleteObject();
            player.Emit("DropObject", player.DropPropertyFurniture.Model, 2,
                new Vector3(player.DropPropertyFurniture.PosX, player.DropPropertyFurniture.PosY, player.DropPropertyFurniture.PosZ),
                new Vector3(player.DropPropertyFurniture.RotR, player.DropPropertyFurniture.RotP, player.DropPropertyFurniture.RotY));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SellPropertyFurniture))]
    public async Task SellPropertyFurniture(Player playerParam, string propertyIdString, string propertyFurnitureIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyIdString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (!CanEditFurniture(property, player))
            {
                player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            var propertyFurnitureId = new Guid(propertyFurnitureIdString);
            var propertyFurniture = property.Furnitures!.FirstOrDefault(x => x.Id == propertyFurnitureId);
            if (propertyFurniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var furniture = Global.Furnitures.FirstOrDefault(x => x.Model.ToLower() == propertyFurniture.Model.ToLower());
            if (furniture is null)
            {
                player.SendNotification(NotificationType.Error, $"Mobília {propertyFurniture.Model} não encontrada. Por favor, reporte o bug.");
                return;
            }

            var charge = !player.OnAdminDuty || !player.StaffFlags.Contains(StaffFlag.Properties);
            if (furniture.Value > 0 && charge)
            {
                var res = await player.GiveMoney(furniture.Value);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    player.SendNotification(NotificationType.Error, res);
                    return;
                }
            }

            var context = Functions.GetDatabaseContext();
            context.PropertiesFurnitures.Remove(propertyFurniture);
            await context.SaveChangesAsync();

            property.Furnitures!.Remove(propertyFurniture);
            propertyFurniture.DeleteObject();

            await player.WriteLog(LogType.SellPropertyFurniture, $"{Functions.Serialize(propertyFurniture)} {charge}", null);
            if (furniture.Value > 0 && charge)
                player.SendNotification(NotificationType.Success, $"Você vendeu {propertyFurniture.GetModelName()} por ${furniture.Value:N0}.");
            else
                player.SendNotification(NotificationType.Success, $"Você removeu {propertyFurniture.GetModelName()}.");
            await ViewFurnitures(player, property);

            if (propertyFurniture.Id == player.DropPropertyFurniture?.Id)
            {
                player.Emit("ClearDropObject");
                player.DropPropertyFurniture = null;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CancelDropFurniture))]
    public async Task CancelDropFurniture(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.DropPropertyFurniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Id == player.DropPropertyFurniture.PropertyId);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.SendNotification(NotificationType.Success, "Você cancelou o drop da mobília.");

            if (player.DropPropertyFurniture.PosX != 0)
                player.DropPropertyFurniture.CreateObject();

            player.DropPropertyFurniture = null;
            await ViewFurnitures(player, property);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task<int> GetMaxFurnitures(Property property, bool interior)
    {
        var userPremium = await property.GetOwnerPremium();

        int maxFurnitures;
        if (interior)
        {
            maxFurnitures = userPremium.Item1 switch
            {
                UserPremium.Gold => 1000,
                UserPremium.Silver => 600,
                UserPremium.Bronze => 300,
                _ => 100,
            };

            maxFurnitures += userPremium.Item2;
        }
        else
        {
            maxFurnitures = userPremium.Item1 switch
            {
                UserPremium.Gold => 10,
                UserPremium.Silver => 6,
                UserPremium.Bronze => 4,
                _ => 2,
            };
        }

        return maxFurnitures;
    }

    [RemoteEvent(nameof(ConfirmDropFurniture))]
    public async Task ConfirmDropFurniture(Player playerParam, Vector3 position, Vector3 rotation)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.DropPropertyFurniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (position.X == 0 && position.Y == 0)
            {
                player.SendNotification(NotificationType.Error, "Não foi possível recuperar a posição do item.");
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Id == player.DropPropertyFurniture.PropertyId);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (!CanEditFurniture(property, player))
            {
                player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            var furniture = Global.Furnitures.FirstOrDefault(x => x.Model.ToLower() == player.DropPropertyFurniture.Model.ToLower());
            if (furniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (!player.DropPropertyFurniture.Interior
                && property.GetEntrancePosition().DistanceTo(position) > Constants.PROPERTY_EXTERIOR_FURNITURE_DISTANCE)
            {
                player.SendNotification(NotificationType.Error, $"Objeto está mais de {Constants.PROPERTY_EXTERIOR_FURNITURE_DISTANCE} metros da entrada da propriedade.");
                return;
            }

            var charge = !player.OnAdminDuty || !player.StaffFlags.Contains(StaffFlag.Properties);
            var newFurniture = false;
            if (player.DropPropertyFurniture.PosX == 0 && player.DropPropertyFurniture.PosY == 0)
            {
                newFurniture = true;

                var maxFurnitures = await GetMaxFurnitures(property, player.DropPropertyFurniture.Interior);
                var furnituresCount = property.Furnitures!.Count(x => x.Interior == player.DropPropertyFurniture.Interior
                    && (Global.Furnitures.FirstOrDefault(y => y.Model.ToLower() == x.Model.ToLower())?.UseSlot ?? true));
                if (furnituresCount >= maxFurnitures)
                {
                    player.SendNotification(NotificationType.Error, $"O limite de {maxFurnitures} mobílias da propriedade foi atingido.");
                    return;
                }

                if (charge)
                {
                    if (player.Money < furniture.Value)
                    {
                        player.SendNotification(NotificationType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, furniture.Value));
                        return;
                    }

                    await player.RemoveMoney(furniture.Value);
                }
            }

            player.DropPropertyFurniture.SetPosition(position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z);

            var context = Functions.GetDatabaseContext();
            if (newFurniture)
                await context.PropertiesFurnitures.AddAsync(player.DropPropertyFurniture);
            else
                context.PropertiesFurnitures.Update(player.DropPropertyFurniture);

            await context.SaveChangesAsync();

            await player.WriteLog(LogType.EditPropertyFurniture, $"{Functions.Serialize(player.DropPropertyFurniture)} {charge}", null);

            if (newFurniture)
                player.DropPropertyFurniture = await context.PropertiesFurnitures
                    .Include(x => x.Property)
                    .FirstOrDefaultAsync(x => x.Id == player.DropPropertyFurniture.Id)!;

            if (!property.Furnitures!.Contains(player.DropPropertyFurniture!))
                property.Furnitures.Add(player.DropPropertyFurniture!);

            player.DropPropertyFurniture!.CreateObject();

            if (newFurniture)
            {
                if (charge)
                    player.SendNotification(NotificationType.Success, $"Você comprou {furniture.Name}.");
                else
                    player.SendNotification(NotificationType.Success, $"Você comprou {furniture.Name} por ${furniture.Value:N0}.");
            }
            else
            {
                player.SendNotification(NotificationType.Success, $"Você editou a posição de {furniture.Name}.");
            }

            player.DropPropertyFurniture = null;
            await ViewFurnitures(player, property);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CopyPropertyFurniture))]
    public static void CopyPropertyFurniture(Player playerParam, string propertyIdString, string propertyFurnitureIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyIdString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var propertyFurniture = property.Furnitures!.FirstOrDefault(x => x.Id == propertyFurnitureIdString.ToGuid());
            if (propertyFurniture is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (player.DropPropertyFurniture is not null)
            {
                player.Emit("DropObject", player.DropPropertyFurniture.Model, 2,
                    new Vector3(propertyFurniture.PosX, propertyFurniture.PosY, propertyFurniture.PosZ),
                    new Vector3(propertyFurniture.RotR, propertyFurniture.RotP, propertyFurniture.RotY));
                return;
            }

            player.DropPropertyFurniture = new PropertyFurniture();
            player.DropPropertyFurniture.Create(property.Id, propertyFurniture.Model, player.GetDimension() != property.EntranceDimension);
            player.Emit("DropObject", player.DropPropertyFurniture.Model, 2,
                new Vector3(propertyFurniture.PosX, propertyFurniture.PosY, propertyFurniture.PosZ),
                new Vector3(propertyFurniture.RotR, propertyFurniture.RotP, propertyFurniture.RotY));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}