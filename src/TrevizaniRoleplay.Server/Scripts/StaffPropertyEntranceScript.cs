using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffPropertyEntranceScript : Script
{
    [Command("entradasprop", "/entradasprop (número)")]
    public static void CMD_entradasprop(MyPlayer player, int number)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Properties))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == number);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma propriedade encontrada com o número {number}.");
            return;
        }

        player.Emit("StaffPropertyEntrance:Show", property.Id.ToString(), GetPropertiesEntrancesJson(property.Id));
    }

    [RemoteEvent(nameof(StaffPropertyEntranceGoto))]
    public static void StaffPropertyEntranceGoto(Player playerParam, string propertyIdString, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var propertyId = propertyIdString.ToGuid();
            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyId);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var id = idString.ToGuid();
            var propertyEntrance = property.Entrances!.FirstOrDefault(x => x.Id == id);
            if (propertyEntrance is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(propertyEntrance.GetEntrancePosition(), property.EntranceDimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffPropertyEntranceSave))]
    public async Task StaffPropertyEntranceSave(Player playerParam, string propertyIdString, string idString, Vector3 entrancePos, Vector3 exitPos,
        Vector3 entranceRot, Vector3 exitRot)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Properties))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyIdString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var propertyEntrance = new PropertyEntrance();
            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            if (isNew)
            {
                propertyEntrance.Create(property.Id,
                    entrancePos.X, entrancePos.Y, entrancePos.Z,
                    exitPos.X, exitPos.Y, exitPos.Z,
                    entranceRot.X, entranceRot.Y, entranceRot.Z,
                    exitRot.X, exitRot.Y, exitRot.Z);
            }
            else
            {
                propertyEntrance = property.Entrances!.FirstOrDefault(x => x.Id == id);
                if (propertyEntrance is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                propertyEntrance.Update(entrancePos.X, entrancePos.Y, entrancePos.Z,
                    exitPos.X, exitPos.Y, exitPos.Z,
                    entranceRot.X, entranceRot.Y, entranceRot.Z,
                    exitRot.X, exitRot.Y, exitRot.Z);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.PropertiesEntrances.AddAsync(propertyEntrance);
            else
                context.PropertiesEntrances.Update(propertyEntrance);

            await context.SaveChangesAsync();

            if (isNew && !property.Entrances!.Contains(propertyEntrance))
                property.Entrances.Add(propertyEntrance);

            property.CreateIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Propriedade Entrada | {Functions.Serialize(propertyEntrance)}", null);
            player.SendNotification(NotificationType.Success, $"Entrada {(isNew ? "criada" : "editada")}.");
            UpdatePropertiesEntrances(player, property.Id);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffPropertyEntranceRemove))]
    public async Task StaffPropertyEntranceRemove(Player playerParam, string propertyIdString, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Properties))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Id == propertyIdString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var propertyEntrance = property.Entrances!.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (propertyEntrance is not null)
            {
                var context = Functions.GetDatabaseContext();
                context.PropertiesEntrances.Remove(propertyEntrance);
                await context.SaveChangesAsync();
                property.Entrances!.Remove(propertyEntrance);
                property.CreateIdentifier();
                await player.WriteLog(LogType.Staff, $"Remover Entrada Propriedade | {Functions.Serialize(propertyEntrance)}", null);
            }

            player.SendNotification(NotificationType.Success, "Entrada excluída.");
            UpdatePropertiesEntrances(player, property.Id);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdatePropertiesEntrances(MyPlayer player, Guid propertyId)
    {
        var json = GetPropertiesEntrancesJson(propertyId);
        player.Emit("StaffPropertyEntrance:Update", json);
    }

    private static string GetPropertiesEntrancesJson(Guid propertyId)
    {
        return Functions.Serialize(Global.Properties.FirstOrDefault(x => x.Id == propertyId)!
            .Entrances!
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.EntrancePosX,
                x.EntrancePosY,
                x.EntrancePosZ,
                x.ExitPosX,
                x.ExitPosY,
                x.ExitPosZ,
                x.EntranceRotR,
                x.EntranceRotP,
                x.EntranceRotY,
                x.ExitRotR,
                x.ExitRotP,
                x.ExitRotY,
            }));
    }
}