using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffCrackDenScript : Script
{
    [Command(["bocasfumo"], "Staff", "Abre o painel gerenciamento de bocas de fumo")]
    public static void CMD_bocasfumo(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffCrackDen:Show", GetCrackDensJson());
    }

    [RemoteEvent(nameof(StaffCrackDenGoto))]
    public static void StaffCrackDenGoto(MyPlayer playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var crackDen = Global.CrackDens.FirstOrDefault(x => x.Id == id);
            if (crackDen is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(crackDen.PosX, crackDen.PosY, crackDen.PosZ), crackDen.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCrackDenRemove))]
    public async Task StaffCrackDenRemove(MyPlayer playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var crackDen = Global.CrackDens.FirstOrDefault(x => x.Id == id);
            if (crackDen is null)
                return;

            var context = Functions.GetDatabaseContext();
            context.CrackDens.Remove(crackDen);
            context.CrackDensItems.RemoveRange(Global.CrackDensItems.Where(x => x.CrackDenId == id));
            await context.SaveChangesAsync();
            await context.CrackDensSells.Where(x => x.CrackDenId == id).ExecuteDeleteAsync();
            Global.CrackDens.Remove(crackDen);
            Global.CrackDensItems.RemoveAll(x => x.CrackDenId == crackDen.Id);
            crackDen.RemoveIdentifier();

            await player.WriteLog(LogType.Staff, $"Remover Boca de Fumo | {Functions.Serialize(crackDen)}", null);
            player.SendNotification(NotificationType.Success, "Boca de fumo excluída.");
            UpdateCrackDens();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateCrackDens()
    {
        var json = GetCrackDensJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.CrackDens)))
            target.Emit("StaffCrackDen:Update", json);
    }

    [RemoteEvent(nameof(StaffCrackDenSave))]
    public async Task StaffCrackDenSave(MyPlayer playerParam, string idString, Vector3 pos, uint dimension,
        int onlinePoliceOfficers, int cooldownQuantityLimit, int cooldownHours)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (onlinePoliceOfficers < 0)
            {
                player.SendNotification(NotificationType.Error, $"Policiais Online deve ser maior ou igual a 0.");
                return;
            }

            if (cooldownQuantityLimit < 0)
            {
                player.SendNotification(NotificationType.Error, $"Quantidade Limite Cooldown deve ser maior ou igual a 0.");
                return;
            }

            if (cooldownHours < 0)
            {
                player.SendNotification(NotificationType.Error, $"Horas Cooldown deve ser maior ou igual a 0.");
                return;
            }

            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            var crackDen = new CrackDen();
            if (isNew)
            {
                crackDen.Create(pos.X, pos.Y, pos.Z, dimension, onlinePoliceOfficers, cooldownQuantityLimit, cooldownHours);
            }
            else
            {
                crackDen = Global.CrackDens.FirstOrDefault(x => x.Id == id);
                if (crackDen == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                crackDen.Update(pos.X, pos.Y, pos.Z, dimension, onlinePoliceOfficers, cooldownQuantityLimit, cooldownHours);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.CrackDens.AddAsync(crackDen);
            else
                context.CrackDens.Update(crackDen);

            await context.SaveChangesAsync();

            if (isNew)
                Global.CrackDens.Add(crackDen);

            crackDen.CreateIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Boca de Fumo | {Functions.Serialize(crackDen)}", null);
            player.SendNotification(NotificationType.Success, $"Boca de fumo {(isNew ? "criada" : "editada")}.");
            UpdateCrackDens();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCrackDensItemsShow))]
    public static void StaffCrackDensItemsShow(MyPlayer playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            player.Emit("StaffCrackDenItem:Show", idString, GetCrackDensItemsJson(id!.Value), GetDrugsItemsTemplatesResponse());
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetDrugsItemsTemplatesResponse()
    {
        return Functions.Serialize(
            Global.ItemsTemplates
            .Where(x => x.Category == ItemCategory.Drug)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
            })
        );
    }

    [RemoteEvent(nameof(StaffCrackDenItemSave))]
    public async Task StaffCrackDenItemSave(MyPlayer playerParam,
        string crackDenIdString,
        string crackDenItemIdString,
        string itemTemplateIdString,
        int value)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == itemTemplateIdString.ToGuid());
            if (itemTemplate is null)
            {
                player.SendNotification(NotificationType.Error, $"{itemTemplateIdString} não é uma droga.");
                return;
            }

            if (itemTemplate.Category != ItemCategory.Drug)
            {
                player.SendNotification(NotificationType.Error, $"{itemTemplate.Name} não é uma droga.");
                return;
            }

            if (value <= 0)
            {
                player.SendNotification(NotificationType.Error, "Valor deve ser maior que 0.");
                return;
            }

            var crackDenId = new Guid(crackDenIdString);
            var crackDenItemId = crackDenItemIdString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(crackDenItemIdString);
            var crackDenItem = new CrackDenItem();
            if (isNew)
            {
                crackDenItem.Create(crackDenId, itemTemplate.Id, value);
            }
            else
            {
                crackDenItem = Global.CrackDensItems.FirstOrDefault(x => x.Id == crackDenItemId);
                if (crackDenItem is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                crackDenItem.Update(itemTemplate.Id, value);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.CrackDensItems.AddAsync(crackDenItem);
            else
                context.CrackDensItems.Update(crackDenItem);

            await context.SaveChangesAsync();

            if (isNew)
                Global.CrackDensItems.Add(crackDenItem);

            await player.WriteLog(LogType.Staff, $"Gravar Item Boca de Fumo | {Functions.Serialize(crackDenItem)}", null);
            player.SendNotification(NotificationType.Success, $"Item {(isNew ? "criado" : "editado")}.");
            UpdateCrackDensItems(crackDenItem.CrackDenId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCrackDenItemRemove))]
    public async Task StaffCrackDenItemRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var crackDenItem = Global.CrackDensItems.FirstOrDefault(x => x.Id == id);
            if (crackDenItem is null)
                return;

            var context = Functions.GetDatabaseContext();
            context.CrackDensItems.Remove(crackDenItem);
            await context.SaveChangesAsync();
            Global.CrackDensItems.Remove(crackDenItem);

            await player.WriteLog(LogType.Staff, $"Remover Item Boca de Fumo | {Functions.Serialize(crackDenItem)}", null);
            player.SendNotification(NotificationType.Success, $"Item da Boca de Fumo excluído.");
            UpdateCrackDensItems(crackDenItem.CrackDenId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateCrackDensItems(Guid crackDenId)
    {
        var json = GetCrackDensItemsJson(crackDenId);
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.CrackDens)))
            target.Emit("StaffCrackDenItem:Update", json);
    }

    [RemoteEvent(nameof(StaffCrackDenRevokeCooldown))]
    public async Task StaffCrackDenRevokeCooldown(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CrackDens))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var crackDen = Global.CrackDens.FirstOrDefault(x => x.Id == id);
            if (crackDen is null)
                return;

            crackDen.ResetCooldownDate();

            var context = Functions.GetDatabaseContext();
            context.CrackDens.Update(crackDen);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Revogar Cool Down Boca de Fumo | {id}", null);
            player.SendNotification(NotificationType.Success, $"Cooldown da boca de fumo revogado.");
            UpdateCrackDens();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetCrackDensJson()
    {
        return Functions.Serialize(Global.CrackDens
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Dimension,
                x.OnlinePoliceOfficers,
                x.CooldownQuantityLimit,
                x.CooldownHours,
                x.CooldownDate,
                x.Quantity,
            }));
    }

    private static string GetCrackDensItemsJson(Guid crackDenId)
    {
        return Functions.Serialize(Global.CrackDensItems
            .Where(x => x.CrackDenId == crackDenId)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.ItemTemplateId,
                ItemTemplateName = Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)!.Name,
                x.Value,
            }));
    }
}