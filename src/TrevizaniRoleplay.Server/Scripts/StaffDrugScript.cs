using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffDrugScript : Script
{
    [Command("drogas")]
    public async Task CMD_drogas(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Drugs))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        foreach (var itemTemplate in Global.ItemsTemplates.Where(x => x.Category == ItemCategory.Drug))
        {
            var drug = Global.Drugs.FirstOrDefault(x => x.ItemTemplateId == itemTemplate.Id);
            if (drug is null)
            {
                drug ??= new Drug();
                drug.Create(itemTemplate.Id);
                var context = Functions.GetDatabaseContext();
                await context.Drugs.AddAsync(drug);
                await context.SaveChangesAsync();
                Global.Drugs.Add(drug);
            }
        }

        player.Emit("StaffDrug:Show", GetDrugsJson());
    }

    [RemoteEvent(nameof(StaffDrugSave))]
    public async Task StaffDrugSave(Player playerParam, string json)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Drugs))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var drugRequest = Functions.Deserialize<DrugRequest>(json);
            var drug = Global.Drugs.FirstOrDefault(x => x.Id == drugRequest.Id);
            if (drug is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            drug.Update(drugRequest.ThresoldDeath, drugRequest.Health, drugRequest.GarbageCollectorMultiplier, drugRequest.TruckerMultiplier,
                drugRequest.MinutesDuration, drugRequest.Warn, drugRequest.ShakeGameplayCamName, drugRequest.ShakeGameplayCamIntensity,
                drugRequest.TimecycModifier, drugRequest.AnimpostFXName);
            var context = Functions.GetDatabaseContext();
            context.Drugs.Update(drug);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Gravar Droga | {Functions.Serialize(drug)}", null);
            player.SendNotification(NotificationType.Success, "Droga editada.");
            await CMD_drogas(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetDrugsJson()
    {
        return Functions.Serialize(
            Global.Drugs
            .Select(x => new
            {
                x.Id,
                Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)?.Name,
                x.ThresoldDeath,
                x.Health,
                x.GarbageCollectorMultiplier,
                x.TruckerMultiplier,
                x.MinutesDuration,
                x.Warn,
                x.ShakeGameplayCamName,
                x.ShakeGameplayCamIntensity,
                x.TimecycModifier,
                x.AnimpostFXName,
            })
            .OrderByDescending(x => x.Name));
    }
}