using GTANetworkAPI;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffFactionFrequencyScript : Script
{
    private static string GetFactionFrequenciesJson(Guid factionId)
    {
        return Functions.Serialize(Global.FactionsFrequencies.Where(x => x.FactionId == factionId).OrderBy(x => x.Frequency).Select(x => new
        {
            x.Id,
            x.Frequency,
            x.Name,
        }));
    }

    [RemoteEvent(nameof(StaffFactionShowFrequencies))]
    public static void StaffFactionShowFrequencies(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            var json = GetFactionFrequenciesJson(id!.Value);
            player.Emit("StaffFactionFrequency:Show", json, idString, Global.Factions.FirstOrDefault(x => x.Id == id)!.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionFrequencySave))]
    public async Task StaffFactionFrequencySave(Player playerParam, string factionIdString, string factionFrequencyIdString, int frequency, string name)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (frequency <= 0)
            {
                player.SendNotification(NotificationType.Error, "Frequência deve ser maior que 0.");
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                player.SendNotification(NotificationType.Error, "Nome não preenchido.");
                return;
            }

            var factionId = factionIdString.ToGuid();
            var factionFrequency = new FactionFrequency();
            var id = factionFrequencyIdString.ToGuid();

            if (Global.FactionsFrequencies.Any(x => x.Frequency == frequency && x.Id != id))
            {
                player.SendNotification(NotificationType.Error, $"A frequência {frequency} já está sendo utilizada.");
                return;
            }

            var isNew = string.IsNullOrWhiteSpace(factionFrequencyIdString);
            if (isNew)
            {
                factionFrequency.Create(factionId!.Value, frequency, name);
            }
            else
            {
                factionFrequency = Global.FactionsFrequencies.FirstOrDefault(x => x.Id == id);
                if (factionFrequency is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                factionFrequency.Update(frequency, name);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.FactionsFrequencies.AddAsync(factionFrequency);
            else
                context.FactionsFrequencies.Update(factionFrequency);

            await context.SaveChangesAsync();

            if (isNew)
                Global.FactionsFrequencies.Add(factionFrequency);

            await player.WriteLog(LogType.Staff, $"Gravar Frequência | {Functions.Serialize(factionFrequency)}", null);
            player.SendNotification(NotificationType.Success, $"Frequência {(isNew ? "criada" : "editada")}.");
            UpdateFrequencies(factionId!.Value);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffFactionFrequencyRemove))]
    public async Task StaffFactionFrequencyRemove(Player playerParam, string factionIdString, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var factionId = new Guid(factionIdString);

            var id = idString.ToGuid();
            var factionFrequency = Global.FactionsFrequencies.FirstOrDefault(x => x.Id == id);
            if (factionFrequency is not null)
            {
                var context = Functions.GetDatabaseContext();
                context.FactionsFrequencies.Remove(factionFrequency);
                await context.SaveChangesAsync();
                Global.FactionsFrequencies.Remove(factionFrequency);
                await player.WriteLog(LogType.Staff, $"Remover Frequência | {Functions.Serialize(factionFrequency)}", null);
            }

            player.SendNotification(NotificationType.Success, "Frequência excluída.");
            UpdateFrequencies(factionId);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateFrequencies(Guid factionId)
    {
        var json = GetFactionFrequenciesJson(factionId);
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Factions)))
            target.Emit("StaffFactionFrequency:Update", json);
    }
}