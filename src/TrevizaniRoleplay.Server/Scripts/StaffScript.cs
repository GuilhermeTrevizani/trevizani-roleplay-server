using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffScript : Script
{
    [Command(["pos"], "Staff", "Vai até a posição", "(x) (y) (z)")]
    public async Task CMD_pos(MyPlayer player, float x, float y, float z)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.SetPosition(new(x, y, z), 0, false);
        await player.WriteLog(LogType.Staff, $"/pos {x} {y} {z}", null);
    }

    [Command(["ooc"], "Staff", "Chat OOC Global", "(mensagem)", GreedyArg = true)]
    public async Task CMD_ooc(MyPlayer player, string message)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        foreach (var target in Global.SpawnedPlayers)
            target.SendMessage(MessageType.None, $"(( {player.User.Name}: {message} ))", "#96a7cd");

        await player.WriteLog(LogType.GlobalOOCChat, message, null);
    }

    [Command(["waypoint"], "Staff", "Teleporta até o waypoint marcado no mapa")]
    public static void CMD_waypoint(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("Waypoint");
    }

    [Command(["limparchatgeral"], "Staff", "Limpa o chat de todos os personagens")]
    public async Task CMD_limparchatgeral(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.HeadAdmin)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        Global.Announcements = [];

        foreach (var x in Global.SpawnedPlayers)
        {
            x.ClearChat();
            x.SendNotification(NotificationType.Success, $"{player.User.Name} limpou o chat de todos.");
        }

        await player.WriteLog(LogType.Staff, "/limparchatgeral", null);
    }

    [Command(["areparar"], "Staff", "Conserta um veículo", "(veículo)")]
    public async Task CMD_areparar(MyPlayer player, uint? id)
    {
        if (!player.StaffFlags.Contains(StaffFlag.VehicleMaintenance))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (!id.HasValue)
        {
            if (!player.IsInVehicle)
            {
                player.SendMessage(MessageType.Error, "Você não está em um veículo.");
                return;
            }

            id = player.Vehicle?.Id;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Veículo {id} não está spawnado.");
            return;
        }

        vehicle.RepairEx();
        if (vehicle.SpawnType == MyVehicleSpawnType.Normal)
            await player.WriteLog(LogType.Staff, $"/areparar {vehicle.Identifier}", null);
        await Functions.SendServerMessage($"{player.User.Name} reparou o veículo {vehicle.Identifier}.", UserStaff.GameAdmin, false);
    }

    [Command(["pp"], "Staff", "Adiciona Premium Points para um usuário", "(usuário) (quantidade)")]
    public async Task CMD_pp(MyPlayer player, string userName, int quantity)
    {
        if (player.User.Staff < UserStaff.Management)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (quantity <= 0)
        {
            player.SendMessage(MessageType.Error, "Quantidade deve ser maior que 0.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUsername.ToLower() == userName.ToLower());
        if (user is null)
        {
            player.SendMessage(MessageType.Error, $"Usuário {userName} não existe.");
            return;
        }

        user.AddPremiumPoints(quantity);

        var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == user.Id);
        if (target is not null)
        {
            target.User = user;
            target.SendMessage(MessageType.Success, $"{player.User!.Name} deu para você {quantity} Premium Points.");
        }
        else
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        player.SendMessage(MessageType.Success, $"Você deu {quantity} Premium Points para {user.Name}.");
        await player.WriteLog(LogType.Staff, $"/pp {user.Id} {user.Name} {quantity}", target);
    }

    [Command(["ir"], "Staff", "Vai a um personagem", "(ID ou nome)")]
    public async Task CMD_ir(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        var pos = target.GetPosition();
        pos.X += 0.5f;
        player.SetPosition(pos, target.GetDimension(), false);
        await player.WriteLog(LogType.Staff, "/ir", target);
    }

    [Command(["trazer"], "Staff", "Traz um personagem", "(ID ou nome)")]
    public async Task CMD_trazer(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        var pos = player.GetPosition();
        pos.X += 0.5f;
        target.SetPosition(pos, player.GetDimension(), false);
        await player.WriteLog(LogType.Staff, "/trazer", target);
    }

    [Command(["tp"], "Staff", "Teleporta um personagem para outro", "(ID ou nome) (ID ou nome)")]
    public async Task CMD_tp(MyPlayer player, string idOrName, string idOrNameDestino)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        var targetDest = player.GetCharacterByIdOrName(idOrNameDestino, false);
        if (targetDest is null)
            return;

        var pos = targetDest.GetPosition();
        pos.X += 0.5f;
        target.SetPosition(pos, targetDest.GetDimension(), false);

        target.SendMessage(MessageType.Success, $"{player.User.Name} teleportou você para {targetDest.Character.Name}.");
        player.SendMessage(MessageType.Success, $"Você teleportou {target.Character.Name} para {targetDest.Character.Name}.");
        await player.WriteLog(LogType.Staff, $"/tp {target.Character.Name} {targetDest.Character.Name}", null);
    }

    [Command(["a"], "Staff", "Envia mensagem no chat administrativo", "(mensagem)", GreedyArg = true)]
    public async Task CMD_a(MyPlayer player, string message)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (player.StaffChatToggle)
        {
            player.SendMessage(MessageType.Error, "Você está com o chat da staff desabilitado.");
            return;
        }

        var sendMessage = $"[ADMIN CHAT] {player.User.Name} ({player.SessionId}): {message}";

        foreach (var x in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.GameAdmin && !x.StaffChatToggle))
            x.SendMessage(MessageType.None, sendMessage, Constants.STAFF_CHAT_COLOR);

        await player.WriteLog(LogType.StaffChat, message, null);
    }

    [Command(["hs"], "Staff", "Envia mensagem no chat head staff", "(mensagem)", GreedyArg = true)]
    public async Task CMD_hs(MyPlayer player, string message)
    {
        if (player.User.Staff < UserStaff.HeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (player.StaffChatToggle)
        {
            player.SendMessage(MessageType.Error, "Você está com o chat da staff desabilitado.");
            return;
        }

        var sendMessage = $"[HEAD CHAT] {player.User.Name} ({player.SessionId}): {message}";

        foreach (var x in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.HeadAdmin && !x.StaffChatToggle))
            x.SendMessage(MessageType.None, sendMessage, "#2386c9");

        await player.WriteLog(LogType.StaffChat, message, null);
    }

    [Command(["sc"], "Staff", "Envia mensagem no chat support", "(mensagem)", GreedyArg = true)]
    public async Task CMD_sc(MyPlayer player, string message)
    {
        if (player.User.Staff < UserStaff.Tester)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (player.StaffChatToggle)
        {
            player.SendMessage(MessageType.Error, "Você está com o chat da staff desabilitado.");
            return;
        }

        var sendMessage = $"[SUPPORT CHAT] {player.User.Name} ({player.SessionId}): {message}";

        foreach (var x in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.Tester && !x.StaffChatToggle))
            x.SendMessage(MessageType.None, sendMessage, "#86988b");

        await player.WriteLog(LogType.StaffChat, message, null);
    }

    [Command(["kick"], "Staff", "Expulsa um personagem", "(ID ou nome) (motivo)", GreedyArg = true)]
    public async Task CMD_kick(MyPlayer player, string idOrName, string reason)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.User.Staff >= player.User.Staff)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var punishment = new Punishment();
        punishment.Create(PunishmentType.Kick, 0, target.Character.Id, reason, player.User.Id);
        var context = Functions.GetDatabaseContext();
        await context.Punishments.AddAsync(punishment);
        await context.SaveChangesAsync();

        await target.Save();
        await Functions.SendServerMessage($"{player.User.Name} kickou {target.Character.Name}. Motivo: {reason}", UserStaff.None, false);
        target.KickEx($"{player.User.Name} kickou você. Motivo: {reason}");
    }

    [Command(["irveh"], "Staff", "Vai a um veículo", "(veículo)")]
    public async Task CMD_irveh(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var veh = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (veh is null)
        {
            player.SendMessage(MessageType.Error, "Veículo não está spawnado.");
            return;
        }

        var pos = veh.GetPosition();
        pos.X += 0.5f;
        player.SetPosition(pos, veh.GetDimension(), false);
        await player.WriteLog(LogType.Staff, $"/irveh {veh.Identifier}", null);
    }

    [Command(["trazerveh"], "Staff", "Traz um veículo", "(veículo)")]
    public async Task CMD_trazerveh(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Veículo não está spawnado.");
            return;
        }

        var pos = player.GetPosition();
        pos.X += 0.5f;
        vehicle.SetDimension(player.GetDimension());
        vehicle.SetPosition(pos);
        await player.WriteLog(LogType.Staff, $"/trazerveh {vehicle.Identifier}", null);
    }

    [Command(["aduty", "atrabalho"], "Staff", "Entra/sai de serviço administrativo")]
    public async Task CMD_aduty(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        if (player.OnAdminDuty)
        {
            player.AdminDutySession!.End();
            context.Sessions.Update(player.AdminDutySession);
            player.AdminDutySession = null;
        }
        else
        {
            player.AdminDutySession = new Session();
            player.AdminDutySession.Create(player.Character.Id, SessionType.StaffDuty, player.RealIp, player.RealSocialClubName);
            await context.Sessions.AddAsync(player.AdminDutySession);
        }
        await context.SaveChangesAsync();

        player.OnAdminDuty = !player.OnAdminDuty;
        player.Invincible = player.OnAdminDuty;
        player.SetNametag();
        player.Emit("ToggleAduty", player.OnAdminDuty);
        await Functions.SendServerMessage($"{player.User.Name} {(player.OnAdminDuty ? "entrou em" : "saiu de")} serviço administrativo.", UserStaff.GameAdmin, false);
    }

    [Command(["at"], "Staff", "Atende um SOS", "(ID)")]
    public async Task CMD_at(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.Tester)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == id && x.Type == HelpRequestType.SOS);
        if (helpRequest is null)
        {
            player.SendMessage(MessageType.Error, $"SOS {id} não existe.");
            return;
        }

        var target = await helpRequest.Check();
        if (target is null)
        {
            player.SendMessage(MessageType.Error, "Jogador do SOS não está mais conectado.");
            return;
        }

        if (target == player)
        {
            await Functions.SendServerMessage($"{player.User.Name} tentou aceitar o próprio SOS.", UserStaff.Tester, false);
            return;
        }

        helpRequest.Answer(player.User.Id);

        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();
        Global.HelpRequests.Remove(helpRequest);

        player.User.AddHelpRequestsAnswersQuantity();

        await Functions.SendServerMessage($"{player.User.Name} está respondendo o {helpRequest.Type.GetDescription()} de {helpRequest.CharacterName} ({helpRequest.CharacterSessionId}) ({helpRequest.UserName}).", UserStaff.Tester, false);
        player.LastPMSessionId = helpRequest.CharacterSessionId;
        target.SendMessage(MessageType.Success, $"{player.User.Name} atendeu o seu {helpRequest.Type.GetDescription()}: {helpRequest.Message}");
    }

    [Command(["ar"], "Staff", "Atende um report", "(ID)")]
    public async Task CMD_ar(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == id && x.Type == HelpRequestType.Report);
        if (helpRequest is null)
        {
            player.SendMessage(MessageType.Error, $"Report {id} não existe.");
            return;
        }

        var target = await helpRequest.Check();
        if (target is null)
        {
            player.SendMessage(MessageType.Error, "Jogador do report não está mais conectado.");
            return;
        }

        if (target == player)
        {
            await Functions.SendServerMessage($"{player.User.Name} tentou aceitar o próprio report.", UserStaff.Tester, false);
            return;
        }

        helpRequest.Answer(player.User.Id);

        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();
        Global.HelpRequests.Remove(helpRequest);

        player.User.AddHelpRequestsAnswersQuantity();

        await Functions.SendServerMessage($"{player.User.Name} está respondendo o {helpRequest.Type.GetDescription()} de {helpRequest.CharacterName} ({helpRequest.CharacterSessionId}) ({helpRequest.UserName}).", UserStaff.GameAdmin, false);
        player.LastPMSessionId = helpRequest.CharacterSessionId;
        target.SendMessage(MessageType.Success, $"{player.User.Name} atendeu o seu {helpRequest.Type.GetDescription()}: {helpRequest.Message}");
    }

    [Command(["listasos"], "Staff", "Lista os SOS pendentes")]
    public static void CMD_listasos(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.Tester)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var helpRequests = Global.HelpRequests.Where(x => x.Type == HelpRequestType.SOS).ToList();
        if (helpRequests.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Não há SOS pendente.");
            return;
        }

        foreach (var helpRequest in helpRequests)
        {
            player.SendMessage(MessageType.Error, $"{helpRequest.Type.GetDescription()} de {helpRequest.CharacterName} ({helpRequest.CharacterSessionId}) ({helpRequest.UserName}) em {helpRequest.RegisterDate}");
            player.SendMessage(MessageType.None, $"{helpRequest.Message} {{{Constants.ERROR_COLOR}}}(/at {helpRequest.CharacterSessionId} /csos {helpRequest.CharacterSessionId})", "#B0B0B0");
        }
    }

    [Command(["listareport"], "Staff", "Lista os reports pendentes")]
    public static void CMD_listareport(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var helpRequests = Global.HelpRequests.Where(x => x.Type == HelpRequestType.Report).ToList();
        if (helpRequests.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Não há report pendente.");
            return;
        }

        foreach (var helpRequest in helpRequests)
        {
            player.SendMessage(MessageType.Error, $"{helpRequest.Type.GetDescription()} de {helpRequest.CharacterName} ({helpRequest.CharacterSessionId}) ({helpRequest.UserName}) em {helpRequest.RegisterDate}");
            player.SendMessage(MessageType.None, $"{helpRequest.Message} {{{Constants.ERROR_COLOR}}}(/ar {helpRequest.CharacterSessionId} /creport {helpRequest.CharacterSessionId})", "#B0B0B0");
        }
    }

    [Command(["csos"], "Staff", "Converte um SOS para report", "(ID)")]
    public async Task CMD_csos(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.Tester)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == id && x.Type == HelpRequestType.SOS);
        if (helpRequest is null)
        {
            player.SendMessage(MessageType.Error, $"SOS {id} não existe.");
            return;
        }

        var target = await helpRequest.Check();
        if (target is null)
        {
            player.SendMessage(MessageType.Error, "Jogador do SOS não está mais conectado.");
            return;
        }

        helpRequest.SetType(HelpRequestType.Report);

        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();

        target.SendMessage(MessageType.Error, $"{player.User.Name} converteu o seu {HelpRequestType.SOS.GetDescription()} para um {helpRequest.Type.GetDescription()}.");
        await Functions.SendServerMessage($"{player.User.Name} converteu o {HelpRequestType.SOS.GetDescription()} de {helpRequest.CharacterName} ({helpRequest.CharacterSessionId}) ({helpRequest.UserName}) para um {helpRequest.Type.GetDescription()}.", UserStaff.Tester, false);
    }

    [Command(["creport"], "Staff", "Converte um report para SOS", "(ID)")]
    public async Task CMD_creport(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == id && x.Type == HelpRequestType.Report);
        if (helpRequest is null)
        {
            player.SendMessage(MessageType.Error, $"Report {id} não existe.");
            return;
        }

        var target = await helpRequest.Check();
        if (target is null)
        {
            player.SendMessage(MessageType.Error, "Jogador do report não está mais conectado.");
            return;
        }

        helpRequest.SetType(HelpRequestType.SOS);

        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();

        target.SendMessage(MessageType.Error, $"{player.User.Name} converteu o seu {HelpRequestType.Report.GetDescription()} para um {helpRequest.Type.GetDescription()}.");
        await Functions.SendServerMessage($"{player.User.Name} converteu o {HelpRequestType.Report.GetDescription()} de {helpRequest.CharacterName} ({helpRequest.CharacterSessionId}) ({helpRequest.UserName}) para um {helpRequest.Type.GetDescription()}.", UserStaff.Tester, false);
    }

    [Command(["spec"], "Staff", "Observa um personagem", "(ID ou nome)")]
    public async Task CMD_spec(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.SPECPosition is not null)
        {
            player.SendMessage(MessageType.Error, "Jogador está observando outro jogador.");
            return;
        }

        if (player.SPECPosition is not null)
        {
            player.Detach();
        }
        else
        {
            player.SPECPosition = player.GetPosition();
            player.SPECDimension = player.GetDimension();
            foreach (var x in Global.SpawnedPlayers.Where(x => x.SPECId == player.SessionId))
                await x.Unspectate();
        }

        player.SetCurrentWeapon((uint)WeaponModel.Fist);
        player.SPECId = target.SessionId;
        player.Visible = false;
        player.Invincible = true;
        player.SetNametag();
        await Task.Delay(1000);
        player.SetPosition(target.GetPosition() + new Vector3(5, 5, 30), target.GetDimension(), true);
        player.AttachToPlayer(target, 0, new(0, 0, 5), new(0, 0, 0));
        player.Frozen = true;
        player.Emit("SpectatePlayer", target, true);
        await player.WriteLog(LogType.Staff, "/spec", target);
        await Functions.SendServerMessage($"{player.User.Name} começou a observar {target.Character.Name}.",
            GetStaffSpec(player.User.Staff), false);
    }

    private static UserStaff GetStaffSpec(UserStaff staff)
    {
        if (staff >= UserStaff.Management)
            return UserStaff.Management;

        if (staff >= UserStaff.HeadAdmin)
            return UserStaff.HeadAdmin;

        return UserStaff.GameAdmin;
    }

    [Command(["specoff"], "Staff", "Para de observar um personagem")]
    public static async Task CMD_specoff(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (player.SPECPosition is null)
        {
            player.SendMessage(MessageType.Error, "Você não está observando um jogador.");
            return;
        }

        await player.Unspectate();
        player.Emit("SpectatePlayer", player, false);
    }

    [Command(["specs"], "Staff", "Lista quem está observando um personagem")]
    public async Task CMD_specs(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var targets = Global.SpawnedPlayers
            .Where(x => x.SPECId.HasValue && player.User.Staff >= GetStaffSpec(x.User.Staff))
            .ToList();
        if (targets.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Nenhuma observação em andamento.");
            return;
        }

        foreach (var target in targets)
        {
            var specTarget = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == target.SPECId);
            player.SendMessage(MessageType.None, $"{target.User.Name} está observando {specTarget?.Character?.Name}.");
        }

        await player.WriteLog(LogType.Staff, "/specs", null);
    }

    [Command(["aferimentos"], "Staff", "Visualiza os ferimentos de um personagem", "(ID ou nome)")]
    public static void CMD_aferimentos(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (target.Wounds.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Jogador não possui ferimentos.");
            return;
        }

        player.Emit("ViewCharacterWounds", target.ICName, Functions.Serialize(
            target.Wounds.OrderByDescending(x => x.Date)
            .Select(x => new
            {
                x.Date,
                x.Weapon,
                x.Damage,
                x.BodyPart,
                Author = x.Attacker,
                x.Distance,
            })), true);
    }

    [Command(["aestacionar"], "Staff", "Estaciona um veículo", "(veículo)")]
    public async Task CMD_aestacionar(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Veículo não está spawnado.");
            return;
        }

        if (vehicle.VehicleDB.CharacterId is not null && vehicle.VehicleDB.Items!.Any(x =>
            (x.GetCategory() == ItemCategory.Weapon && Functions.IsWeaponWithAmmo(x.GetItemType()))
                || x.GetCategory() == ItemCategory.WeaponComponent
                || GlobalFunctions.CheckIfIsAmmo(x.GetCategory())
                || x.GetCategory() == ItemCategory.Drug))
        {
            player.SendMessage(MessageType.Error, "Você não pode estacionar o veículo com armas, componentes de armas, munições ou drogas.");
            return;
        }

        await Functions.SendServerMessage($"{player.User.Name} estacionou o veículo {vehicle.Identifier}.", UserStaff.GameAdmin, false);
        await vehicle.Park(player);
    }

    [Command(["acurar"], "Staff", "Cura um personagem ferido", "(ID ou nome)")]
    public async Task CMD_acurar(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
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

        if (!target.Wounded)
        {
            player.SendMessage(MessageType.Error, "Jogador não está ferido.");
            return;
        }

        target.Heal();
        player.SendMessage(MessageType.Success, $"Você curou {target.Character.Name}.");
        target.SendMessage(MessageType.Success, $"{player.User.Name} curou você.");

        await player.WriteLog(LogType.Staff, "/acurar", target);
    }

    [Command(["adanos"], "Staff", "Visualiza os danos de um veículo", "(veículo)")]
    public static void CMD_adanos(MyPlayer player, int id)
    {
        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhum veículo encontrado com o ID {id}.");
            return;
        }

        if (vehicle.Damages.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Veículo não possui danos.");
            return;
        }

        player.Emit("ViewVehicleDamages", vehicle.Identifier,
            Functions.Serialize(
                vehicle.Damages.OrderByDescending(x => x.Date)
                .Select(x => new
                {
                    x.Date,
                    x.Weapon,
                    x.BodyHealthDamage,
                    x.EngineHealthDamage,
                    Author = x.Attacker,
                    x.Distance,
                })), true);
    }

    [Command(["checarveh"], "Staff", "Visualiza o proprietário de um veículo", "(veículo)")]
    public async Task CMD_checarveh(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Veículo {id} não está spawnado.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        var owner = "N/A";
        if (vehicle.VehicleDB.FactionId.HasValue)
            owner = $"{Global.Factions.FirstOrDefault(x => x.Id == vehicle.VehicleDB.FactionId)!.Name}";
        else if (vehicle.VehicleDB.CharacterId.HasValue)
            owner = $"{(await context.Characters.FirstOrDefaultAsync(x => x.Id == vehicle.VehicleDB.CharacterId))!.Name}";
        else if (vehicle.RentExpirationDate.HasValue)
            owner = $"{vehicle.NameInCharge}{{#FFFFFF}} | Término Aluguel: {{{Constants.MAIN_COLOR}}}{vehicle.RentExpirationDate}";

        player.SendMessage(MessageType.None, $"Veículo: {{{Constants.MAIN_COLOR}}}{vehicle.VehicleDB.Id}{{#FFFFFF}} | Modelo: {{{Constants.MAIN_COLOR}}}{vehicle.VehicleDB.Model}{{#FFFFFF}} | Proprietário: {{{Constants.MAIN_COLOR}}}{owner}");
    }

    [Command(["proximo", "prox"], "Staff", "Lista os itens que estão próximos", "(distância)")]
    public static void CMD_proximo(MyPlayer player, float distance)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var hasAnythingNear = false;

        foreach (var x in Global.Blips)
        {
            if (player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Blip {x.Id}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Spots)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Ponto {x.Id} | Tipo: {x.Type.GetDescription()} ({(byte)x.Type})");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.FactionsStorages)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Arsenal {x.Id}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Vehicles)
        {
            if (x.GetDimension() == player.GetDimension()
                && player.GetPosition().DistanceTo(x.GetPosition()) <= distance)
            {
                player.SendMessage(MessageType.None, $"Veículo {x.VehicleDB.Id} | Modelo: {x.VehicleDB.Model.ToUpper()}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Doors)
        {
            if (player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Porta {x.Id}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Infos)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Info {x.Id} | Data: {x.RegisterDate} | Expiração: {x.ExpirationDate}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Companies)
        {
            if (player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Empresa {x.Id}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Items)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Item {x.Id} | Nome: {x.GetName()} | Quantidade: {x.Quantity} | Extra: {x.GetExtra()}");
                hasAnythingNear = true;
            }
        }

        foreach (var property in Global.Properties)
        {
            if (property.EntranceDimension == player.GetDimension()
                && player.GetPosition().DistanceTo(property.GetEntrancePosition()) <= distance)
            {
                player.SendMessage(MessageType.None, $"Propriedade {property.Number} | {property.FormatedAddress}");
                hasAnythingNear = true;
            }

            foreach (var entrance in property.Entrances!)
            {
                if (property.EntranceDimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(entrance.GetEntrancePosition()) <= distance)
                {
                    player.SendMessage(MessageType.None, $"Propriedade {property.Number} | {property.FormatedAddress}");
                    hasAnythingNear = true;
                }
            }

            foreach (var furniture in property.Furnitures!)
            {
                if ((furniture.Interior ? furniture.Property!.Number : furniture.Property!.EntranceDimension) == player.GetDimension()
                    && player.GetPosition().DistanceTo(new(furniture.PosX, furniture.PosY, furniture.PosZ)) <= distance)
                {
                    player.SendMessage(MessageType.None, $"Mobília {furniture.Id} | Modelo: {furniture.Model}");
                    hasAnythingNear = true;
                }
            }
        }

        foreach (var x in Global.Graffitis)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Grafite {x.Id} | Data: {x.RegisterDate} | Expiração: {x.ExpirationDate}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.Bodies)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Corpo {x.Id} | Data: {x.RegisterDate} | Nome: {x.Name}");
                hasAnythingNear = true;
            }
        }

        foreach (var x in Global.AdminObjects)
        {
            if (x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            {
                player.SendMessage(MessageType.None, $"Objeto Admin {x.Id} | Data: {x.RegisterDate} | Modelo: {x.Model}");
                hasAnythingNear = true;
            }
        }

        if (!hasAnythingNear)
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum item.");
    }

    [RemoteEvent(nameof(NoClip))]
    public async Task NoClip(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendNotification(NotificationType.Error, "Você não pode usar esse comando dentro de um veículo.");
                return;
            }

            if (!player.OnAdminDuty)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotOnAdministrativeDuty);
                return;
            }

            player.SetCurrentWeapon((uint)WeaponModel.Fist);
            player.Visible = player.NoClip;
            player.Invincible = !player.NoClip;
            player.NoClip = !player.NoClip;
            player.StartNoClip(false);
            player.SetNametag();
            await player.WriteLog(LogType.Staff, "NoClip", null);
            player.SendNotification(NotificationType.Success, $"Você {(!player.NoClip ? "des" : string.Empty)}ativou a câmera livre.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["raviso"], "Staff", "Remove o aviso mais recente de um personagem", "(nome do personagem)", GreedyArg = true)]
    public async Task CMD_raviso(MyPlayer player, string characterName)
    {
        if (player.User.Staff < UserStaff.HeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var character = await context.Characters
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Name == characterName);
        if (character is null)
        {
            player.SendMessage(MessageType.Error, $"Personagem {characterName} não encontrado.");
            return;
        }

        var lastWarn = await context.Punishments
            .Where(x => x.CharacterId == character.Id && x.Type == PunishmentType.Warn)
            .OrderByDescending(x => x.RegisterDate)
            .FirstOrDefaultAsync();
        if (lastWarn is null)
        {
            player.SendMessage(MessageType.Error, $"Personagem {characterName} não possui avisos.");
            return;
        }

        context.Punishments.Remove(lastWarn);
        await context.SaveChangesAsync();

        await player.WriteLog(LogType.Staff, $"/raviso {Functions.Serialize(lastWarn)}", null);
        await Functions.SendServerMessage($"{player.User.Name} removeu um aviso ({lastWarn.Reason}) de {character.Name}.", UserStaff.None, false);
    }

    [Command(["ajail"], "Staff", "Prende um personagem online administrativamente", "(ID ou nome) (minutos) (motivo)", GreedyArg = true)]
    public async Task CMD_ajail(MyPlayer player, string idOrName, int minutes, string reason)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (minutes <= 0)
        {
            player.SendMessage(MessageType.Error, "Tempo deve ser maior que 0.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.User!.AjailMinutes > 0)
        {
            player.SendNotification(NotificationType.Error, "Usuário já está na prisão administrativa.");
            return;
        }

        if (target.User.Staff >= player.User.Staff)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var punishment = new Punishment();
        punishment.Create(PunishmentType.Ajail, minutes, target.Character.Id, reason, player.User.Id);
        var context = Functions.GetDatabaseContext();
        await context.Punishments.AddAsync(punishment);
        await context.SaveChangesAsync();

        target.User.SetAjailMinutes(minutes);
        await Functions.SendServerMessage($"{player.User.Name} prendeu {target.Character.Name} por {minutes} minuto(s). Motivo: {reason}", UserStaff.None, false);
        await target.ListCharacters("Prisão Administrativa", $"{player.User.Name} prendeu você por {minutes} minuto(s). Motivo: {reason}");
    }

    [RemoteEvent(nameof(StaffGiveCharacterAjail))]
    public async Task StaffGiveCharacterAjail(Player playerParam, string idString, int minutes, string reason)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (minutes <= 0)
            {
                player.SendNotification(NotificationType.Error, "Tempo deve ser maior que 0.");
                return;
            }

            if (reason.Length < 1 || reason.Length > 500)
            {
                player.SendNotification(NotificationType.Error, "Motivo deve ter entre 1 e 500 caracteres.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, "Personagem não encontrado.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is not null)
            {
                await CMD_ajail(player, target.SessionId.ToString(), minutes, reason);
            }
            else
            {
                if (character.User!.AjailMinutes > 0)
                {
                    player.SendNotification(NotificationType.Error, "Usuário já está na prisão administrativa.");
                    return;
                }

                if (character.User!.Staff >= player.User.Staff)
                {
                    player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                    return;
                }

                var punishment = new Punishment();
                punishment.Create(PunishmentType.Ajail, minutes, character.Id, reason, player.User.Id);
                await context.Punishments.AddAsync(punishment);

                character.User.SetAjailMinutes(minutes);
                await context.SaveChangesAsync();

                await Functions.SendServerMessage($"{player.User.Name} prendeu {character.Name} por {minutes} minuto(s). Motivo: {reason}", UserStaff.None, false);
            }

            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["rajail"], "Staff", "Solta um personagem da prisão administrativa", "(nome do personagem)", GreedyArg = true)]
    public async Task CMD_rajail(MyPlayer player, string characterName)
    {
        if (player.User.Staff < UserStaff.HeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var character = await context.Characters
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Name == characterName);
        if (character is null)
        {
            player.SendMessage(MessageType.Error, $"Personagem {characterName} não encontrado.");
            return;
        }

        if (character.User!.AjailMinutes == 0)
        {
            player.SendMessage(MessageType.Error, $"Usuário {character.User.Name} não está preso administrativamente.");
            return;
        }

        var lastAjail = await context.Punishments
            .Where(x => x.CharacterId == character.Id && x.Type == PunishmentType.Ajail)
            .OrderByDescending(x => x.RegisterDate)
            .FirstOrDefaultAsync();
        if (lastAjail is not null)
            context.Punishments.Remove(lastAjail);

        var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == character.UserId);
        if (target is not null)
        {
            target.Timer?.Stop();
            target.Timer = null;
            target.User.SetAjailMinutes(0);
            await target.ListCharacters(string.Empty, string.Empty);
        }
        else
        {
            character.User.SetAjailMinutes(0);
            context.Users.Update(character.User);
        }

        await context.SaveChangesAsync();

        await player.WriteLog(LogType.Staff, $"/rajail {character.Id} {(lastAjail is not null ? Functions.Serialize(lastAjail) : string.Empty)}", null);
        await Functions.SendServerMessage($"{player.User.Name} soltou {character.Name} da prisão administrativa.", UserStaff.None, false);
    }

    [Command(["vflip"], "Staff", "Descapota um veículo", "(veículo)")]
    public async Task CMD_vflip(MyPlayer player, int id)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Veículo não está spawnado.");
            return;
        }

        vehicle.Rotation = new(0, vehicle.Rotation.Y, vehicle.Rotation.Z);
        await player.WriteLog(LogType.Staff, $"/vflip {vehicle.Identifier}", null);
        await Functions.SendServerMessage($"{player.User.Name} descapotou o veículo {vehicle.Identifier}.", UserStaff.GameAdmin, false);
    }

    [Command(["amotor"], "Staff", "Liga/desliga o motor de um veículo")]
    public async Task CMD_amotor(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.VehicleMaintenance))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo.");
            return;
        }

        await Functions.SendServerMessage($"{player.User.Name} {(vehicle.GetEngineStatus() ? "des" : string.Empty)}ligou o motor do veículo {vehicle.Identifier}.", UserStaff.GameAdmin, false);
        vehicle.SetEngineStatus(!vehicle.GetEngineStatus());
        await player.WriteLog(LogType.Staff, $"/amotor {vehicle.Identifier}", null);
    }

    [Command(["aabastecer"], "Staff", "Abastece um veículo")]
    public async Task CMD_aabastecer(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.VehicleMaintenance))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo.");
            return;
        }

        vehicle.SetFuel(vehicle.VehicleDB.GetMaxFuel());

        await player.WriteLog(LogType.Staff, $"/aabastecer {vehicle.Identifier}", null);
        await Functions.SendServerMessage($"{player.User.Name} abasteceu o veículo {vehicle.Identifier}.", UserStaff.GameAdmin, false);
    }

    [Command(["aveiculo"], "Staff", "Cria um veículo temporário", "(modelo)")]
    public async Task CMD_aveiculo(MyPlayer player, string model)
    {
        if (!player.StaffFlags.Contains(StaffFlag.VehicleMaintenance))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você está em um veículo.");
            return;
        }

        if (!GlobalFunctions.CheckIfVehicleExists(model))
        {
            player.SendMessage(MessageType.Error, $"Modelo {model} não existe.");
            return;
        }

        model = model.ToUpper();

        var vehicle = Functions.CreateVehicle(model, player.GetPosition(), player.GetRotation(), MyVehicleSpawnType.Admin);
        vehicle.PearlescentColor = 0;
        vehicle.Dimension = player.GetDimension();
        vehicle.NumberPlate = "ADMIN";
        player.SetIntoVehicleEx(vehicle, Constants.VEHICLE_SEAT_DRIVER);
        vehicle.SetEngineStatus(true);
        await player.WriteLog(LogType.Staff, $"/aveiculo {model}", null);
        await Functions.SendServerMessage($"{player.User.Name} criou o veículo {model}.", UserStaff.GameAdmin, false);
    }

    [Command(["alteracoesplaca"], "Staff", "Lista as solicitações de alterações de placa")]
    public async Task CMD_alteracoesplaca(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.LeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var vehicles = await context.Vehicles
            .Where(x => !string.IsNullOrWhiteSpace(x.NewPlate))
            .Select(x => new
            {
                x.Model,
                x.NewPlate,
                User = x.Character!.User!.Name,
            })
            .ToListAsync();
        if (vehicles.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Não há solicitações de alterações de placa pendentes.");
            return;
        }

        player.SendMessage(MessageType.Title, "Solicitações de alterações de placa");
        foreach (var vehicle in vehicles)
            player.SendMessage(MessageType.None, $"Placa {vehicle.NewPlate.ToUpper()} de {vehicle.Model.ToUpper()} por {vehicle.User}");
    }

    [Command(["aprovarplaca"], "Staff", "Aprova uma alteração de placa", "(placa)")]
    public async Task CMD_aprovarplaca(MyPlayer player, string plate)
    {
        if (player.User.Staff < UserStaff.LeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var vehicle = await context.Vehicles
            .Include(x => x.Character)
                .ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.NewPlate == plate);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, Resources.RecordNotFound);
            return;
        }

        if (vehicle.Character!.User!.PlateChanges == 0)
        {
            player.SendMessage(MessageType.Error, $"{vehicle.Character.User.Name} não possui mais trocas de placa.");
            return;
        }

        var oldPlate = vehicle.Plate;

        var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == vehicle.Id);
        if (veh is null)
        {
            vehicle.SetPlate(plate.ToUpper());
            vehicle.SetNewPlate(string.Empty);
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync();
        }
        else
        {
            veh.VehicleDB.SetPlate(plate.ToUpper());
            veh.VehicleDB.SetNewPlate(string.Empty);
            context.Vehicles.Update(veh.VehicleDB);
            await context.SaveChangesAsync();
            veh.NumberPlate = veh.VehicleDB.Plate.ToUpper();
        }

        var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == vehicle.Character.UserId);
        if (target is null)
        {
            vehicle.Character.User.RemovePlateChanges();
            context.Users.Update(vehicle.Character.User);
            await context.SaveChangesAsync();
        }
        else
        {
            target.User.RemovePlateChanges();
            await target.Save();
            target.SendMessage(MessageType.Success, $"{player.User.Name} aprovou sua placa {plate.ToUpper()}.");
        }

        await Functions.SendServerMessage($"{player.User.Name} aprovou a placa {plate.ToUpper()}.", UserStaff.LeadAdmin, false);
        await player.WriteLog(LogType.PlateChange, $"Aprovou placa {vehicle.Id} {vehicle.Model} {oldPlate.ToUpper()} > {plate.ToUpper()} de {vehicle.Character.Name}", null);
    }

    [Command(["reprovarplaca"], "Staff", "Reprova uma alteração de placa", "(placa)")]
    public async Task CMD_reprovarplaca(MyPlayer player, string plate)
    {
        if (player.User.Staff < UserStaff.LeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var vehicle = await context.Vehicles
            .FirstOrDefaultAsync(x => x.NewPlate == plate);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, Resources.RecordNotFound);
            return;
        }

        var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == vehicle.Id);
        if (veh is null)
        {
            vehicle.SetNewPlate(string.Empty);
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync();
        }
        else
        {
            veh.VehicleDB.SetNewPlate(string.Empty);
            context.Vehicles.Update(veh.VehicleDB);
            await context.SaveChangesAsync();
        }

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == vehicle.CharacterId);
        target?.SendMessage(MessageType.Success, $"{player.User.Name} reprovou sua placa {plate.ToUpper()}.");

        await Functions.SendServerMessage($"{player.User.Name} reprovou a placa {plate.ToUpper()}.", UserStaff.LeadAdmin, false);
        await player.WriteLog(LogType.PlateChange, $"Reprovou placa {vehicle.Id} {vehicle.Model} {plate.ToUpper()}", null);
    }

    [Command(["usuario"], "Staff", "Visualiza as informações de um usuário", "(nome)")]
    public async Task CMD_usuario(MyPlayer player, string name)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUsername.ToLower() == name.ToLower());
        if (user is null)
        {
            player.SendNotification(NotificationType.Error, $"Nenhum usuário encontrado com o nome {name}.");
            return;
        }

        var characters = await context.Characters
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync();

        var punishments = await context.Punishments
            .Include(x => x.Character)
            .Include(x => x.StaffUser)
            .Where(x => x.Character!.UserId == user.Id)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync();

        var staffFlagJson = Functions.Serialize(
            Enum.GetValues<StaffFlag>()
            .Select(x => new
            {
                Value = x,
                Label = x.GetDescription(),
            })
            .OrderBy(x => x.Label)
        );

        var userStaffJson = Functions.Serialize(
            Enum.GetValues<UserStaff>()
            .Select(x => new
            {
                Value = x,
                Label = x.GetDescription(),
            })
        );

        player.Emit("StaffSearchUser:Show", userStaffJson, staffFlagJson, (int)player.User.Staff,
            Functions.Serialize(new
            {
                user.Id,
                Name = user.DiscordUsername,
                user.Staff,
                StaffFlags = Functions.Deserialize<StaffFlag[]>(user.StaffFlagsJSON),
                Characters = characters.Select(y => new
                {
                    Name = $"{y.Name}{(y.DeletedDate.HasValue ? $" (EXCLUÍDO EM {y.DeletedDate})" : string.Empty)}",
                }),
                Punishments = punishments.Select(y => new
                {
                    Character = y.Character!.Name,
                    y.RegisterDate,
                    Type = y.Type.GetDescription(),
                    y.Duration,
                    Staff = y.StaffUser!.Name,
                    y.Reason
                })
            }));
    }

    [RemoteEvent(nameof(StaffSaveUser))]
    public async Task StaffSaveUser(Player playerParam, string idString, int staffValue, string flagsJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.Management)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (!Enum.IsDefined(typeof(UserStaff), Convert.ToByte(staffValue)))
            {
                player.SendNotification(NotificationType.Error, $"Staff {staffValue} não existe.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhum usuário encontrado com o código {id}.");
                return;
            }

            var staff = (UserStaff)staffValue;
            if ((player.User.Staff <= user.Staff && player.User.Id != user.Id) || player.User.Staff < staff)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var staffFlags = Functions.Deserialize<List<StaffFlag>>(flagsJSON).ToList();
            if (staff == UserStaff.None)
                staffFlags = [];

            var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == user.Id);
            if (target is not null)
            {
                target.StaffFlags = staffFlags;
                target.User.SetStaff(staff, Functions.Serialize(staffFlags));
                context.Users.Update(target.User);
                await context.SaveChangesAsync();
                target.SendMessage(MessageType.Success, $"{player.User.Name} modificou suas configurações administrativas.");
            }
            else
            {
                user.SetStaff(staff, Functions.Serialize(staffFlags));
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            await player.WriteLog(LogType.Staff, $"Alterar Usuário {user.Name} ({user.DiscordUsername}) {staff} {Functions.Serialize(staffFlags.Select(x => x.GetDescription()))}", target);
            player.SendNotification(NotificationType.Success, $"Você alterou as configurações administrativas de {user.Name}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["checar"], "Staff", "Visualiza as informações de um personagem", "(ID ou nome)", GreedyArg = true)]
    public async Task CMD_checar(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, true, false);
        if (target is not null)
            idOrName = target.Character.Name;

        var context = Functions.GetDatabaseContext();
        var character = await context.Characters
            .Include(x => x.User)
            .Include(x => x.EvaluatorStaffUser)
            .Include(x => x.Items)
            .Include(x => x.Vehicles)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == idOrName.ToLower());
        if (character is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhum personagem encontrado com o nome {idOrName}.");
            return;
        }

        var banishment = await context.Banishments
            .Include(x => x.StaffUser)
            .FirstOrDefaultAsync(x => x.CharacterId == character.Id);

        player.Emit("StaffSearchCharacter:Show", (int)player.User.Staff, player.User.StaffFlagsJSON,
            Functions.Serialize(new
            {
                character.Id,
                character.Name,
                character.History,
                EvaluatorStaffUser = character.EvaluatorStaffUser!.Name,
                Faction = Global.Factions.FirstOrDefault(x => x.Id == character.FactionId)?.Name ?? "N/A",
                Rank = Global.FactionsRanks.FirstOrDefault(x => x.Id == character.FactionRankId)?.Name ?? "N/A",
                User = $"{character.User!.Name} ({character.User.DiscordUsername})",
                Banishment = banishment is null ? null : new
                {
                    banishment.RegisterDate,
                    banishment.Reason,
                    banishment.ExpirationDate,
                    Staff = banishment.StaffUser!.Name,
                    banishment.UserId,
                },
                Items = character.Items!.Select(x => new
                {
                    Name = x.GetName(),
                    x.Quantity,
                    Extra = x.GetExtra().Replace("<br/>", ", "),
                }),
                Properties = Global.Properties.Where(x => x.CharacterId == character.Id).Select(x => new
                {
                    x.Number,
                    x.Address,
                    x.Value,
                    x.ProtectionLevel,
                }),
                Vehicles = character.Vehicles!.Where(x => !x.Sold).Select(x => new
                {
                    x.Model,
                    x.Plate,
                    x.ProtectionLevel,
                    x.XMR,
                }),
                Companies = Global.Companies.Where(x => x.CharacterId == character.Id || x.Characters!.Any(y => y.CharacterId == character.Id)).Select(x => new
                {
                    x.Name,
                    Owner = x.CharacterId == character.Id,
                }),
                Job = character.Job.GetDescription(),
                character.RegisterDate,
                Premium = character.User!.GetCurrentPremium().GetDescription(),
                character.ConnectedTime,
                character.User.NameChanges,
                character.User.PlateChanges,
                character.Bank,
                character.NameChangeStatus,
                character.DeathDate,
                character.DeathReason,
                character.CKAvaliation,
                character.JailFinalDate,
                character.DeletedDate,
                character.User.NumberChanges,
                character.User.PremiumPoints,
                character.User.AjailMinutes,
                IP = target?.RealIp ?? character.LastAccessIp,
                SocialClubName = target?.RealSocialClubName ?? string.Empty,
            }));
    }

    [Command(["ban"], "Staff", "Bane um jogador", "(ID ou nome) (dias [0 para permanente]) (motivo)", GreedyArg = true)]
    public async Task CMD_ban(MyPlayer player, string idOrName, int days, string reason)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.User.Staff >= player.User.Staff)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        if (await context.Banishments.AnyAsync(x => x.CharacterId == target.Character.Id))
        {
            player.SendNotification(NotificationType.Error, "Personagem já está banido.");
            return;
        }

        var banishment = new Banishment();
        banishment.Create(days > 0 ? DateTime.Now.AddDays(days) : null, target.Character.Id, target.User.Id, reason, player.User.Id);
        await context.Banishments.AddAsync(banishment);

        var punishment = new Punishment();
        punishment.Create(PunishmentType.Ban, days, target.Character.Id, reason, player.User.Id);
        await context.Punishments.AddAsync(punishment);
        await context.SaveChangesAsync();

        await target.Save();
        var strBan = days == 0 ? "permanentemente" : $"por {days} dia{(days > 1 ? "s" : string.Empty)}";
        await Functions.SendServerMessage($"{player.User.Name} baniu {target.Character.Name} ({target.User.Name}) {strBan}. Motivo: {reason}", UserStaff.GameAdmin, false);
        target.KickEx($"{player.User.Name} baniu você {strBan}. Motivo: {reason}");
    }

    [RemoteEvent(nameof(StaffBanCharacter))]
    public async Task StaffBanCharacter(Player playerParam, string idString, int days, string reason)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                player.SendNotification(NotificationType.Error, "Motivo não informado.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, "Personagem não existe.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is not null)
            {
                await CMD_ban(player, target.SessionId.ToString(), days, reason);
            }
            else
            {
                target = Global.SpawnedPlayers.FirstOrDefault(x => x.User.Id == character.UserId);
                if (target is not null)
                {
                    player.SendNotification(NotificationType.Error, $"O usuário do personagem está online! Use /ban {target.SessionId}.");
                    return;
                }

                if (character.User!.Staff >= player.User.Staff)
                {
                    player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                    return;
                }

                if (await context.Banishments.AnyAsync(x => x.CharacterId == character.Id))
                {
                    player.SendNotification(NotificationType.Error, "Personagem já está banido.");
                    return;
                }

                var banishment = new Banishment();
                banishment.Create(days > 0 ? DateTime.Now.AddDays(days) : null, character.Id, character.User.Id, reason, player.User.Id);
                await context.Banishments.AddAsync(banishment);

                var punishment = new Punishment();
                punishment.Create(PunishmentType.Ban, days, character.Id, reason, player.User.Id);
                await context.Punishments.AddAsync(punishment);
                await context.SaveChangesAsync();

                var strBan = days == 0 ? "permanentemente" : $"por {days} dia{(days > 1 ? "s" : string.Empty)}";
                player.SendNotification(NotificationType.Success, $"Você baniu {character.Name} ({character.User.Name}) {strBan}. Motivo: {reason}");
            }

            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCKAvaliationRemoveCharacter))]
    public async Task StaffCKAvaliationRemoveCharacter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CK))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character == null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} não existe.");
                return;
            }

            if (!character.DeathDate.HasValue && !character.CKAvaliation)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} não está morto ou em avaliação de CK.");
                return;
            }

            character.RemoveDeath();
            context.Characters.Update(character);
            await context.SaveChangesAsync();
            await Functions.SendServerMessage($"{player.User.Name} removeu o CK / avaliação de CK do personagem {character.Name}.", UserStaff.None, false);
            await player.WriteLog(LogType.Staff, $"Remover Avaliação de CK / avaliação de CK do personagem {character.Id}", null);
            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCKCharacter))]
    public async Task StaffCKCharacter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CK))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} não existe.");
                return;
            }

            if (character.DeathDate.HasValue)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} já está morto.");
                return;
            }

            var reason = "CK aceito";

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == id);
            if (target is not null)
            {
                target.Character.SetDeath(reason);
                await target.Save();
                await target.ListCharacters("CK", $"{player.User.Name} aplicou CK no seu personagem. Motivo: {reason}");
            }
            else
            {
                character.SetDeath(reason);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }

            await Functions.SendServerMessage($"{player.User.Name} aplicou CK no personagem {character.Name}. Motivo: {reason}", UserStaff.None, false);
            await player.WriteLog(LogType.Staff, $"CK {character.Id} {reason}", target);
            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCKAvaliationCharacter))]
    public async Task StaffCKAvaliationCharacter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CK))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character == null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} não existe.");
                return;
            }

            if (character.DeathDate.HasValue)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} já está morto.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == id);
            if (target != null)
            {
                target.Character.SetCKAvaliation();
                await target.Save();
                await target.ListCharacters("Avaliação de CK", $"{player.User.Name} colocou seu personagem na avaliação de CK.");
            }
            else
            {
                character.SetCKAvaliation();
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }

            await Functions.SendServerMessage($"{player.User.Name} colocou o personagem {character.Name} na avaliação de CK.", UserStaff.None, false);
            await player.WriteLog(LogType.Staff, $"Avaliação de CK {character.Id}", target);
            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffNameChangeStatusCharacter))]
    public async Task StaffNameChangeStatusCharacter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.CK))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} não existe.");
                return;
            }

            if (character.NameChangeStatus == CharacterNameChangeStatus.Done)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} realizou a mudança de nome.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == id);
            if (target is not null)
            {
                target.Character.SetNameChangeStatus();
                await target.Save();
                target.SendMessage(MessageType.Success, $"{player.User.Name} {(target.Character.NameChangeStatus == CharacterNameChangeStatus.Allowed ? "des" : string.Empty)}bloqueou a troca de nome do seu personagem.");
            }
            else
            {
                character.SetNameChangeStatus();
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }

            player.SendNotification(NotificationType.Success, $"Você {(character.NameChangeStatus == CharacterNameChangeStatus.Allowed ? "des" : string.Empty)}bloqueou a troca de nome de {character.Name}.");
            await player.WriteLog(LogType.Staff, $"{(character.NameChangeStatus == CharacterNameChangeStatus.Allowed ? "Desbloquear" : "Bloquear")} Mudança de Nome {character.Id}", target);
            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffRemoveJailCharacter))]
    public async Task StaffRemoveJailCharacter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.HeadAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {id} não existe.");
                return;
            }

            if ((character.JailFinalDate ?? DateTime.MinValue) <= DateTime.Now)
            {
                player.SendNotification(NotificationType.Error, $"{character.Name} não está preso.");
                return;
            }

            var jail = await context.Jails.OrderByDescending(x => x.RegisterDate).LastOrDefaultAsync(x => x.CharacterId == character.Id);
            if (jail is null)
            {
                player.SendNotification(NotificationType.Error, $"Não foi possível encontrar o registro de prisão de {character.Name}. Por favor, reporte o bug.");
                return;
            }

            if (string.IsNullOrWhiteSpace(jail.Description))
            {
                context.Jails.Remove(jail);
                await context.SaveChangesAsync();
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is null)
            {
                character.SetJailFinalDate(null);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }
            else
            {
                target.RemoveFromJail();
            }

            player.SendNotification(NotificationType.Success, $"Você removeu {character.Name} da prisão.");
            await player.WriteLog(LogType.Staff, $"Remover da Prisão {character.Id}", null);
            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["aviso"], "Staff", "Aplica um aviso em um personagem online", "(ID ou nome) (motivo)", GreedyArg = true)]
    public async Task CMD_aviso(MyPlayer player, string idOrName, string reason)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.User.Staff >= player.User.Staff)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var punishment = new Punishment();
        punishment.Create(PunishmentType.Warn, 0, target.Character.Id, reason, player.User.Id);
        var context = Functions.GetDatabaseContext();
        await context.Punishments.AddAsync(punishment);
        await context.SaveChangesAsync();

        await Functions.SendServerMessage($"{player.User.Name} aplicou um aviso em {target.Character.Name}. Motivo: {reason}", UserStaff.None, false);

        var warns = await CountWarnsInLastMonth(target.User.Id);
        if (warns == 3)
            await CMD_ban(player, target.SessionId.ToString(), 0, "3 avisos");
    }

    [RemoteEvent(nameof(StaffGiveCharacterWarning))]
    public async Task StaffGiveCharacterWarning(Player playerParam, string idString, string reason)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.GameAdmin)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (reason.Length < 1 || reason.Length > 500)
            {
                player.SendNotification(NotificationType.Error, "Motivo deve ter entre 1 e 500 caracteres.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, "Personagem não encontrado.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is not null)
            {
                await CMD_aviso(player, target.SessionId.ToString(), reason);
            }
            else
            {
                if (character.User!.Staff >= player.User.Staff)
                {
                    player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                    return;
                }

                var punishment = new Punishment();
                punishment.Create(PunishmentType.Warn, 0, character.Id, reason, player.User.Id);
                await context.Punishments.AddAsync(punishment);
                await context.SaveChangesAsync();

                await Functions.SendServerMessage($"{player.User.Name} aplicou um aviso em {character.Name}. Motivo: {reason}", UserStaff.None, false);

                var warns = await CountWarnsInLastMonth(character.User.Id);
                if (warns == 3)
                    await StaffBanCharacter(player, character.Id.ToString(), 0, "3 avisos");
            }

            await CMD_checar(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["deletarsangue"], "Staff", "Deleta todas as amostras de sangue do chão na distância informada", "(distância)")]
    public async Task CMD_deletarsangue(MyPlayer player, int distance)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (distance < 1 || distance > 100)
        {
            player.SendNotification(NotificationType.Error, $"Distância deve ser entre 1 e 100.");
            return;
        }

        var items = Global.Items.Where(x => x.GetCategory() == ItemCategory.BloodSample
            && x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            .ToList();
        if (items.Count == 0)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma amostra de sangue para ser deletada na distância de {distance}m.");
            return;
        }

        items.ForEach(x => x.DeleteObject());
        var context = Functions.GetDatabaseContext();
        context.Items.RemoveRange(items);
        await context.SaveChangesAsync();
        await player.WriteLog(LogType.Staff, $"/deletarsangue {distance} {Functions.Serialize(items)}", null);
        await Functions.SendServerMessage($"{player.User.Name} removeu {items.Count} amostra(s) de sangue.", UserStaff.GameAdmin, true);
    }

    [Command(["deletarcapsulas"], "Staff", "Deleta todas as cápsulas do chão na distância informada", "(distância)")]
    public async Task CMD_deletarcapsulas(MyPlayer player, int distance)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (distance < 1 || distance > 100)
        {
            player.SendNotification(NotificationType.Error, $"Distância deve ser entre 1 e 100.");
            return;
        }

        var items = Global.Items.Where(x => GlobalFunctions.CheckIfIsBulletShell(x.GetCategory())
            && x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
            .ToList();
        if (items.Count == 0)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma cápsula para ser deletada na distância de {distance}m.");
            return;
        }

        items.ForEach(x => x.DeleteObject());
        var context = Functions.GetDatabaseContext();
        context.Items.RemoveRange(items);
        await context.SaveChangesAsync();
        await player.WriteLog(LogType.Staff, $"/deletarcapsulas {distance} {Functions.Serialize(items)}", null);
        await Functions.SendServerMessage($"{player.User.Name} removeu {items.Count} cápsula(s).", UserStaff.GameAdmin, true);
    }

    Task<int> CountWarnsInLastMonth(Guid userId)
    {
        var context = Functions.GetDatabaseContext();
        return context.Punishments
            .Include(x => x.Character)
            .Where(x => x.Type == PunishmentType.Warn
                && x.Character!.UserId == userId
                && x.RegisterDate >= DateTime.Now.AddMonths(-1))
            .CountAsync();
    }

    [Command(["irls"], "Staff", "Vai para o spawn inicial")]
    public async Task CMD_irls(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        player.SetPosition(new(Constants.INITIAL_SPAWN_POSITION_X, Constants.INITIAL_SPAWN_POSITION_Y, Constants.INITIAL_SPAWN_POSITION_Z), 0, false);
        await player.WriteLog(LogType.Staff, "/irls", null);
    }

    [Command(["aspawn"], "Staff", "Spawna um veículo", "(placa)")]
    public async Task CMD_aspawn(MyPlayer player, string plate)
    {
        if (player.User.Staff < UserStaff.LeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => !x.Sold && x.Plate.ToLower() == plate.ToLower());
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhum veículo encontrado com a placa {plate}.");
            return;
        }

        if (Global.Vehicles.Any(x => x.VehicleDB.Id == vehicle.Id))
        {
            player.SendMessage(MessageType.Error, "Veículo já está spawnado.");
            return;
        }

        var spawnedVehicle = await vehicle.Spawnar(player);
        await Functions.SendServerMessage($"{player.User.Name} spawnou o veículo {spawnedVehicle.Identifier} (ID: {spawnedVehicle.Id}).",
            UserStaff.GameAdmin, false);
    }

    [Command(["setvw"], "Staff", "Define o VW de um jogador", "(ID ou nome) (VW)")]
    public async Task CMD_setvw(MyPlayer player, string idOrName, uint vw)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
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

        target.SetPosition(target.GetPosition(), vw, false);
        player.SendMessage(MessageType.Success, $"Você alterou o VW de {target.Character.Name} para {vw}.");
        target.SendMessage(MessageType.Success, $"{player.User.Name} alterou o seu VW para {vw}.");
        await player.WriteLog(LogType.Staff, $"/setvw {vw}", target);
    }

    [Command(["idade"], "Staff", "Altera a idade de um personagem", "(ID ou nome) (idade)")]
    public async Task CMD_idade(MyPlayer player, string idOrName, int age)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (age < Constants.MIN_AGE || age > Constants.MAX_AGE)
        {
            player.SendMessage(MessageType.Error, $"Idade deve ser entre {Constants.MIN_AGE} e {Constants.MAX_AGE}.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        target.Character.SetAttributes(target.Character.Attributes, age);

        player.SendMessage(MessageType.Success, $"Você alterou a idade de {target.Character.Name} para {age}.");
        target.SendMessage(MessageType.Success, $"{player.User.Name} alterou a sua idade para {age}.");
        await player.WriteLog(LogType.Staff, $"/idade {age}", target);
        target.SetNametag();
    }

    [Command(["debug"], "Staff", "Habilita a visão de depuração")]
    public static void CMD_debug(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        player.Debug = !player.Debug;
        player.SendMessage(MessageType.Success, $"Você {(!player.Debug ? "des" : string.Empty)}ativou o modo de depuração.");
        player.Emit("ToggleDebug", player.Debug);
    }

    [Command(["audios"], "Staff", "Lista os áudios ativos")]
    public static void CMD_audios(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        player.Emit("Audio:Debug");
        player.SendMessage(MessageType.Title, $"Áudios: {Global.AudioSpots.Count}");
        foreach (var audioSpot in Global.AudioSpots)
            player.SendMessage(MessageType.None, $"{audioSpot?.Id} {audioSpot?.RegisterDate} {audioSpot?.Source} {audioSpot?.PlayerId} {audioSpot?.VehicleId} {audioSpot?.Position} {audioSpot?.Dimension}");
    }

    [Command(["congelar"], "Staff", "Congela/descongela um jogador", "(ID ou nome)", GreedyArg = true)]
    public async Task CMD_congelar(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
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

        target.Frozen = !target.Frozen;
        var message = target.Frozen ? string.Empty : "des";
        player.SendMessage(MessageType.Success, $"Você {message}congelou {target.Character.Name}.");
        target.SendMessage(MessageType.Success, $"{player.User.Name} {message}congelou você.");
        await player.WriteLog(LogType.Staff, $"/congelar {target.Frozen}", target);
    }

    [Command(["fixpos"], "Staff", "Corrige a posição do personagem para evitar crash ao logar", "(nome do personagem)", GreedyArg = true)]
    public async Task CMD_fixpos(MyPlayer player, string name)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var character = await context.Characters.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        if (character is null)
        {
            player.SendMessage(MessageType.Error, $"Personagem {name} não encontrado.");
            return;
        }

        if (Global.SpawnedPlayers.Any(x => x.Character.Id == character.Id))
        {
            player.SendMessage(MessageType.Error, $"Personagem {character.Name} está logado.");
            return;
        }

        character.FixPosition();
        context.Characters.Update(character);
        await context.SaveChangesAsync();
        player.SendMessage(MessageType.Success, $"Você corrigiu a posição de {character.Name}.");
        await player.WriteLog(LogType.Staff, $"/fixpos {character.Name} ({character.Id})", null);
    }

    [Command(["vida"], "Staff", "Altera a vida de um jogador", "(ID ou nome) (vida)")]
    public async Task CMD_vida(MyPlayer player, string idOrName, int health)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (health < 0 || health > 100)
        {
            player.SendMessage(MessageType.Error, "Vida deve ser entre 0 e 100.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (target.User.Staff >= player.User.Staff
            && target.Character.Id != player.Character.Id)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        target.SetHealth(health);
        target.SendMessage(MessageType.Success, $"{player.User.Name} alterou sua vida para {health}.");
        await Functions.SendServerMessage($"{player.User.Name} alterou a vida de {target.Character.Name} para {health}.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, $"/vida {health}", target);
    }

    [Command(["colete"], "Staff", "Altera o colete de um jogador", "(ID ou nome) (colete)")]
    public async Task CMD_colete(MyPlayer player, string idOrName, int armor)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        if (armor < 0 || armor > 100)
        {
            player.SendMessage(MessageType.Error, "Colete deve ser entre 0 e 100.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (target.User.Staff >= player.User.Staff
            && target.Character.Id != player.Character.Id)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        target.SetArmor(armor);
        target.SendMessage(MessageType.Success, $"{player.User.Name} alterou seu colete para {armor}.");
        await Functions.SendServerMessage($"{player.User.Name} alterou o colete de {target.Character.Name} para {armor}.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, $"/colete {armor}", target);
    }

    [Command(["vertela"], "Staff", "Visualiza informações da tela de um jogador", "(ID ou nome)")]
    public async Task CMD_vertela(MyPlayer player, string idOrName)
    {
        player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
        return;

        if (player.User.Staff < UserStaff.Management)
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

        target.Emit("GetScreenTarget", player.Id);
        player.SendMessage(MessageType.Error, $"Obtendo a tela de {target.Character.Name} ({target.SessionId}). Aguarde...");
        await player.WriteLog(LogType.Staff, "/vertela", target);
    }

    [RemoteEvent(nameof(SendScreenTarget))]
    public static void SendScreenTarget(Player playerParam, ushort targetId, string screen, int index, int length)
    {
        try
        {
            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Id == targetId);
            if (target is null || target.User.Staff < UserStaff.GameAdmin)
                return;

            target.Emit("GetScreenStaff", screen, index, length);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["blocktogstaff"], "Staff", "Bloqueia/desbloqueia os togs administrativos")]
    public async Task CMD_blocktogstaff(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.HeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        Global.StaffToggleBlocked = !Global.StaffToggleBlocked;
        if (Global.StaffToggleBlocked)
        {
            foreach (var target in Global.AllPlayers.Where(x => x.User?.Staff < UserStaff.Management))
                target.StaffChatToggle = target.StaffToggle = false;
        }

        await Functions.SendServerMessage($"{player.User.Name} {(!Global.StaffToggleBlocked ? "des" : string.Empty)}bloqueou os togs administrativo.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, "/blocktogstaff", null);
    }

    [Command(["anametag"], "Staff", "Ativa/desativa a nametag à distância")]
    public async Task CMD_anametag(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        player.Anametag = !player.Anametag;
        player.Emit("ToggleAnametag", player.Anametag);
        await Functions.SendServerMessage($"{player.User.Name} {(!player.Anametag ? "des" : string.Empty)}ativou a nametag à distância.", UserStaff.GameAdmin, false);
        await player.WriteLog(LogType.Staff, $"/anametag {player.Anametag}", null);
    }

    [Command(["checarafk"], "Staff", "Checa se o jogador está AFK", "(ID ou nome)")]
    public static void CMD_checarafk(MyPlayer player, string idOrName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (target.AFKSince is null)
        {
            player.SendMessage(MessageType.Error, "Jogador não está AFK.");
            return;
        }

        player.SendMessage(MessageType.None, $"{target.Character.Name} está AFK desde {target.AFKSince}.");
    }

    [Command(["afks"], "Staff", "Lista os jogadores que estão AFK")]
    public static void CMD_afks(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var afkPlayers = Global.SpawnedPlayers.Where(x => x.AFKSince is not null).ToList();
        if (afkPlayers.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Nenhum jogador está AFK.");
            return;
        }

        player.SendMessage(MessageType.Title, "Jogadores AFK");
        foreach (var afkPlayer in afkPlayers)
            player.SendMessage(MessageType.None, $"{afkPlayer.Character.Name} está AFK desde {afkPlayer.AFKSince}.");
    }

    [Command(["lobby"], "Staff", "Lista os jogadores que não estão spawnados")]
    public static void CMD_lobby(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.HeadAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var lobbyPlayers = Global.AllPlayers.Where(x => x.Character?.PersonalizationStep != CharacterPersonalizationStep.Ready).ToList();
        if (lobbyPlayers.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Nenhum jogador está no lobby.");
            return;
        }

        player.SendMessage(MessageType.Title, "Jogadores no lobby");
        foreach (var lobbyPlayer in lobbyPlayers)
            player.SendMessage(MessageType.None, $"SocialClub: {lobbyPlayer.RealSocialClubName} | IP: {lobbyPlayer.RealIp} | UCP: {lobbyPlayer.User?.Name} ({lobbyPlayer.User?.DiscordUsername}) | Personagem: {lobbyPlayer.Character?.Name}");
    }

    [Command(["mascarados"], "Staff", "Lista os jogadores mascarados")]
    public static void CMD_mascarados(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        var maskedPlayers = Global.AllPlayers.Where(x => x.Masked).ToList();
        if (maskedPlayers.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Nenhum jogador está mascarado.");
            return;
        }

        player.SendMessage(MessageType.Title, "Jogadores mascarados");
        foreach (var maskedPlayer in maskedPlayers)
            player.SendMessage(MessageType.None, $"{maskedPlayer.ICName} ({maskedPlayer.SessionId})");
    }

    [Command(["nome"], "Staff", "Altera o nome permanente de um jogador", "(ID ou nome) (novo nome)", GreedyArg = true)]
    public async Task CMD_nome(MyPlayer player, string idOrName, string newName)
    {
        if (player.User.Staff < UserStaff.Management)
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
            player.SendMessage(MessageType.Error, $"Já existe um personagem com o nome {newName}.");
            return;
        }

        var oldName = target.Character.Name;
        target.Character.SetName(newName);
        await target.Save();
        target.SendMessage(MessageType.Success, $"{player.User.Name} alterou seu nome para {target.Character.Name}.");
        await Functions.SendServerMessage($"{player.User.Name} alterou o nome de {oldName} para {target.Character.Name}.", UserStaff.Management, false);
        await player.WriteLog(LogType.Staff, $"/nome {oldName} {target.Character.Name}", target);
    }

    [Command(["setfaccao"], "Staff", "Define a facção de um jogador", "(ID ou nome) (nome da facção)", GreedyArg = true)]
    public async Task CMD_setfaccao(MyPlayer player, string idOrName, string factionName)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Factions))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (target.Character.FactionId.HasValue)
        {
            player.SendMessage(MessageType.Error, "Jogador já está em uma facção.");
            return;
        }

        var faction = Global.Factions.FirstOrDefault(x => x.Name.ToLower() == factionName.ToLower());
        if (faction is null)
        {
            player.SendMessage(MessageType.Error, $"Facção {factionName} não existe.");
            return;
        }

        var rank = Global.FactionsRanks.Where(x => x.FactionId == faction.Id).MinBy(x => x.Position);
        if (rank is null)
        {
            player.SendMessage(MessageType.Error, $"Facção {factionName} não possui ranks.");
            return;
        }

        target.Character.SetFaction(faction.Id, rank.Id, faction.Type == FactionType.Criminal);

        if (faction.Type != FactionType.Criminal)
            target.OnDuty = false;

        target.SendFactionMessage($"{target.Character.Name} entrou na facção.");
        await player.Save();
        await player.WriteLog(LogType.Staff, $"/setfaccao {factionName}", target);
        await Functions.SendServerMessage($"{player.User.Name} setou {target.Character.Name} na facção {faction.Name}.", UserStaff.GameAdmin, false);
    }

    [Command(["atunar"], "Staff", "Realiza modificações em um veículo")]
    public static void CMD_atunar(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Factions))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        if (!player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnAdministrativeDuty);
            return;
        }

        Functions.CMDTuning(player, null, null, true);
    }

    [Command(["empregos"], "Staff", "Abre o painel de gerenciamento de empregos")]
    public static void CMD_empregos(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.Management)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.Emit("StaffJob:Show", GetJobsJson());
    }

    [RemoteEvent(nameof(StaffJobSave))]
    public async Task StaffJobSave(Player playerParam, string jsonString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.Management)
            {
                player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var request = Functions.Deserialize<JobRequest>(jsonString);

            if (request.Salary <= 0)
            {
                player.SendNotification(NotificationType.Error, "Salário deve ser maior que 0.");
                return;
            }

            if (request.BlipType < 1 || request.BlipType > Constants.MAX_BLIP_TYPE)
            {
                player.SendNotification(NotificationType.Error, string.Format("Tipo do Blip deve ser entre 1 e {0}.", Constants.MAX_BLIP_TYPE));
                return;
            }

            if (request.BlipColor < 1 || request.BlipColor > 85)
            {
                player.SendNotification(NotificationType.Error, "Cor do Blip deve ser entre 1 e 85.");
                return;
            }

            request.BlipName ??= string.Empty;
            if (request.BlipName.Length < 1 || request.BlipName.Length > 100)
            {
                player.SendNotification(NotificationType.Error, "Nome do Blip deve ter entre 1 e 100 caracteres.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleRentModel) && !GlobalFunctions.CheckIfVehicleExists(request.VehicleRentModel))
            {
                player.SendNotification(NotificationType.Error, $"Veículo {request.VehicleRentModel} não existe.");
                return;
            }

            if (request.VehicleRentValue < 0)
            {
                player.SendNotification(NotificationType.Error, "Valor Aluguel deve ser maior ou igual a 0.");
                return;
            }

            var job = Global.Jobs.FirstOrDefault(x => x.Id == request.Id);
            if (job is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            job.Update(request.PosX, request.PosY, request.PosZ, request.Salary,
                request.BlipType, request.BlipColor, request.BlipName,
                request.VehicleRentModel, request.VehicleRentValue,
                request.VehicleRentPosX, request.VehicleRentPosY, request.VehicleRentPosZ,
                request.VehicleRentRotR, request.VehicleRentRotP, request.VehicleRentRotY);

            var context = Functions.GetDatabaseContext();
            context.Jobs.Update(job);
            await context.SaveChangesAsync();

            job.CreateIdentifier();

            await player.WriteLog(LogType.Staff, $"Gravar Emprego | {Functions.Serialize(job)}", null);
            player.SendNotification(NotificationType.Success, "Emprego editado.");

            var json = GetJobsJson();
            foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff == UserStaff.Management))
                target.Emit("StaffJob:Update", json);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetJobsJson()
    {
        return Functions.Serialize(Global.Jobs
            .Select(x => new
            {
                x.Id,
                Name = x.CharacterJob.GetDescription(),
                x.PosX,
                x.PosY,
                x.PosZ,
                x.Salary,
                x.BlipType,
                x.BlipColor,
                x.BlipName,
                x.VehicleRentModel,
                x.VehicleRentValue,
                x.VehicleRentPosX,
                x.VehicleRentPosY,
                x.VehicleRentPosZ,
                x.VehicleRentRotR,
                x.VehicleRentRotP,
                x.VehicleRentRotY,
            })
            .OrderBy(x => x.Name));
    }

    [Command(["testarefeito"], "Staff", "Testa efeitos do GTA V", "(nome da droga)", GreedyArg = true)]
    public static async Task CMD_testarefeito(MyPlayer player, string drugName)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Name.ToLower() == drugName.ToLower());

        var drug = Global.Drugs.FirstOrDefault(x => x.ItemTemplateId == itemTemplate?.Id);
        if (drug is null)
        {
            player.SendMessage(MessageType.Error, $"Droga {drugName} não existe.");
            return;
        }

        player.SetDrugEffect(drug.ShakeGameplayCamName, drug.ShakeGameplayCamIntensity, drug.TimecycModifier, drug.AnimpostFXName);
        await player.WriteLog(LogType.Staff, $"/testarefeito {drugName}", null);
        player.SendMessage(MessageType.Success, $"Você está testando os efeitos da droga {drugName}.");
    }

    [Command(["pararefeito"], "Staff", "Para os efeitos do GTA V")]
    public static async Task CMD_pararefeito(MyPlayer player)
    {
        if (player.User.Staff < UserStaff.GameAdmin)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
            return;
        }

        player.ClearDrugEffect();
        await player.WriteLog(LogType.Staff, "/pararefeito", null);
        player.SendMessage(MessageType.Success, "Você parou de testar os efeitos da droga.");
    }
}