using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffDoorScript : Script
{
    [Command(["portas"], "Staff", "Abre o painel de gerenciamento de portas")]
    public static void CMD_portas(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Doors))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffDoor:Show", GetDoorsJson());
    }

    [RemoteEvent(nameof(StaffDoorGoto))]
    public static void StaffDoorGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Doors))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var door = Global.Doors.FirstOrDefault(x => x.Id == id);
            if (door is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(door.PosX, door.PosY, door.PosZ), 0, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffDoorRemove))]
    public async Task StaffDoorRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Doors))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var door = Global.Doors.FirstOrDefault(x => x.Id == id);
            if (door == null)
                return;

            var context = Functions.GetDatabaseContext();
            context.Doors.Remove(door);
            await context.SaveChangesAsync();
            Global.Doors.Remove(door);
            door.SetLocked(false);
            door.SetupAllClients();

            await player.WriteLog(LogType.Staff, $"Remover Porta | {Functions.Serialize(door)}", null);
            player.SendNotification(NotificationType.Success, "Porta excluída.");
            UpdateDoors();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffDoorSave))]
    public async Task StaffDoorSave(Player playerParam, string idString, string name, long hash, Vector3 pos,
        string factionName, string companyName, bool locked)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Doors))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == factionName?.ToLower());
            if (!string.IsNullOrWhiteSpace(factionName) && faction is null)
            {
                player.SendNotification(NotificationType.Error, $"Facção {factionName} não existe.");
                return;
            }

            var company = Global.Companies.FirstOrDefault(x => x.Name.ToLower() == companyName?.ToLower());
            if (!string.IsNullOrWhiteSpace(companyName) && company is null)
            {
                player.SendNotification(NotificationType.Error, $"Empresa {companyName} não existe.");
                return;
            }

            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            var door = new Door();
            if (isNew)
            {
                door.Create(name, hash, pos.X, pos.Y, pos.Z, faction?.Id, company?.Id, locked);
            }
            else
            {
                door = Global.Doors.FirstOrDefault(x => x.Id == id);
                if (door == null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                door.Update(name, hash, pos.X, pos.Y, pos.Z, faction?.Id, company?.Id, locked);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Doors.AddAsync(door);
            else
                context.Doors.Update(door);

            await context.SaveChangesAsync();

            if (isNew)
                Global.Doors.Add(door);

            door.SetupAllClients();

            await player.WriteLog(LogType.Staff, $"Gravar Porta | {Functions.Serialize(door)}", null);
            player.SendNotification(NotificationType.Success, $"Porta {(isNew ? "criada" : "editada")}.");
            UpdateDoors();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateDoors()
    {
        var json = GetDoorsJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Doors)))
            target.Emit("StaffDoor:Update", json);
    }

    private static string GetDoorsJson()
    {
        return Functions.Serialize(Global.Doors.OrderByDescending(x => x.RegisterDate).Select(x => new
        {
            x.Id,
            x.Name,
            x.Hash,
            x.PosX,
            x.PosY,
            x.PosZ,
            FactionName = Global.Factions.FirstOrDefault(y => y.Id == x.FactionId)?.Name,
            CompanyName = Global.Companies.FirstOrDefault(y => y.Id == x.CompanyId)?.Name,
            x.Locked,
        }));
    }
}