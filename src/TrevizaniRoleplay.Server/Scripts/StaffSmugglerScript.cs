using GTANetworkAPI;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffSmugglerScript : Script
{
    [Command(["contrabandistas"], "Staff", "Abre o painel de gerenciamento de contrabandistas")]
    public static void CMD_contrabandistas(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Factions))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffSmuggler:Show", GetSmugglersJson());
    }

    [RemoteEvent(nameof(StaffSmugglerGoto))]
    public static void StaffSmugglerGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var smuggler = Global.Smugglers.FirstOrDefault(x => x.Id == id);
            if (smuggler is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(smuggler.PosX, smuggler.PosY, smuggler.PosZ), smuggler.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffTSmugglerRemove))]
    public async Task StaffTSmugglerRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var smuggler = Global.Smugglers.FirstOrDefault(x => x.Id == id);
            if (smuggler is null)
                return;

            var context = Functions.GetDatabaseContext();
            context.Smugglers.Remove(smuggler);
            await context.SaveChangesAsync();
            Global.Smugglers.Remove(smuggler);
            smuggler.RemoveIdentifier();

            await player.WriteLog(LogType.Staff, $"Remover Contrabandista | {Functions.Serialize(smuggler)}", null);
            player.SendNotification(NotificationType.Success, "Contrabandista excluído.");
            UpdateSmugglers();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffSmugglerSave))]
    public async Task StaffSmugglerSave(Player playerParam, string idString, string model,
        uint dimension, Vector3 position, Vector3 rotation,
        int value, int cooldownQuantityLimit, int cooldownMinutes, string allowedCharactersJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Factions))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (!Functions.CheckIfPedModelExists(model))
            {
                player.SendNotification(NotificationType.Error, $"Modelo {model} não existe.");
                return;
            }

            if (value <= 0)
            {
                player.SendNotification(NotificationType.Error, "Valor deve ser maior que 0.");
                return;
            }

            if (cooldownQuantityLimit <= 0)
            {
                player.SendNotification(NotificationType.Error, "Quantidade Limite Cooldown deve ser maior que 0.");
                return;
            }

            if (cooldownMinutes <= 0)
            {
                player.SendNotification(NotificationType.Error, "Minutos Cooldown deve ser maior que 0.");
                return;
            }

            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            var smuggler = new Smuggler();
            if (isNew)
            {
                var cellphone = await Functions.GetNewCellphoneNumber();
                smuggler.Create(cellphone, model, dimension, position.X, position.Y, position.Z, rotation.X,
                    rotation.Y, rotation.Z, allowedCharactersJSON, value, cooldownQuantityLimit, cooldownMinutes);
            }
            else
            {
                smuggler = Global.Smugglers.FirstOrDefault(x => x.Id == id);
                if (smuggler is null)
                {
                    player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                    return;
                }

                smuggler.Update(model, dimension, position.X, position.Y, position.Z, rotation.X,
                    rotation.Y, rotation.Z, allowedCharactersJSON, value, cooldownQuantityLimit, cooldownMinutes);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Smugglers.AddAsync(smuggler);
            else
                context.Smugglers.Update(smuggler);

            await context.SaveChangesAsync();

            if (isNew)
                Global.Smugglers.Add(smuggler);

            smuggler.RemoveIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Contrabandista | {Functions.Serialize(smuggler)}", null);
            player.SendNotification(NotificationType.Success, $"Contrabandista {(isNew ? "criado" : "editado")}.");
            UpdateSmugglers();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void UpdateSmugglers()
    {
        var json = GetSmugglersJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Factions)))
            target.Emit("StaffSmuggler:Update", json);
    }

    public static string GetSmugglersJson()
    {
        return Functions.Serialize(Global.Smugglers.OrderByDescending(x => x.RegisterDate).Select(x => new
        {
            x.Id,
            x.Model,
            x.Dimension,
            x.PosX,
            x.PosY,
            x.PosZ,
            x.RotR,
            x.RotP,
            x.RotY,
            x.Value,
            x.CooldownQuantityLimit,
            x.CooldownMinutes,
            x.CooldownDate,
            x.Quantity,
            x.Cellphone,
            AllowedCharacters = x.GetAllowedCharacters(),
        }));
    }
}