using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffEventScript : Script
{
    [Command(["tempo"], "Staff", "Define um tempo e temperatura fixos", "(tempo) (temperatura)")]
    public async Task CMD_tempo(MyPlayer player, int tempo, int temperatura)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Events))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!Enum.IsDefined(typeof(Weather), tempo))
        {
            player.SendMessage(MessageType.Error, "Tempo inválido.");
            return;
        }

        Global.WeatherInfo.WeatherType = (Weather)tempo;
        Global.WeatherInfo.Main.Temp = temperatura;
        Global.WeatherInfo.Manual = true;
        Functions.SetWeatherInfo();

        await Functions.SendServerMessage($"{player.User.Name} alterou o tempo para {Global.WeatherInfo.WeatherType} e a temperatura para {Global.WeatherInfo.Main.Temp:N0}°C.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, $"/tempo {Global.WeatherInfo.WeatherType} {Global.WeatherInfo.Main.Temp:N0}", null);
    }

    [Command(["anrp"], "Staff", "Envia um anúncio de roleplay", "(mensagem)", GreedyArg = true)]
    public async Task CMD_anrp(MyPlayer player, string message)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Events))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        foreach (var target in Global.SpawnedPlayers)
            target.SendMessage(MessageType.None, $"[Anúncio de Roleplay] {message}", "#ed0202");

        await Functions.SendServerMessage($"{player.User.Name} enviou o anúncio de roleplay.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, $"/anrp {message}", null);

        if (Global.DiscordClient is null
            || Global.DiscordClient.GetChannel(Global.RoleplayAnnouncementDiscordChannel) is not SocketTextChannel channel)
            return;

        var embedBuilder = new Discord.EmbedBuilder
        {
            Title = "Anúncio de Roleplay",
            Description = message,
            Color = new Discord.Color(Global.MainRgba.Red, Global.MainRgba.Green, Global.MainRgba.Blue),
        };
        embedBuilder.WithFooter($"Enviado em {DateTime.Now}.");

        await channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    [Command(["rtempo"], "Staff", "Remove o tempo e ativa a sincronização automática")]
    public async Task CMD_rtempo(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Events))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        Global.WeatherInfo.Manual = false;

        await Functions.SendServerMessage($"{player.User.Name} removeu o tempo fixo e ativou a sincronização automática.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, "/rtempo", null);
    }

    [Command(["enome"], "Staff", "Define um nome temporário para seu personagem", "(ID ou nome) (novo nome)", GreedyArg = true, AllowEmptyStrings = true)]
    public async Task CMD_enome(MyPlayer player, string idOrName, string newName)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Events))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        var context = Functions.GetDatabaseContext();
        if (await context.Characters.AnyAsync(x => x.Name.ToLower() == newName.ToLower()))
        {
            player.SendMessage(MessageType.Error, $"Nome {newName} já é utilizado por um personagem.");
            return;
        }

        target.TemporaryName = newName;
        var message = string.IsNullOrWhiteSpace(newName) ? $"{player.User.Name} removeu o nome temporário de {target.Character.Name}."
            :
            $"{player.User.Name} alterou o nome temporário de {target.Character.Name} para {newName}.";
        await Functions.SendServerMessage(message, UserStaff.GameAdmin, false);
        var targetMessage = string.IsNullOrWhiteSpace(newName) ? $"{player.User.Name} removeu o seu nome temporário."
            :
            $"{player.User.Name} alterou seu nome temporário para {newName}.";
        target.SendMessage(MessageType.Success, targetMessage);
        target.SetNametag();

        await player.WriteLog(LogType.Staff, $"/enome {newName}", target);
    }

    [Command(["skin"], "Staff", "Altera a skin de um jogador", "(ID ou nome) (skin)")]
    public async Task CMD_skin(MyPlayer player, string idOrName, string skin)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Events))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!Functions.CheckIfPedModelExists(skin))
        {
            player.SendMessage(MessageType.Error, $"Skin {skin} não existe.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        target.SetModel(Functions.Hash(skin));
        if (target.Model == (uint)player.CorrectPed)
        {
            target.SetPersonalization();
            target.SetOutfit();
        }

        await Functions.SendServerMessage($"{player.User.Name} alterou a skin de {target.Character.Name} para {skin}.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, $"/skin {skin}", target);
    }

    [Command(["objetos"], "Staff", "Abre o painel de gerenciamento de objetos")]
    public static void CMD_objetos(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Events))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffObject:Show", GetObjectsJson());
    }

    [RemoteEvent(nameof(StaffObjectGoto))]
    public static void StaffObjectGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Events))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var adminObject = Global.AdminObjects.FirstOrDefault(x => x.Id == id);
            if (adminObject is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetPosition(new(adminObject.PosX, adminObject.PosY, adminObject.PosZ), adminObject.Dimension, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffObjectRemove))]
    public async Task StaffObjectRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Events))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var adminObject = Global.AdminObjects.FirstOrDefault(x => x.Id == id);
            if (adminObject is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.AdminObjects.Remove(adminObject);
            await context.SaveChangesAsync();
            adminObject.DeleteObject();
            Global.AdminObjects.Remove(adminObject);

            await player.WriteLog(LogType.Staff, $"Remover Objeto | {Functions.Serialize(adminObject)}", null);
            player.SendNotification(NotificationType.Success, "Objeto excluído.");
            CMD_objetos(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffObjectAdd))]
    public static void StaffObjectAdd(Player playerParam, string model)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Events))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            player.DropAdminObjectModel = model;
            player.DropAdminObjectId = null;
            player.Emit("DropObject", player.DropAdminObjectModel, 3);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffObjectEdit))]
    public static void StaffObjectEdit(Player playerParam, string id)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Events))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var adminObject = Global.AdminObjects.FirstOrDefault(x => x.Id == id.ToGuid());
            if (adminObject is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            adminObject.DeleteObject();
            player.DropAdminObjectModel = adminObject.Model;
            player.DropAdminObjectId = adminObject.Id;
            player.Emit("DropObject", player.DropAdminObjectModel, 3,
                new Vector3(adminObject.PosX, adminObject.PosY, adminObject.PosZ),
                new Vector3(adminObject.RotR, adminObject.RotP, adminObject.RotY));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CancelDropStaffObject))]
    public static void CancelDropStaffObject(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Events))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var adminObject = Global.AdminObjects.FirstOrDefault(x => x.Id == player.DropAdminObjectId);
            adminObject?.CreateObject();

            player.DropAdminObjectModel = null;
            player.DropAdminObjectId = null;
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmDropStaffObject))]
    public async Task ConfirmDropStaffObject(Player playerParam, Vector3 position, Vector3 rotation)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Events))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var adminObject = Global.AdminObjects.FirstOrDefault(x => x.Id == player.DropAdminObjectId);
            var isNew = adminObject is null;
            if (isNew)
            {
                adminObject = new();
                adminObject.Create(player.DropAdminObjectModel!, player.GetDimension(),
                    position.X, position.Y, position.Z,
                    rotation.X, rotation.Y, rotation.Z);
            }
            else
            {
                adminObject!.Update(player.DropAdminObjectModel!, player.GetDimension(),
                    position.X, position.Y, position.Z,
                    rotation.X, rotation.Y, rotation.Z);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.AdminObjects.AddAsync(adminObject);
            else
                context.AdminObjects.Update(adminObject);

            await context.SaveChangesAsync();

            if (isNew)
                Global.AdminObjects.Add(adminObject);

            adminObject.CreateObject();
            player.DropAdminObjectModel = null;
            player.DropAdminObjectId = null;
            await player.WriteLog(LogType.Staff, $"Gravar Objeto | {Functions.Serialize(adminObject)}", null);
            player.SendNotification(NotificationType.Success, $"Objeto {(isNew ? "criado" : "editado")}.");
            CMD_objetos(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetObjectsJson()
    {
        return Functions.Serialize(Global.AdminObjects
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
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
            }));
    }
}