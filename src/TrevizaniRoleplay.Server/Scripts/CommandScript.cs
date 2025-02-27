using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extesions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class CommandScript : Script
{
    [Command("id", "/id (ID ou nome)", GreedyArg = true)]
    public static void CMD_id(MyPlayer player, string idOrName)
    {
        var targets = Global.SpawnedPlayers
            .Where(x => int.TryParse(idOrName, out int id) && x.SessionId == id || x.ICName.ToLower().Contains(idOrName.ToLower()))
            .OrderBy(x => x.SessionId)
            .ToList();
        if (targets.Count == 0)
        {
            player.SendMessage(MessageType.Error, $"Nenhum jogador foi encontrado com a pesquisa: {idOrName}.");
            return;
        }

        player.SendMessage(MessageType.Title, $"Jogadores encontrados com a pesquisa: {idOrName}.");
        foreach (var target in targets)
            player.SendMessage(MessageType.None, $"{target.ICName} ({target.SessionId})");
    }

    [Command("aceitar", "/aceitar (tipo)", Aliases = ["ac"])]
    public async Task CMD_aceitar(MyPlayer player, int type)
    {
        if (!Enum.IsDefined(typeof(InviteType), type))
        {
            player.SendMessage(MessageType.Error, "Tipo inválido.");
            return;
        }

        var invite = player.Invites.FirstOrDefault(x => x.Type == (InviteType)type);
        if (invite is null)
        {
            player.SendMessage(MessageType.Error, $"Você não possui nenhum convite do tipo {type}.");
            return;
        }

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == invite.SenderCharacterId);
        if (target is null)
        {
            player.Invites.RemoveAll(x => x.Type == (InviteType)type);
            player.SendMessage(MessageType.Error, "Jogador que enviou o convite não está online.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        switch ((InviteType)type)
        {
            case InviteType.Faction:
                if (!Guid.TryParse(invite.Value[0], out Guid factionId) || !Guid.TryParse(invite.Value[1], out Guid factionRankId))
                    return;

                var faction = Global.Factions.FirstOrDefault(x => x.Id == factionId)!;
                player.Character.SetFaction(factionId, factionRankId, faction.Type == FactionType.Criminal);

                if (faction.Type != FactionType.Criminal)
                    player.OnDuty = false;

                player.SendFactionMessage($"{player.Character.Name} entrou na facção.");
                await player.Save();
                break;
            case InviteType.PropertySell:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
                    return;
                }

                if (!Guid.TryParse(invite.Value[0], out Guid propriedade) || !int.TryParse(invite.Value[1], out int value))
                    return;

                if (player.Money < value)
                {
                    player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, value));
                    break;
                }

                var prop = Global.Properties.FirstOrDefault(x => x.Id == propriedade && x.CharacterId == target.Character.Id);
                if (prop is null)
                {
                    player.SendMessage(MessageType.Error, "Propriedade inválida.");
                    break;
                }

                if (player.GetDimension() != prop.Number)
                {
                    player.SendMessage(MessageType.Error, "Você não está no interior da propriedade.");
                    return;
                }

                var res = await target.GiveMoney(value);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    player.SendMessage(MessageType.Error, res);
                    return;
                }

                await player.RemoveMoney(value);

                await prop.ChangeOwner(player.Character.Id);

                await target.WriteLog(LogType.Sell, $"/pvender {prop.FormatedAddress} {prop.Number} {prop.Id} {value}", player);
                player.SendMessage(MessageType.Success, $"Você comprou {prop.FormatedAddress} de {target.ICName} por ${value:N0}.");
                target.SendMessage(MessageType.Success, $"Você vendeu {prop.FormatedAddress} para {player.ICName} por ${value:N0}.");
                break;
            case InviteType.Frisk:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
                    return;
                }

                player.CloseInventory();
                player.SendMessage(MessageType.Success, $"Você está sendo revistado por {target.ICName}.");
                target.ShowInventory(InventoryShowType.Inspect, player.ICName, player.GetInventoryItemsJson(), false, player.Character.Id);
                break;
            case InviteType.VehicleSell:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
                    return;
                }

                if (!Guid.TryParse(invite.Value[0], out Guid veiculo) || !int.TryParse(invite.Value[1], out int vehicleValue))
                    return;

                if (player.Money < vehicleValue)
                {
                    player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, vehicleValue));
                    break;
                }

                var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == veiculo);
                if (veh == null)
                {
                    player.SendMessage(MessageType.Error, "Veículo inválido.");
                    break;
                }

                if (player.GetPosition().DistanceTo(veh.GetPosition()) > Constants.RP_DISTANCE)
                {
                    player.SendMessage(MessageType.Error, "Você não está próximo do veículo.");
                    return;
                }

                res = await target.GiveMoney(vehicleValue);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    player.SendMessage(MessageType.Error, res);
                    return;
                }

                await player.RemoveMoney(vehicleValue);

                veh.VehicleDB.SetOwner(player.Character.Id);

                context.Vehicles.Update(veh.VehicleDB);
                await context.SaveChangesAsync();

                player.SendMessage(MessageType.Success, $"Você comprou o veículo {veh.VehicleDB.Id} de {target.ICName} por ${vehicleValue:N0}.");
                target.SendMessage(MessageType.Success, $"Você vendeu o veículo {veh.VehicleDB.Id} para {player.ICName} por ${vehicleValue:N0}.");
                await target.WriteLog(LogType.Sell, $"/vvender {veh.Identifier} {veh.VehicleDB.Id} {vehicleValue}", player);
                break;
            case InviteType.Company:
                if (!Guid.TryParse(invite.Value[0], out Guid companyId))
                    return;

                var company = Global.Companies.FirstOrDefault(x => x.Id == companyId);
                if (company == null)
                    return;

                var companyCharacter = new CompanyCharacter();
                companyCharacter.Create(companyId, player.Character.Id);
                await context.CompaniesCharacters.AddAsync(companyCharacter);
                await context.SaveChangesAsync();

                company.Characters!.Add(companyCharacter);

                player.SendMessage(MessageType.Success, $"Você aceitou o convite para entrar na empresa.");
                target.SendMessage(MessageType.Success, $"{player.Character.Name} aceitou seu convite para entrar na empresa.");
                break;
            case InviteType.Mechanic:
                player.VehicleTuning = Functions.Deserialize<VehicleTuning>(invite.Value.FirstOrDefault()!);

                player.SendMessage(MessageType.Success, $"Você aceitou o convite para receber o catálogo de modificações veiculares.");
                target.SendMessage(MessageType.Success, $"{player.Character.Name} aceitou seu convite para receber o catálogo de modificações veiculares.");
                break;
            case InviteType.VehicleTransfer:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
                    return;
                }

                if (!Guid.TryParse(invite.Value[0], out Guid transferVehicleId))
                    return;

                var transferVehicle = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == transferVehicleId);
                if (transferVehicle is null)
                {
                    player.SendMessage(MessageType.Error, "Veículo inválido.");
                    break;
                }

                if (player.GetPosition().DistanceTo(transferVehicle.GetPosition()) > Constants.RP_DISTANCE)
                {
                    player.SendMessage(MessageType.Error, "Você não está próximo do veículo.");
                    return;
                }

                transferVehicle.VehicleDB.SetOwner(player.Character.Id);

                context.Vehicles.Update(transferVehicle.VehicleDB);
                await context.SaveChangesAsync();

                player.SendMessage(MessageType.Success, $"O veículo {transferVehicle.VehicleDB.Model.ToUpper()} {transferVehicle.VehicleDB.Plate} de {target.ICName} foi transferido para você.");
                target.SendMessage(MessageType.Success, $"Você transferiu o veículo {transferVehicle.VehicleDB.Model.ToUpper()} {transferVehicle.VehicleDB.Plate} para {player.ICName}.");
                await target.WriteLog(LogType.Sell, $"/vtransferir {transferVehicle.Identifier} {transferVehicle.VehicleDB.Id}", player);
                break;
            case InviteType.Carry:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
                    return;
                }

                player.StartBeingCarried(target);
                await player.WriteLog(LogType.General, "/carregar não ferido", target);
                break;
        }

        player.Invites.RemoveAll(x => x.Type == (InviteType)type);
    }

    [Command("recusar", "/recusar (tipo)", Aliases = ["rc"])]
    public static void CMD_recusar(MyPlayer player, int type)
    {
        if (!Enum.IsDefined(typeof(InviteType), type))
        {
            player.SendMessage(MessageType.Error, "Tipo inválido.");
            return;
        }

        var invite = player.Invites.FirstOrDefault(x => x.Type == (InviteType)type);
        if (invite is null)
        {
            player.SendMessage(MessageType.Error, $"Você não possui nenhum convite do tipo {type}.");
            return;
        }

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == invite.SenderCharacterId);
        var strPlayer = string.Empty;
        var strTarget = string.Empty;

        switch ((InviteType)type)
        {
            case InviteType.Faction:
                strPlayer = strTarget = "entrar na facção";
                break;
            case InviteType.PropertySell:
                strPlayer = "compra da propriedade";
                strTarget = "venda da propriedade";
                break;
            case InviteType.VehicleSell:
                strPlayer = "compra de veículo";
                strTarget = "venda de veículo";
                break;
            case InviteType.Frisk:
                strPlayer = strTarget = "revista";
                break;
            case InviteType.Company:
                strPlayer = strTarget = "entrar na empresa";
                break;
            case InviteType.Mechanic:
                strPlayer = strTarget = "receber o catálogo de modificações veiculares";
                break;
            case InviteType.VehicleTransfer:
                strPlayer = strTarget = "transferência de veículo";
                break;
            case InviteType.Carry:
                strPlayer = strTarget = "ser carregado";
                break;
        }

        player.SendMessage(MessageType.Success, $"Você recusou o convite para {strPlayer}.");
        target?.SendMessage(MessageType.Success, $"{player.ICName} recusou seu convite para {strTarget}.");

        player.Invites.RemoveAll(x => x.Type == (InviteType)type);
    }

    [Command("revistar", "/revistar (ID ou nome)")]
    public static void CMD_revistar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        if (player.Faction?.Type == FactionType.Police && player.OnDuty)
        {
            target.CloseInventory();
            target.SendMessage(MessageType.Success, $"Você está sendo revistado por {player.ICName}.");
            player.ShowInventory(InventoryShowType.Inspect, target.ICName, target.GetInventoryItemsJson(), false, target.Character.Id);
            return;
        }

        var invite = new Invite
        {
            Type = InviteType.Frisk,
            SenderCharacterId = player.Character.Id,
        };
        target.Invites.RemoveAll(x => x.Type == InviteType.Frisk);
        target.Invites.Add(invite);

        player.SendMessage(MessageType.Success, $"Você solicitou uma revista para {target.ICName}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} solicitou uma revista em você. (seus itens poderão ser subtraídos) (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
    }

    [Command("comprar")]
    public async Task CMD_comprar(MyPlayer player)
    {
        var property = Global.Properties
            .Where(x => !x.CharacterId.HasValue && x.Value > 0
                && player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhuma propriedade à venda.");
            return;
        }

        if (player.Money < property.Value)
        {
            player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, property.Value));
            return;
        }

        await player.RemoveMoney(property.Value);
        await property.ChangeOwner(player.Character.Id);

        await player.WriteLog(LogType.Buy, $"Comprar Propriedade {property.FormatedAddress} ({property.Id}) por {property.Value}", null);
        player.SendMessage(MessageType.Success, $"Você comprou {property.FormatedAddress} por ${property.Value:N0}.");
    }

    [Command("sos", "/sos (mensagem)", GreedyArg = true)]
    public async Task CMD_sos(MyPlayer player, string message)
    {
        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == player.SessionId && x.Type == HelpRequestType.SOS);
        if (helpRequest is not null)
        {
            var target = await helpRequest.Check();
            if (target is not null)
            {
                player.SendMessage(MessageType.Error, "Você já possui um SOS. Use /cancelarsos.");
                return;
            }
        }

        helpRequest = new HelpRequest();
        helpRequest.Create(message, player.User.Id, player.SessionId, player.Character.Name, player.User.Name, HelpRequestType.SOS);

        var context = Functions.GetDatabaseContext();
        await context.HelpRequests.AddAsync(helpRequest);
        await context.SaveChangesAsync();

        Global.HelpRequests.Add(helpRequest);

        foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.ServerSupport && !x.StaffToggle))
        {
            target.SendMessage(MessageType.Error, $"SOS de {player.Character.Name} ({player.SessionId}) ({player.User.Name})");
            target.SendMessage(MessageType.Error, $"Pergunta: {{#B0B0B0}}{message} {{{Constants.ERROR_COLOR}}}(/at {player.SessionId} /csos {player.SessionId})");
        }

        player.SendMessage(MessageType.Success, "O seu SOS foi enviado e em breve será respondido por um Supporter!");
    }

    [Command("cancelarsos")]
    public async Task CMD_cancelarsos(MyPlayer player)
    {
        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == player.SessionId && x.Type == HelpRequestType.SOS);
        if (helpRequest is null || await helpRequest.Check() is null)
        {
            player.SendMessage(MessageType.Error, "Você não possui um SOS.");
            return;
        }

        helpRequest.Answer(null);
        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();

        Global.HelpRequests.Remove(helpRequest);
        player.SendMessage(MessageType.Success, "Você cancelou seu SOS.");
    }

    [Command("cancelarreport")]
    public async Task CMD_cancelarreport(MyPlayer player)
    {
        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == player.SessionId && x.Type == HelpRequestType.Report);
        if (helpRequest is null || await helpRequest.Check() is null)
        {
            player.SendMessage(MessageType.Error, "Você não possui um report.");
            return;
        }

        helpRequest.Answer(null);
        var context = Functions.GetDatabaseContext();
        context.HelpRequests.Update(helpRequest);
        await context.SaveChangesAsync();

        Global.HelpRequests.Remove(helpRequest);
        player.SendMessage(MessageType.Success, "Você cancelou seu report.");
    }

    [Command("reportar", "/reportar (mensagem)", GreedyArg = true)]
    public async Task CMD_reportar(MyPlayer player, string message)
    {
        var helpRequest = Global.HelpRequests.FirstOrDefault(x => x.CharacterSessionId == player.SessionId && x.Type == HelpRequestType.Report);
        if (helpRequest is not null)
        {
            var target = await helpRequest.Check();
            if (target is not null)
            {
                player.SendMessage(MessageType.Error, "Você já possui um report. Use /cancelarreport.");
                return;
            }
        }

        helpRequest = new HelpRequest();
        helpRequest.Create(message, player.User.Id, player.SessionId, player.Character.Name, player.User.Name, HelpRequestType.Report);

        var context = Functions.GetDatabaseContext();
        await context.HelpRequests.AddAsync(helpRequest);
        await context.SaveChangesAsync();

        Global.HelpRequests.Add(helpRequest);

        foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.JuniorServerAdmin && !x.StaffToggle))
        {
            target.SendMessage(MessageType.Error, $"Report de {player.Character.Name} ({player.SessionId}) ({player.User.Name})");
            target.SendMessage(MessageType.Error, $"Pergunta: {{#B0B0B0}}{message} {{{Constants.ERROR_COLOR}}}(/ar {player.SessionId} /creport {player.SessionId})");
        }

        player.SendMessage(MessageType.Success, "O seu Report foi enviado e em breve será respondido por um administrador!");
    }

    [Command("ferimentos", "/ferimentos (ID ou nome)")]
    public static void CMD_ferimentos(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

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
            })), false);
    }

    [Command("aceitarhospital")]
    [RemoteEvent("AcceptPlayerKill")]
    public static void CMD_aceitarhospital(Player playerParam)
    {
        var player = Functions.CastPlayer(playerParam);
        if (player.Character.Wound != CharacterWound.CanHospitalCK)
        {
            player.SendMessage(MessageType.Error, "Você ainda não pode executar esse comando.");
            return;
        }

        player.Emit("AcceptDeath", false);
    }

    [Command("aceitarck")]
    [RemoteEvent("AcceptCharacterKill")]
    public static void CMD_aceitarck(Player playerParam)
    {
        var player = Functions.CastPlayer(playerParam);
        if (player.Character.Wound != CharacterWound.CanHospitalCK)
        {
            player.SendMessage(MessageType.Error, "Você ainda não pode executar esse comando.");
            return;
        }

        player.Emit("AcceptDeath", true);
    }

    [RemoteEvent(nameof(AcceptDeath))]
    public async Task AcceptDeath(Player playerParam, bool ck, string address)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var spot = Global.Spots
                .Where(x => x.Type == SpotType.HealMe)
                .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
            if (spot is null)
            {
                player.SendMessage(MessageType.Error, "Nenhum ponto de cura foi configurado. Por favor, reporte o bug.");
                return;
            }

            if (player.GetDimension() != 0)
            {
                var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
                if (property is not null)
                    address = property.FormatedAddress;
            }

            var droppableCategories = new List<ItemCategory> {
                ItemCategory.Weapon,
                ItemCategory.Drug,
                ItemCategory.WeaponComponent,
            };
            var itemsToRemove = player.Items.Where(x => droppableCategories.Contains(x.GetCategory())
                || Functions.CheckIfIsAmmo(x.GetCategory())
                || x.OnlyOnDuty)
                .ToList();
            var bodyItems = new List<BodyItem>();
            foreach (var characterItem in itemsToRemove)
            {
                if (characterItem.OnlyOnDuty)
                    continue;

                var bodyItem = new BodyItem();
                bodyItem.Create(characterItem.ItemTemplateId, characterItem.Subtype, characterItem.Quantity, characterItem.Extra);
                bodyItem.SetSlot(characterItem.Slot);
                bodyItems.Add(bodyItem);
            }

            var body = new Body();
            body.Create(player.Character.Id, player.Character.Name, (uint)player.CorrectPed,
                player.GetPosition().X, player.GetPosition().Y, player.GetPosition().Z,
                player.GetDimension(), address, player.Character.PersonalizationJSON!, Functions.Serialize(player.GetOutfit()),
                player.Character.WoundsJSON, bodyItems);

            var context = Functions.GetDatabaseContext();
            await context.Bodies.AddAsync(body);
            await context.SaveChangesAsync();

            body.CreateIdentifier();
            Global.Bodies.Add(body);

            await player.RemoveItem(itemsToRemove);
            var stackedItems = itemsToRemove
                .Where(x => x.GetIsStack())
                .Select(x => new
                {
                    x.ItemTemplateId,
                    x.Quantity
                })
                .ToList();
            foreach (var stackedItem in stackedItems)
                await player.RemoveStackedItem(stackedItem.ItemTemplateId, stackedItem.Quantity);

            if (ck)
            {
                player.Character.SetDeath("Aceitou CK");

                foreach (var target in Global.SpawnedPlayers.Where(x => x.GetDimension() == player.GetDimension() && player.GetPosition().DistanceTo(x.GetPosition()) <= 20))
                    target.SendMessage(MessageType.Error, $"(( {player.ICName} ({player.SessionId}) aceitou CK. ))");

                await Functions.SendServerMessage($"{player.Character.Name} aceitou CK.", UserStaff.None, false);
                await player.Save();
                await player.WriteLog(LogType.Death, "/aceitarck", null);
                await player.ListCharacters("CK", "Você aceitou o CK no seu personagem.");
            }
            else
            {
                player.Heal();
                player.StopBeingCarried();
                player.Character.ClearDrug();
                player.DrugTimer?.Stop();
                player.ClearDrugEffect();
                player.Cuffed = false;

                foreach (var target in Global.SpawnedPlayers.Where(x => x.GetDimension() == player.GetDimension() && player.GetPosition().DistanceTo(x.GetPosition()) <= 20))
                    target.SendMessage(MessageType.Error, $"(( {player.ICName} ({player.SessionId}) aceitou tratamento no hospital. ))");

                player.RemoveBank(Global.Parameter.HospitalValue);

                if (player.Character.JailFinalDate.HasValue && player.Character.JailFinalDate > DateTime.Now)
                    player.SetPosition(new(Global.Parameter.PrisonInsidePosX, Global.Parameter.PrisonInsidePosY, Global.Parameter.PrisonInsidePosZ), Global.Parameter.PrisonInsideDimension, true);
                else
                    player.SetPosition(new(spot.PosX, spot.PosY, spot.PosZ), spot.Dimension, true);

                var financialTransaction = new FinancialTransaction();
                financialTransaction.Create(FinancialTransactionType.Withdraw, player.Character.Id, Global.Parameter.HospitalValue, "Custos Hospitalares");
                await context.FinancialTransactions.AddAsync(financialTransaction);
                await context.SaveChangesAsync();

                player.SendMessage(MessageType.Success, $"Você aceitou o tratamento e foi levado para o hospital mais próximo. Os custos hospitalares foram ${Global.Parameter.HospitalValue:N0}.");
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("mostraridentidade", "/mostraridentidade (ID ou nome)", Aliases = ["mostrarid"])]
    public static void CMD_mostraridentidade(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        target.SendMessage(MessageType.Title, $"ID de {player.Character.Name}");
        target.SendMessage(MessageType.None, $"Sexo: {player.Character.Sex.GetDisplay()}");
        target.SendMessage(MessageType.None, $"Idade: {player.Character.Age} anos");
        player.SendMessageToNearbyPlayers(player == target ? "olha sua própria ID." : $"mostra sua ID para {target.ICName}.", MessageCategory.Ame);
    }

    [Command("mostrarlicenca", "/mostrarlicenca (ID ou nome)", Aliases = ["ml"])]
    public static void CMD_mostrarlicenca(MyPlayer player, string idOrName)
    {
        if (!player.Character.DriverLicenseValidDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não possui uma licença de motorista.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        var status = $"{{{Constants.SUCCESS_COLOR}}}VÁLIDA";
        if (player.Character.PoliceOfficerBlockedDriverLicenseCharacterId.HasValue)
            status = $"{{{Constants.ERROR_COLOR}}}REVOGADA";
        else if (player.Character.DriverLicenseBlockedDate?.Date >= DateTime.Now.Date)
            status = $"{{{Constants.ERROR_COLOR}}}SUSPENSA";
        else if (player.Character.DriverLicenseValidDate?.Date < DateTime.Now.Date)
            status = $"{{{Constants.ERROR_COLOR}}}VENCIDA";

        target.SendMessage(MessageType.Title, $"Licença de Motorista de {player.Character.Name}");
        target.SendMessage(MessageType.None, $"Validade: {player.Character.DriverLicenseValidDate?.ToShortDateString()}");
        target.SendMessage(MessageType.None, $"Status: {status}");
        player.SendMessageToNearbyPlayers(player == target ? "olha sua própria licença de motorista." : $"mostra sua licença de motorista para {target.ICName}.", MessageCategory.Ame);
    }

    [Command("horas")]
    public static void CMD_horas(MyPlayer player)
    {
        var horas = Convert.ToInt32(Math.Truncate(player.Character.ConnectedTime / 60M));
        var tempo = 60 - ((horas + 1) * 60 - player.Character.ConnectedTime);
        player.SendMessage(MessageType.None, $"{Constants.SERVER_INITIALS} | {DateTime.Now} | {{{Constants.MAIN_COLOR}}}{tempo} {{#FFFFFF}}de {{{Constants.MAIN_COLOR}}}60 {{#FFFFFF}}minutos para o próximo pagamento.");

        if (player.Character.JailFinalDate.HasValue)
            player.SendMessage(MessageType.None, $"Você está preso até {player.Character.JailFinalDate}.");
    }

    [Command("limparchat")]
    public static void CMD_limparchat(MyPlayer player) => player.ClearChat();

    [Command("save")]
    public static void CMD_save(MyPlayer player)
    {
        player.Emit("alt:log", $"DIMENSION: {player.GetDimension()}");
        if (player.Vehicle is MyVehicle vehicle)
        {
            player.Emit("alt:log", $"POS: {vehicle.GetPosition().X.ToString().Replace(",", ".")}f, {vehicle.GetPosition().Y.ToString().Replace(",", ".")}f, {vehicle.GetPosition().Z.ToString().Replace(",", ".")}f");
            player.Emit("alt:log", $"ROT: {vehicle.Rotation.X.ToString().Replace(",", ".")}f, {vehicle.Rotation.Y.ToString().Replace(",", ".")}f, {vehicle.Rotation.Z.ToString().Replace(",", ".")}f");
        }
        else
        {
            player.Emit("alt:log", $"POS: {player.GetPosition().X.ToString().Replace(",", ".")}f, {player.GetPosition().Y.ToString().Replace(",", ".")}f, {player.GetPosition().Z.ToString().Replace(",", ".")}f");
            player.Emit("alt:log", $"ROT: {player.GetRotation().X.ToString().Replace(",", ".")}f, {player.GetRotation().Y.ToString().Replace(",", ".")}f, {player.GetRotation().Z.ToString().Replace(",", ".")}f");
        }
    }

    [Command("dados", "/dados (2-20)")]
    public static void CMD_dados(MyPlayer player, int maxNumber)
    {
        if (maxNumber < 2 || maxNumber > 20)
        {
            player.SendMessage(MessageType.Error, $"Número precisa ser entre 2 e 20.");
            return;
        }

        var number = Enumerable.Range(1, maxNumber).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        var message = $@"[DADOS] {player.ICName} joga um dado de {maxNumber} lados, caindo em {number}.";
        player.SendMessageToNearbyPlayers(message, MessageCategory.DiceCoin);
    }

    [Command("moeda")]
    public static void CMD_moeda(MyPlayer player)
    {
        var number = new List<int> { 1, 2 }.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        var message = $@"[MOEDA] {player.ICName} joga a moeda e esta fica com a {(number == 1 ? "cara" : "coroa")} voltada para cima.";
        player.SendMessageToNearbyPlayers(message, MessageCategory.DiceCoin);
    }

    [Command("levantar", "/levantar (ID ou nome)")]
    public static void CMD_levantar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        if (target.Character.Wound != CharacterWound.SeriouslyInjured || target.Wounds.Any(x => x.Weapon != Functions.GetWeaponName((uint)WeaponModel.Fist)))
        {
            player.SendMessage(MessageType.Error, "Jogador não está gravemente ferido ou ferido somente com socos.");
            return;
        }

        target.Heal(true);
        player.SendMessageToNearbyPlayers($"ajuda {target.ICName} a se levantar.", MessageCategory.NormalMe);
    }

    [Command("trocarpersonagem")]
    public async Task CMD_trocarpersonagem(MyPlayer player) => await player.ListCharacters("Troca de Personagem", string.Empty);

    [Command("admins")]
    public static void CMD_admins(MyPlayer player)
    {
        player.SendMessage(MessageType.Title, $"STAFF: {Constants.SERVER_NAME}");

        var admins = Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.JuniorServerAdmin);
        foreach (var admin in admins
            .Where(x => x.OnAdminDuty || player.User.Staff >= UserStaff.JuniorServerAdmin)
            .OrderByDescending(x => x.OnAdminDuty)
            .ThenByDescending(x => x.User.Staff)
            .ThenBy(x => x.User.Name))
            player.SendMessage(MessageType.None, $"{admin.User.Staff.GetDisplay()} {admin.User.Name}{(player.User.Staff >= UserStaff.JuniorServerAdmin ? $" ({admin.SessionId})" : string.Empty)}", admin.OnAdminDuty ? admin.StaffColor : "#B0B0B0");

        var adminsOffDuty = admins.Count(x => !x.OnAdminDuty);
        player.SendMessage(MessageType.None, $"{adminsOffDuty} administrador{(adminsOffDuty == 1 ? " está" : "es estão")} online em roleplay. Se precisar de ajuda de um administrador, utilize o /reportar.");
    }

    [Command("historicocriminal")]
    public async Task CMD_historicocriminal(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        var faction = Global.Factions.FirstOrDefault(x => x.Id == property?.FactionId);
        if (faction?.Type != FactionType.Police)
        {
            player.SendNotification(NotificationType.Error, "Você não está em uma propriedade policial.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        var jails = await context.Jails.Where(x => x.CharacterId == player.Character.Id)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync();

        player.SendMessage(MessageType.Title, $"Histórico Criminal de {player.Character.Name} ({DateTime.Now})");
        if (jails.Count == 0)
        {
            player.SendMessage(MessageType.None, "Histórico criminal está limpo.");
        }
        else
        {
            foreach (var jail in jails)
                player.SendMessage(MessageType.None, $"Preso em {jail.RegisterDate}. Solto em {jail.EndDate}.");
        }
    }

    [Command("cancelarconvite", "/cancelarconvite (ID ou nome) (tipo)", Aliases = ["cc"])]
    public static void CMD_cancelarconvite(MyPlayer player, string idOrName, int tipo)
    {
        if (!Enum.IsDefined(typeof(InviteType), tipo))
        {
            player.SendMessage(MessageType.Error, "Tipo inválido.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        var type = (InviteType)tipo;
        if (!target.Invites.Any(x => x.SenderCharacterId == player.Character.Id && x.Type == type))
        {
            player.SendMessage(MessageType.Error, $"Você não enviou um convite do tipo {type.GetDisplay()} para {target.ICName}.");
            return;
        }

        var strPlayer = string.Empty;
        var strTarget = string.Empty;

        switch (type)
        {
            case InviteType.Faction:
                strPlayer = strTarget = "entrar na facção";
                break;
            case InviteType.PropertySell:
                strPlayer = "compra da propriedade";
                strTarget = "venda da propriedade";
                break;
            case InviteType.VehicleSell:
                strPlayer = "compra de veículo";
                strTarget = "venda de veículo";
                break;
            case InviteType.Frisk:
                strPlayer = strTarget = "revista";
                break;
        }

        target.Invites.RemoveAll(x => x.Type == type);
        player.SendMessage(MessageType.Success, $"Você cancelou o convite para {strPlayer}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} cancelou o convite para {strTarget}.");
    }

    [Command("boombox")]
    public static void CMD_boombox(MyPlayer player)
    {
        var item = Global.Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.Boombox
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (item is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de uma boombox.");
            return;
        }

        var audioSpot = item.GetAudioSpot();
        player.Emit("Boombox", item.Id.ToString(), audioSpot?.Source ?? string.Empty, audioSpot?.Volume ?? 1,
            player.GetCurrentPremium() != UserPremium.None, Functions.Serialize(Global.AudioRadioStations.OrderBy(x => x.Name)));
    }

    [Command("stopanim", Aliases = ["sa"])]
    public static void CMD_stopanim(MyPlayer player) => player.CheckAnimations(true);

    [Command("animacoes")]
    public static void CMD_animacoes(MyPlayer player)
    {
        if (Global.Animations.Count == 0)
        {
            player.SendNotification(NotificationType.Error, "Não há animações criadas. Por favor, reporte o bug.");
            return;
        }

        player.Emit("ViewAnimations",
            Functions.Serialize(Global.Animations.Select(x => x.Category).Distinct().Order()),
            Functions.Serialize(Global.Animations.OrderBy(x => x.Category).ThenBy(x => x.Display).Select(x => new
            {
                x.Category,
                x.Display,
            })));
    }

    [Command("dl")]
    public static void CMD_dl(MyPlayer player)
    {
        player.User.SetVehicleTagToggle(!player.User.VehicleTagToggle);
        player.Emit("dl:Config", player.User.VehicleTagToggle);
        player.SendNotification(NotificationType.Success, $"Você {(!player.User.VehicleTagToggle ? "des" : string.Empty)}ativou o DL.");
    }

    [Command("tela")]
    public static void CMD_tela(MyPlayer player)
    {
        player.ToggleChatBackgroundColor = !player.ToggleChatBackgroundColor;
        player.Emit(Constants.CHAT_PAGE_TOGGLE_SCREEN, player.ToggleChatBackgroundColor ? $"#{player.User.ChatBackgroundColor}" : "transparent");
    }

    [Command("timestamp")]
    public static void CMD_timestamp(MyPlayer player)
    {
        player.User.SetTimeStampToggle(!player.User.TimeStampToggle);
        player.ConfigChat();
        player.SendNotification(NotificationType.Success, $"Você {(!player.User.TimeStampToggle ? "des" : string.Empty)}ativou o timestamp.");
    }

    [Command("modofoto")]
    public static void CMD_modofoto(MyPlayer player)
    {
        player.PhotoMode = !player.PhotoMode;
        player.StartNoClip(true);
        player.SetNametag();
        player.SendNotification(NotificationType.Success, $"Você {(!player.PhotoMode ? "des" : string.Empty)}ativou o modo foto.");
    }

    [Command("anuncios")]
    public static void CMD_anuncios(MyPlayer player)
    {
        player.Emit("Announcements:Show", Functions.Serialize(Global.Announcements.OrderByDescending(x => x.Date)));
    }

    [Command("carregar", "/carregar (ID ou nome)")]
    public static async Task CMD_carregar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        if (target.PlayerCarrying.HasValue)
        {
            player.SendMessage(MessageType.Error, "Jogador já está sendo carregado.");
            return;
        }

        if (Global.SpawnedPlayers.Any(x => x.PlayerCarrying == target.Id))
        {
            player.SendMessage(MessageType.Error, "Jogador está carregando alguém.");
            return;
        }

        if (target.Character.Wound != CharacterWound.None)
        {
            target.StartBeingCarried(player);
            await player.WriteLog(LogType.General, "/carregar ferido", target);
            return;
        }

        var invite = new Invite
        {
            Type = InviteType.Carry,
            SenderCharacterId = player.Character.Id,
        };
        target.Invites.RemoveAll(x => x.Type == InviteType.Carry);
        target.Invites.Add(invite);

        player.SendMessage(MessageType.Success, $"Você solicitou carregar {target.ICName}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} solicitou carregar você. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
    }

    [Command("soltar")]
    public static async Task CMD_soltar(MyPlayer player)
    {
        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.PlayerCarrying == player.Id);
        if (target is null)
        {
            player.SendMessage(MessageType.Error, "Você não está carregando ninguém.");
            return;
        }

        target.StopBeingCarried();
        await player.WriteLog(LogType.General, "/soltar", target);
    }

    [Command("maquiagem")]
    public static void CMD_maquiagem(MyPlayer player)
    {
        player.Edit(4);
    }

    [Command("fontsize", "/fontsize (tamanho)")]
    public static void CMD_fontsize(MyPlayer player, int fontSize)
    {
        if (fontSize < Constants.MIN_CHAT_FONT_SIZE || fontSize > Constants.MAX_CHAT_FONT_SIZE)
        {
            player.SendMessage(MessageType.Error, $"Tamanho da fonte do chat deve ser entre {Constants.MIN_CHAT_FONT_SIZE} e {Constants.MAX_CHAT_FONT_SIZE}.");
            return;
        }

        player.User.SetChatFontSize(fontSize);
        player.ConfigChat();
        player.SendMessage(MessageType.Success, $"Tamanho da fonte do chat alterado para {fontSize}.");
    }

    [Command("pagesize", "/pagesize (tamanho)")]
    public static void CMD_pagesize(MyPlayer player, int pageSize)
    {
        if (pageSize < Constants.MIN_CHAT_LINES || pageSize > Constants.MAX_CHAT_LINES)
        {
            player.SendMessage(MessageType.Error, $"Quantidade de linhas do chat deve ser entre {Constants.MIN_CHAT_LINES} e {Constants.MAX_CHAT_LINES}.");
            return;
        }

        player.User.SetChatLines(pageSize);
        player.ConfigChat();
        player.SendMessage(MessageType.Success, $"Quantidade de linhas do chat alterada para {pageSize}.");
    }

    [Command("alterarnumero", "/alterarnumero (número)")]
    public async Task CMD_alterarnumero(MyPlayer player, uint number)
    {
        if (number.ToString().Length < 4 || number.ToString().Length > 7)
        {
            player.SendNotification(NotificationType.Error, "Número deve ter entre 4 e 7 caracteres.");
            return;
        }

        if (player.User.NumberChanges == 0)
        {
            player.SendNotification(NotificationType.Error, "Você não possui uma mudança de número.");
            return;
        }

        var cellphoneItem = player.Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.Cellphone && x.InUse);
        if (cellphoneItem is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está com um celular equipado.");
            return;
        }

        if (await Functions.CheckIfCellphoneExists(number))
        {
            player.SendNotification(NotificationType.Error, "Número já está sendo utilizado.");
            return;
        }

        var oldNumber = cellphoneItem.Subtype;

        cellphoneItem.SetSubtype(number);
        player.Character.SetCellphone(cellphoneItem.Subtype);
        await player.ConfigureCellphone();

        player.User.RemoveNumberChanges();
        await player.Save();

        var phoneNumber = new PhoneNumber();
        phoneNumber.Create(player.Character.Cellphone);

        var context = Functions.GetDatabaseContext();
        await context.PhonesNumbers.AddAsync(phoneNumber);
        await context.SaveChangesAsync();

        await player.WriteLog(LogType.Premium, $"Alterou número de {oldNumber} para {number} do celular {cellphoneItem.Id}", null);
        player.SendMessage(MessageType.Success, $"Você alterou o número do seu celular equipado para {player.Character.Cellphone}.");
    }

    [Command("caminhada", "/caminhada (tipo)")]
    public static void CMD_caminhada(MyPlayer player, byte style)
    {
        if (player.GetCurrentPremium() == UserPremium.None)
        {
            player.SendMessage(MessageType.Error, "Você não é premium.");
            return;
        }

        if (!Enum.IsDefined(typeof(CharacterWalkStyle), style))
        {
            var max = Enum.GetValues<CharacterWalkStyle>().Max();
            player.SendMessage(MessageType.Error, $"Tipo inválido. Use entre 1 e {(byte)max}.");
            return;
        }

        player.Character.SetWalkStyle((CharacterWalkStyle)style);
        player.SetMovement();
        player.SendMessage(MessageType.Success, $"Você alterou seu estilo de caminhada para {style}.");
    }

    [Command("booster")]
    public async Task CMD_booster(MyPlayer player)
    {
        var response = await player.CheckDiscordBooster();
        if (!response.Item1)
        {
            player.SendMessage(MessageType.Error, response.Item2);
            return;
        }

        player.SendMessage(MessageType.Success, response.Item2);
    }

    [Command("stats")]
    public async Task CMD_stats(MyPlayer player)
    {
        var context = Functions.GetDatabaseContext();

        var paycheck = await player.Paycheck(true);

        var vehicles = await context.Vehicles.Where(x => x.CharacterId == player.Character.Id && !x.Sold).ToListAsync();
        var currentPremium = player.GetCurrentPremium();

        var response = new CharacterInfoResponse
        {
            FactionId = player.Character.FactionId,
            Staff = player.User.Staff,
            Premium = $"{currentPremium.GetDisplay()}{(currentPremium != UserPremium.None ? $" (até {player.Character.PremiumValidDate ?? player.User.PremiumValidDate})" : string.Empty)}",
            Armor = player.GetArmor(),
            Health = player.GetHealth(),
            Bank = player.Character.Bank,
            Companies = player.Companies.Select(x => new CharacterInfoResponse.Company
            {
                Name = x.Name,
                Owner = x.CharacterId == player.Character.Id,
            }),
            Name = player.Character.Name,
            ConnectedTime = player.Character.ConnectedTime,
            CurrentDate = DateTime.Now,
            FactionName = player.Faction?.Name ?? "N/A",
            RankName = player.FactionRank?.Name ?? "N/A",
            User = player.User.Name,
            HelpRequestsAnswersQuantity = player.User.HelpRequestsAnswersQuantity,
            History = player.Character.History,
            Items = player.Items.Select(x => new CharacterInfoResponse.Item
            {
                Name = x.GetName(),
                Quantity = x.Quantity,
                Extra = x.GetExtra().Replace("<br/>", ", "),
            }),
            Invites = player.Invites.Select(x => new CharacterInfoResponse.Invite
            {
                Type = x.Type.GetDisplay(),
                WaitingTime = Functions.GetTimespan(x.Date),
            }),
            Job = player.Character.Job.GetDisplay(),
            NameChanges = player.User.NameChanges,
            PaycheckItems = paycheck.PaycheckItems,
            PaycheckValue = paycheck.PaycheckValue,
            PaycheckMultiplier = paycheck.PaycheckMultiplier,
            PlateChanges = player.User.PlateChanges,
            Properties = player.Properties.Select(x => new CharacterInfoResponse.Property
            {
                Number = x.Number,
                Address = x.Address,
                Price = x.Value,
                ProtectionLevel = x.ProtectionLevel,
            }),
            RegisterDate = player.Character.RegisterDate,
            Skin = ((PedHash)player.GetModel()).ToString(),
            StaffDutyTime = player.User.StaffDutyTime,
            StaffName = player.User.Staff.GetDisplay(),
            ThresoldDeath = $"{player.Character.ThresoldDeath}/100",
            ThresoldDeathReset = player.Character.ThresoldDeathEndDate.HasValue ? player.Character.ThresoldDeathEndDate.ToString()! : "N/A",
            UsingDrug = player.Character.DrugItemTemplateId.HasValue ? (Global.ItemsTemplates.FirstOrDefault(x => x.Id == player.Character.DrugItemTemplateId)?.Name ?? string.Empty) : "N/A",
            EndDrugEffects = player.Character.DrugEndDate.HasValue ? player.Character.DrugEndDate.ToString()! : "N/A",
            Vehicles = vehicles.Select(x => new CharacterInfoResponse.Vehicle
            {
                Model = x.Model,
                Plate = x.Plate,
                ProtectionLevel = x.ProtectionLevel,
                XMR = x.XMR,
            }),
            NumberChanges = player.User.NumberChanges,
            PremiumPoints = player.User.PremiumPoints,
            BankAccount = player.Character.BankAccount,
        };

        player.Emit("Stats:Open", Functions.Serialize(response));
    }

    [Command("ajuda")]
    public static void CMD_ajuda(MyPlayer player)
    {
        var commands = new List<CommandResponse>()
        {
            new("Teclas", "F2", "Ativa/desativa o cursor"),
            new("Teclas", "F7", "Habilita/desabilita HUD"),
            new("Teclas", "B", "Aponta/para de apontar o dedo estando fora de um veículo"),
            new("Teclas", "T", "Abrir caixa de texto para digitação no chat"),
            new("Teclas", "F", "Entra em veículo como motorista"),
            new("Teclas", "G", "Entra em veículo como passageiro"),
            new("Teclas", "L", "Tranca/destranca propriedades, veículos e portas"),
            new("Teclas", "J", "Ativa/desativa o controle de velocidade (cruise control)"),
            new("Teclas", "Z", "Ativa/desativa o modo de andar agachado"),
            new("Teclas", "Y", "Interage com um ponto de interação ou liga/desliga o motor de um veículo"),
            new("Teclas", "Shift + G", "Entra em veículo dando prioridade como passageiro externo"),
            new("Teclas", "O", "Abre/fecha lista de jogadores online"),
            new("Teclas", "I", "Abre o inventário"),
            new("Teclas", "SHIFT", "Segurar para ativar o modo drift de um veículo"),
            new("Teclas", "K", "Entra/sai de garagens com um veículo"),
            new("Teclas", "HOME", "Abre o celular"),
            new("Teclas", "F6", "Alterar modo de disparo de uma arma"),
            new("Teclas", "F8", "Tira uma screenshot"),
            new("Geral", "/stats", "Exibe as informações do seu personagem"),
            new("Geral", "/ajuda", "Exibe os comandos do servidor"),
            new("Geral", "/configuracoes /config", "Exibe as suas configurações"),
            new("Geral", "/tela", "Exibe um fundo com a cor de fundo configurada na tela"),
            new("Geral", "/id", "Procura o ID de um personagem"),
            new("Geral", "/aceitar /ac", "Aceita um convite"),
            new("Geral", "/recusar /rc", "Recusa um convite"),
            new("Geral", "/revistar", "Solicita uma revista em um personagem"),
            new("Geral", "/comprar", "Compra uma propriedade"),
            new("Geral", "/emprego", "Pega um emprego"),
            new("Geral", "/sos", "Envia uma dúvida"),
            new("Geral", "/reportar", "Envia um report aos administradores em serviço"),
            new("Geral", "/ferimentos", "Visualiza os ferimentos de um personagem"),
            new("Geral", "/aceitarhospital", "Aceita o tratamento médico após estar ferido e é levado ao hospital"),
            new("Geral", "/aceitarck", "Aceita o CK no personagem"),
            new("Geral", "/mostraridentidade /mostrarid", "Mostra a identidade para um personagem"),
            new("Geral", "/mostrarlicenca /ml", "Mostra a licença de motorista para um personagem"),
            new("Geral", "/horas", "Exibe o horário"),
            new("Geral", "/limparchat", "Limpa o seu chat"),
            new("Geral", "/save", "Exibe sua posição e rotação ou do seu veículo no console"),
            new("Geral", "/dados", "Joga um dado cujo o resultado será um número aleatório"),
            new("Geral", "/moeda", "Joga uma moeda cujo o resultado será cara ou coroa"),
            new("Geral", "/levantar", "Levanta um jogador gravemente ferido somente de socos"),
            new("Geral", "/trocarpersonagem", "Desconecta do personagem atual e abre a seleção de personagens"),
            new("Geral", "/admins", "Exibe os administradores do servidor"),
            new("Geral", "/dl", "Ativa/desativa o DL"),
            new("Geral", "/timestamp", "Ativa/desativa o timestamp"),
            new("Geral", "/historicocriminal", "Visualiza o histórico criminal do seu personagem"),
            new("Geral", "/cancelarconvite /cc", "Cancela um convite"),
            new("Geral", "/atm", "Gerencia sua conta bancária em uma ATM"),
            new("Geral", "/infos", "Gerencia suas marcas de informações"),
            new("Geral", "/bocafumo", "Usa uma boca de fumo"),
            new("Geral", "/boombox", "Altera as configurações de uma boombox"),
            new("Geral", "/mic", "Fala em um microfone"),
            new("Geral", "/stopanim /sa", "Para as animações"),
            new("Geral", "/animacoes", "Abre o painel de animações"),
            new("Geral", "/alugarempresa", "Aluga uma empresa"),
            new("Geral", "/empresa", "Gerencia suas empresas"),
            new("Geral", "/pagar", "Entrega dinheiro para um personagem próximo"),
            new("Geral", "/modofoto", "Ativa/desativa o modo foto"),
            new("Geral", "/anuncios", "Exibe os anúncios em andamento"),
            new("Geral", "/carregar", "Carrega um jogador"),
            new("Geral", "/soltar", "Para de carregar um jogador"),
            new("Geral", "/grafitar", "Inicia um grafite"),
            new("Geral", "/rgrafite", "Remove um grafite de sua autoria"),
            new("Geral", "/maquiagem", "Edita sua maquiagem"),
            new("Geral", "/corpoinv", "Abre o inventário de um corpo"),
            new("Geral", "/corpoferimentos", "Visualiza os ferimentos de um corpo"),
            new("Chat OOC", "/pm", "Envia uma mensagem privada"),
            new("Chat OOC", "/b", "Chat OOC local"),
            new("Chat OOC", "/re", "Responde a última mensagem privada recebida"),
            new("Chat IC", "/g", "Grita"),
            new("Chat IC", "/baixo", "Fala baixo"),
            new("Chat IC", "/s", "Sussura"),
            new("Chat IC", "/me", "Interpretação de ações de um personagem"),
            new("Chat IC", "/do", "Interpretação do ambiente"),
            new("Chat IC", "/ame", "Interpretação de ações de um personagem"),
            new("Chat IC", "/ado", "Interpretação do ambiente"),
            new("Chat IC", "/autobaixo", "Ativa/desativa mensagens em tom baixo automaticamente"),
            new("Chat IC", "/para /p", "Fala destinada a uma pessoa"),
            new("Chat IC", "/paragritar /pg", "Grito destinado a uma pessoa"),
            new("Chat IC", "/parabaixo /pb", "Fala baixo destinado a uma pessoa"),
            new("Chat IC", "/mebaixo /meb", "Interpretação de ações de um personagem com metade do range normal"),
            new("Chat IC", "/dobaixo /dob", "Interpretação do ambiente com metade do range normal"),
            new("Chat IC", "/mealto /mea", "Interpretação de ações de um personagem com dobro do range normal"),
            new("Chat IC", "/doalto /doa", "Interpretação do ambiente com metade do dobro normal"),
            new("Chat IC", "/cs /cw", "Sussura para todos no veículo"),
            new("Celular", "/celular /cel", "Abre o celular"),
            new("Celular", "/sms", "Envia um SMS"),
            new("Celular", "/ligar", "Liga para um número"),
            new("Celular", "/atender", "Atende a ligação"),
            new("Celular", "/an", "Envia um anúncio"),
            new("Celular", "/gps", "Busca a localização de uma propriedade"),
            new("Celular", "/temperatura", "Visualiza a temperatura e o clima atual"),
            new("Rádio Comunicador", "/canal", "Troca os canais de rádio"),
            new("Rádio Comunicador", "/r", "Fala no canal de rádio 1"),
            new("Rádio Comunicador", "/r2", "Fala no canal de rádio 2"),
            new("Rádio Comunicador", "/r3", "Fala no canal de rádio 3"),
            new("Rádio Comunicador", "/r4", "Fala no canal de rádio 4"),
            new("Rádio Comunicador", "/r5", "Fala no canal de rádio 5"),
            new("Rádio Comunicador", "/rbaixo", "Fala baixo no canal de rádio 1"),
            new("Rádio Comunicador", "/rbaixo2", "Fala baixo no canal de rádio 2"),
            new("Rádio Comunicador", "/rbaixo3", "Fala baixo no canal de rádio 3"),
            new("Rádio Comunicador", "/rbaixo4", "Fala baixo no canal de rádio 4"),
            new("Rádio Comunicador", "/rbaixo5", "Fala baixo no canal de rádio 5"),
            new("Rádio Comunicador", "/rme", "Interpretação de ações de um personagem no canal de rádio 1"),
            new("Rádio Comunicador", "/rme2", "Interpretação de ações de um personagem no canal de rádio 2"),
            new("Rádio Comunicador", "/rme3", "Interpretação de ações de um personagem no canal de rádio 3"),
            new("Rádio Comunicador", "/rme4", "Interpretação de ações de um personagem no canal de rádio 4"),
            new("Rádio Comunicador", "/rme5", "Interpretação de ações de um personagem no canal de rádio 5"),
            new("Rádio Comunicador", "/rdo", "Interpretação do ambiente no canal de rádio 1"),
            new("Rádio Comunicador", "/rdo2", "Interpretação do ambiente no canal de rádio 2"),
            new("Rádio Comunicador", "/rdo3", "Interpretação do ambiente no canal de rádio 3"),
            new("Rádio Comunicador", "/rdo4", "Interpretação do ambiente no canal de rádio 4"),
            new("Rádio Comunicador", "/rdo5", "Interpretação do ambiente no canal de rádio 5"),
            new("Propriedades", "/pvender", "Vende uma propriedade para um personagem"),
            new("Propriedades", "/pvendergoverno", "Venda uma propriedade para o governo"),
            new("Propriedades", "/armazenamento", "Visualiza o armazenamento de uma propriedade"),
            new("Propriedades", "/pchave", "Cria uma chave cópia de uma propriedade"),
            new("Propriedades", "/pfechadura", "Altera a fechadura de uma propriedade"),
            new("Propriedades", "/pupgrade", "Realiza atualições na propriedade"),
            new("Propriedades", "/arrombar", "Arromba a porta de uma propriedade"),
            new("Propriedades", "/roubarpropriedade", "Rouba uma propriedade"),
            new("Propriedades", "/pliberar", "Libera uma propriedade roubada"),
            new("Propriedades", "/mobilias", "Gerencia as mobílias de uma propriedade"),
            new("Propriedades", "/propnoclip", "Ativa/desativa a câmera livre para mobiliar"),
            new("Propriedades", "/pboombox", "Altera as configurações de uma saída de áudio da propriedade"),
            new("Propriedades", "/tv", "Altera as configurações de uma TV da propriedade"),
            new("Veículos", "/vestacionar", "Estaciona um veículo"),
            new("Veículos", "/vlista", "Mostra seus veículos"),
            new("Veículos", "/vvender", "Vende um veículo para outro personagem"),
            new("Veículos", "/portamalas", "Gerencia o porta-malas de um veículo"),
            new("Veículos", "/capo", "Abre/fecha o capô de um veículo"),
            new("Veículos", "/vporta", "Abre/fecha a porta de um veículo"),
            new("Veículos", "/abastecer", "Abastece um veículo em um posto de combustível"),
            new("Veículos", "/danos", "Visualiza os danos de um veículo"),
            new("Veículos", "/velmax", "Define a velocidade máxima de um veículo"),
            new("Veículos", "/janela /janelas /ja", "Abre/fecha a janela de um veículo"),
            new("Veículos", "/vchave", "Cria uma chave cópia de um veículo"),
            new("Veículos", "/vfechadura", "Altera a fechadura de um veículo"),
            new("Veículos", "/xmr", "Altera as configurações do XMR"),
            new("Veículos", "/quebrartrava /picklock", "Quebra a trava de um veículo"),
            new("Veículos", "/colocar", "Coloca um jogador em um veículo"),
            new("Veículos", "/retirar", "Retira de um veículo"),
            new("Veículos", "/reparar", "Conserta um veículo na central de mecânicos quando não há mecânicos em serviço"),
            new("Veículos", "/ligacaodireta /hotwire", "Faz ligação direta em um veículo"),
            new("Veículos", "/desmanchar", "Desmancha um veículo"),
            new("Veículos", "/rebocar", "Rebocar um veículo"),
            new("Veículos", "/rebocaroff", "Solta um veículo"),
            new("Veículos", "/vtransferir", "Transfere um veículo para outro personagem"),
            new("Veículos", "/desembaralhar /desem", "Desembaralha uma palavra da ligação direta em um veículo"),
            new("Veículos", "/helif", "Congela/descongela um helicóptero"),
            new("Veículos", "/valugar", "Aluga um veículo de emprego"),
            new("Veículos", "/motor", "Liga/desliga o motor de um veículo"),
            new("Transmissão", "/transmissao", "Acompanha/para de acompanhar uma transmissão"),
            new("Transmissão", "/t", "Fala em uma transmissão se possuir permissão"),
            new("Contrabando", "/contrabando", "Vende itens para um contrabandista"),
            new("Geral", "/armacorpo /armac", "Altera posicionamento da arma acoplada ao corpo"),
            new("Geral", "/fontsize", "Altera o tamanho da fonte do chat"),
            new("Geral", "/pagesize", "Altera a quantidade de linhas do chat"),
            new("Propriedades", "/phora /ptime", "Altera o horário da propriedade"),
            new("Propriedades", "/ptempo /pweather", "Altera o tempo da propriedade"),
            new("Geral", "/premium", "Abre o painel de gerenciamento Premium"),
            new("Geral", "/alterarnumero", "Altera o número do seu celular equipado"),
            new("Geral", "/caminhada", "Altera o estilo de caminhada"),
            new("Geral", "/booster", "Obtem recompensa por boostar o Discord principal do servidor"),
            new("Veículos", "/cinto", "Coloca/retira o cinto de segurança"),
            new("Geral", "/doar", "Remove a quantidade de dinheiro especificada"),
            new("Geral", "/modoanim", "Edita sua posição/rotação na animação"),
            new("Geral", "/mascara", "Coloca/remove a máscara"),
            new("Geral", "/inventario /inv", "Abre o inventário"),
            new("Propriedades", "/fixloc", "Corrige sua localização dentro de uma propriedade"),
            new("Geral", "/outfits", "Abre a interface de outfits"),
            new("Geral", "/outfit", "Altera o outfit em uso"),
            new("Geral", "/corrigirvw", "Corrige seu VW"),
            new("Geral", "/atributos", "Define os seus atributos"),
            new("Geral", "/examinar", "Visualiza os atributos de um personagem próximo"),
            new("Geral", "/supporters", "Exibe os supporters do servidor"),
            new("Veículos", "/vtrancar", "Tranca/destranca um veículo"),
            new("Geral", "/cabelo", "Ativa/desativa o cabelo"),
            new("Geral", "/cancelarsos", "Cancela o seu SOS"),
            new("Geral", "/cancelarreport", "Cancela o seu report"),
            new("Geral", "/celulares", "Lista os seus números"),
            new("Geral", "/transferir", "Transfere o valor para uma conta bancária"),
            new("Geral", "/fixinvi", "Corrige a invisibilidade do seu personagem"),
        };

        if (player.Character.Job != CharacterJob.None)
        {
            commands.AddRange(
            [
                new("Emprego", "/sairemprego", "Sai do emprego"),
                new("Emprego", "/duty /trabalho", "Entra/sai de serviço"),
            ]);

            if (player.Character.Job == CharacterJob.TaxiDriver || player.Character.Job == CharacterJob.Mechanic)
                commands.AddRange(
                [
                    new("Emprego", "/chamadas", "Exibe as chamadas aguardando resposta"),
                    new("Emprego", "/atcha", "Atende uma chamada"),
                ]);

            if (player.Character.Job == CharacterJob.GarbageCollector)
                commands.AddRange(
                [
                    new("Emprego", "/pegarlixo", "Pega um saco de lixo em um ponto de coleta"),
                    new("Emprego", "/colocarlixo", "Coloca um saco de lixo em um caminhão de lixo"),
                ]);
            else if (player.Character.Job == CharacterJob.Trucker)
                commands.AddRange(
                [
                    new("Emprego", "/rotas", "Exibe as rotas disponíveis para trabalho"),
                    new("Emprego", "/carregarcaixas", "Carrega o seu veículo com as caixas da rota"),
                    new("Emprego", "/cancelarcaixas", "Devolve as caixas da rota"),
                    new("Emprego", "/entregarcaixas", "Entrega as caixas da rota em um ponto de entrega"),
                ]);
        }

        if (player.Character.FactionId.HasValue)
        {
            commands.AddRange(
            [
                new(Globalization.FACTION, "/f", "Chat OOC da facção"),
                new(Globalization.FACTION, "/faccao", "Abre o painel de gerenciamento da facção"),
                new(Globalization.FACTION, "/sairfaccao", "Sai da facção"),
            ]);

            if (player.Faction!.Government)
                commands.AddRange(
                [
                    new("Teclas", "Q", "Desliga/liga som da sirene de um veículo"),
                    new(Globalization.FACTION, "/rapel", "Desce de rapel dos assentos traseiros de um helicóptero apropriado"),
                    new(Globalization.FACTION, "/meg", "Fala no megafone"),
                    new(Globalization.FACTION, "/equipar", "Pega equipamentos"),
                    new(Globalization.FACTION, "/ploc", "Atualiza a localização da sua unidade"),
                    new(Globalization.FACTION, "/ps", "Atualiza o status da sua unidade"),
                    new("Teclas", "F3", "Ativa/desativa a câmera do helicóptero"),
                ]);

            if (player.Faction!.HasWalkieTalkie)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/dep", "Fala no canal interdepartamental"),
                    new(Globalization.FACTION, "/setdep", "Define qual o departamento de destino no canal interdepartamental"),
                ]);

            if (player.Faction!.HasDuty)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/mostrardistintivo", "Mostra seu distintivo para um personagem"),
                    new(Globalization.FACTION, "/duty /trabalho", "Entra/sai de trabalho"),
                    new(Globalization.FACTION, "/uniforme", "Abre o menu de seleção de roupas"),
                    new(Globalization.FACTION, "/usaruniforme", "Veste um uniforme pré-definido"),
                    new(Globalization.FACTION, "/freparar", "Conserta veículos da facção"),
                ]);

            if (player.Faction!.HasBarriers)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/br", "Cria uma barreira"),
                    new(Globalization.FACTION, "/rb", "Remove uma barreira"),
                    new(Globalization.FACTION, "/rballme", "Remove todas barreiras criadas por você"),
                    new(Globalization.FACTION, "/rball", "Remove todas barreiras"),
                ]);

            if (player.Faction!.CanSeizeVehicles)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/apreender", "Apreende um veículo"),
                ]);

            if (player.Faction!.HasMDC)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/mdc", "Abre o MDC"),
                ]);

            if (player.Faction.Type == FactionType.Police)
                commands.AddRange(
                [
                    new("Teclas", "Botão Esquerdo do Mouse", "Ativa/desativa a luz do helicóptero enquanto a câmera estiver ativa"),
                    new("Teclas", "Botão Direito do Mouse", "Altera o modo da visão do helicóptero enquanto a câmera estiver ativa"),
                    new(Globalization.FACTION, "/algemar", "Algema/desalgema um personagem"),
                    new(Globalization.FACTION, "/radar", "Coloca um radar de velocidade"),
                    new(Globalization.FACTION, "/radaroff", "Remove um radar de velocidade"),
                    //new(Globalization.FACTION, "/spotlight /holofote", "Ativa/desativa o holofote de um veículo"),
                    new(Globalization.FACTION, "/confisco", "Cria um registro de confisco"),
                    new(Globalization.FACTION, "/vpegarpregos", "Pega um tapete de pregos do porta-malas de um veículo"),
                    new(Globalization.FACTION, "/colocarpregos", "Coloca um tapete de pregos no chão"),
                    new(Globalization.FACTION, "/pegarpregos", "Pega um tapete de pregos do chão"),
                    new(Globalization.FACTION, "/vcolocarpregos", "Coloca um tapete de pregos no porta-malas de um veículo"),
                    new(Globalization.FACTION, "/recolhercorpo", "Envia um corpo para o necrotério"),
                ]);
            else if (player.Faction.Type == FactionType.Firefighter)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/curar", "Cura um personagem ferido"),
                ]);
            else if (player.Faction.Type == FactionType.Media)
                commands.AddRange(
                [
                    new(Globalization.FACTION, "/transmissao", "Inicia/para uma transmissão"),
                    new(Globalization.FACTION, "/tplayer", "Convida/expulsa alguém para uma transmissão"),
                ]);

            if (player.FactionFlags.Count > 0)
            {
                if (player.FactionFlags.Contains(FactionFlag.BlockChat))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.BlockChat.GetDisplay()}", "/blockf", "Bloqueia/desbloqueia o chat OOC da facção"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.GovernmentAdvertisement))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.GovernmentAdvertisement.GetDisplay()}", "/gov", "Envia um anúncio governamental da facção"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.HQ))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.HQ.GetDisplay()}", "/hq", "Envia uma mensagem no rádio da facção como dispatcher"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.Storage))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.Storage.GetDisplay()}", "/farmazenamento", "Usa o armazenamento da facção"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.Uniform))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.Uniform.GetDisplay()}", "/adduniforme", "Cria um uniforme com as roupas que está vestindo"),
                        new($"Flag Facção {FactionFlag.Uniform.GetDisplay()}", "/deluniforme", "Remove um uniforme"),
                        new($"Flag Facção {FactionFlag.Uniform.GetDisplay()}", "/criaruniforme", "Cria um uniforme através do menu de seleção"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.FireManager))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.FireManager.GetDisplay()}", "/incendios", "Abre o painel de gerenciamento de incêndios"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.RespawnVehicles))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.RespawnVehicles.GetDisplay()}", "/ftow", "Respawna todos os veículos da facção sem ocupantes"),
                    ]);

                if (player.FactionFlags.Contains(FactionFlag.InviteMember))
                    commands.AddRange(
                    [
                        new($"Flag Facção {FactionFlag.InviteMember.GetDisplay()}", "/convidar", "Convida um jogador para sua facção"),
                    ]);
            }
        }

        if (player.StaffFlags.Count > 0)
        {
            if (player.StaffFlags.Contains(StaffFlag.Doors))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Doors.GetDisplay()}", "/portas", "Abre o painel de gerenciamento de portas"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Jobs))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Jobs.GetDisplay()}", "/empregos", "Abre o painel de gerenciamento de empregos"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Factions))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Factions.GetDisplay()}", "/faccoes", "Abre o painel de gerenciamento de facções"),
                    new($"Flag Staff {StaffFlag.Factions.GetDisplay()}", "/contrabandistas", "Abre o painel de gerenciamento de contrabandistas"),
                    new($"Flag Staff {StaffFlag.Factions.GetDisplay()}", "/setfaccao", "Define a facção de um jogador"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.FactionsStorages))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.FactionsStorages.GetDisplay()}", "/aarmazenamentos", "Abre o painel de gerenciamento de armazenamentos"),
                    new($"Flag Staff {StaffFlag.FactionsStorages.GetDisplay()}", "/aequipamentos", "Abre o painel de gerenciamento de equipamentos"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Properties))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/eprop", "Edita uma propriedade"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/int", "Visualiza um tipo de interior"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/aarmazenamento", "Visualiza o armazenamento de uma propriedade"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/criarpropriedade /cprop", "Cria uma propriedade"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/criarapartamento /cap", "Cria um apartamento"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/irprop", "Vai para uma propriedade"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/delprop", "Deleta uma propriedade"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/rdonoprop", "Remove o dono da propriedade"),
                    new($"Flag Staff {StaffFlag.Properties.GetDisplay()}", "/entradasprop", "Edita as entradas de uma propriedade"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.GiveItem))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.GiveItem.GetDisplay()}", "/daritem", "Dá um item para um personagem"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.CrackDens))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.CrackDens.GetDisplay()}", "/bocasfumo", "Abre o painel de gerenciamento de bocas de fumo"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.TruckerLocations))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.TruckerLocations.GetDisplay()}", "/acaminhoneiro", "Abre o painel de gerenciamento de localizações de caminhoneiros"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Companies))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Companies.GetDisplay()}", "/empresas", "Abre o painel de gerenciamento de empresas"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Vehicles))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Vehicles.GetDisplay()}", "/veiculos", "Abre o painel de gerenciamento de veículos"),
                    new($"Flag Staff {StaffFlag.Vehicles.GetDisplay()}", "/atunar", "Realiza modificações em um veículo"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Items))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Items.GetDisplay()}", "/itens", "Abre o painel de gerenciamento de itens"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.VehicleMaintenance))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.VehicleMaintenance.GetDisplay()}", "/areparar", "Conserta um veículo"),
                    new($"Flag Staff {StaffFlag.VehicleMaintenance.GetDisplay()}", "/amotor", "Liga/desliga o motor de um veículo"),
                    new($"Flag Staff {StaffFlag.VehicleMaintenance.GetDisplay()}", "/aabastecer", "Abastece um veículo"),
                    new($"Flag Staff {StaffFlag.VehicleMaintenance.GetDisplay()}", "/aveiculo", "Cria um veículo temporário"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Drugs))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Drugs.GetDisplay()}", "/drogas", "Abre o painel de gerenciamento de drogas"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Spots))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Spots.GetDisplay()}", "/pontos", "Abre o painel de gerenciamento de pontos"),
                    new($"Flag Staff {StaffFlag.Spots.GetDisplay()}", "/criarponto", "Cria um ponto"),
                    new($"Flag Staff {StaffFlag.Spots.GetDisplay()}", "/delponto", "Deleta o ponto mais próximo"),
                ]);

            if (player.StaffFlags.Contains(StaffFlag.Spots))
                commands.AddRange(
                [
                    new($"Flag Staff {StaffFlag.Events.GetDisplay()}", "/tempo", "Define um tempo e temperatura fixos"),
                    new($"Flag Staff {StaffFlag.Events.GetDisplay()}", "/anrp", "Envia um anúncio de roleplay"),
                    new($"Flag Staff {StaffFlag.Events.GetDisplay()}", "/rtempo", "Remove o tempo e ativa a sincronização automática"),
                    new($"Flag Staff {StaffFlag.Events.GetDisplay()}", "/enome", "Define um nome temporário para seu personagem"),
                    new($"Flag Staff {StaffFlag.Events.GetDisplay()}", "/skin", "Altera a skin de um jogador"),
                    new($"Flag Staff {StaffFlag.Events.GetDisplay()}", "/objetos", "Abre o painel de gerenciamento de objetos"),
                ]);
        }

        if (player.User.Staff >= UserStaff.ServerSupport)
        {
            var display = UserStaff.ServerSupport.GetDisplay();
            commands.AddRange(
            [
                new(display, "/at", "Atende um SOS"),
                new(display, "/sc", "Envia mensagem no chat support"),
                new(display, "/listasos", "Lista os SOS pendentes"),
                new(display, "/csos", "Converte um SOS para report"),
            ]);
        }

        if (player.User.Staff >= UserStaff.JuniorServerAdmin)
        {
            var display = UserStaff.JuniorServerAdmin.GetDisplay();
            commands.AddRange(
            [
                new(display, "/checar", "Visualiza as informações de um personagem"),
                new(display, "/usuario", "Visualiza as informações de um usuário"),
                new(display, "/ir", "Vai a um personagem"),
                new(display, "/trazer", "Traz um personagem"),
                new(display, "/tp", "Teleporta um personagem para outro"),
                new(display, "/a", "Envia mensagem no chat administrativo"),
                new(display, "/kick", "Expulsa um personagem"),
                new(display, "/irveh", "Vai a um veículo"),
                new(display, "/trazerveh", "Traz um veículo"),
                new(display, "/aduty /atrabalho", "Entra/sai de serviço administrativo"),
                new(display, "/spec", "Observa um personagem"),
                new(display, "/specoff", "Para de observar um personagem"),
                new(display, "/aferimentos", "Visualiza os ferimentos de um personagem"),
                new(display, "/aestacionar", "Estaciona um veículo"),
                new(display, "/acurar", "Cura um personagem ferido"),
                new(display, "/adanos", "Visualiza os danos de um veículo"),
                new(display, "/checarveh", "Visualiza o proprietário de um veículo"),
                new(display, "/proximo /prox", "Lista os itens que estão próximos"),
                new(display, "/ainfos", "Gerencia todas marcas de informações"),
                new("Teclas", "F5", "Ativa/desativa o no-clip"),
                new(display, "/ban", "Bane um jogador"),
                new(display, "/pos", "Vai até a posição"),
                new(display, "/ooc", "Chat OOC Global"),
                new(display, "/waypoint", "Teleporta até o waypoint marcado no mapa"),
                new(display, "/aviso", "Aplica um aviso em um personagem online"),
                new(display, "/ajail", "Prende um personagem online administrativamente"),
                new(display, "/rajail", "Solta um personagem da prisão administrativa"),
                new(display, "/vflip", "Descapota um veículo"),
                new(display, "/grafites", "Gerencia os grafites"),
                new(display, "/arecolhercorpo", "Envia um corpo para o necrotério"),
                new(display, "/specs", "Lista quem está observando um personagem"),
                new(display, "/deletarsangue", "Deleta todas as amostras de sangue do chão na distância informada"),
                new(display, "/deletarcapsulas", "Deleta todas as cápsulas do chão na distância informada"),
                new(display, "/irls", "Vai para o spawn inicial"),
                new(display, "/setvw", "Define o VW de um jogador"),
                new(display, "/debug", "Habilita a visão de depuração"),
                new(display, "/audios", "Lista os áudios ativos"),
                new(display, "/congelar", "Congela/descongela um jogador"),
                new(display, "/fixpos", "Corrige a posição do personagem para evitar crash ao logar"),
                new(display, "/vertela", "Visualiza informações da tela de um jogador"),
                new(display, "/anametag", "Ativa/desativa a nametag à distância"),
                new(display, "/ar", "Atende um report"),
                new(display, "/listareport", "Lista os reports pendentes"),
                new(display, "/creport", "Converte um report para SOS"),
                new(display, "/checarafk", "Checa se o jogador está AFK"),
                new(display, "/afks", "Lista os jogadores que estão AFK"),
                new(display, "/mascarados", "Lista os jogadores mascarados"),
            ]);
        }

        if (player.User.Staff >= UserStaff.ServerAdminI)
        {
            var display = UserStaff.ServerAdminI.GetDisplay();
            commands.AddRange(
            [
                new(display, "/aremovercorpo", "Remove um corpo"),
                new(display, "/vida", "Altera a vida de um jogador"),
            ]);
        }

        if (player.User.Staff >= UserStaff.ServerAdminII)
        {
            var display = UserStaff.ServerAdminII.GetDisplay();
            commands.AddRange(
            [
                new(display, "/idade", "Altera a idade de um personagem"),
                new(display, "/colete", "Altera o colete de um jogador"),
            ]);
        }

        if (player.User.Staff >= UserStaff.SeniorServerAdmin)
        {
            var display = UserStaff.SeniorServerAdmin.GetDisplay();
            commands.AddRange(
            [
                new(display, "/alteracoesplaca", "Lista as solicitações de alterações de placa"),
                new(display, "/aprovarplaca", "Aprova uma alteração de placa"),
                new(display, "/reprovarplaca", "Reprova uma alteração de placa"),
                new(display, "/aspawn", "Spawna um veículo"),
            ]);
        }

        if (player.User.Staff >= UserStaff.LeadServerAdmin)
        {
            var display = UserStaff.LeadServerAdmin.GetDisplay();
            commands.AddRange(
            [
                new(display, "/limparchatgeral", "Limpa o chat de todos os personagens"),
                new(display, "/hs", "Envia mensagem no chat head staff"),
                new(display, "/raviso", "Remove o aviso mais recente de um personagem"),
                new(display, "/blocktogstaff", "Bloqueia/desbloqueia os togs administrativos"),
                new(display, "/lobby", "Lista os jogadores que não estão spawnados"),
            ]);
        }

        if (player.User.Staff >= UserStaff.ServerManager)
        {
            var display = UserStaff.ServerManager.GetDisplay();
            commands.AddRange(
            [
                new(display, "/lsp", "Adiciona LS Points para um usuário"),
                new(display, "/concessionarias", "Abre o painel de gerenciamento de concessionárias"),
                new(display, "/nome", "Altera o nome permanente de um jogador"),
            ]);
        }

        commands = [.. commands.OrderBy(x => x.Category).ThenBy(x => x.Name)];
        player.Emit("Help:Open", Functions.Serialize(commands));
    }

    [Command("configuracoes", Aliases = ["config"])]
    public static void CMD_configuracoes(MyPlayer player)
    {
        var settings = new UCPSettingsRequest(player.User.TimeStampToggle, player.User.AnnouncementToggle,
            player.User.PMToggle, player.User.FactionChatToggle, player.StaffChatToggle, player.User.ChatFontType,
            player.User.ChatLines, player.User.ChatFontSize, player.User.FactionToggle, player.User.VehicleTagToggle,
            player.User.ChatBackgroundColor, player.User.ShowNametagId, player.User.AmbientSoundToggle,
            player.User.FreezingTimePropertyEntrance, player.User.ShowOwnNametag, player.StaffToggle,
            player.FactionWalkieTalkieToggle, player.User.ReceiveSMSDiscord);
        player.Emit("Settings:Open", Functions.Serialize(settings), Functions.Serialize(new
        {
            IsPremium = player.GetCurrentPremium() != UserPremium.None,
            IsStaff = player.User.Staff >= UserStaff.JuniorServerAdmin,
            HasFaction = player.Character.FactionId.HasValue,
        }), Functions.Serialize(
                Enum.GetValues<UserReceiveSMSDiscord>()
                .Select(x => new
                {
                    Value = x,
                    Label = x.GetDisplay(),
                })
            ));
    }

    [RemoteEvent(nameof(SaveSettings))]
    public static void SaveSettings(Player playerParam, string settingsJson)
    {
        var player = Functions.CastPlayer(playerParam);
        var settings = Functions.Deserialize<UCPSettingsRequest>(settingsJson);
        if (settings.ChatFontSize < Constants.MIN_CHAT_FONT_SIZE || settings.ChatFontSize > Constants.MAX_CHAT_FONT_SIZE)
        {
            player.SendNotification(NotificationType.Error, $"Tamanho da fonte do chat deve ser entre {Constants.MIN_CHAT_FONT_SIZE} e {Constants.MAX_CHAT_FONT_SIZE}.");
            return;
        }

        if (settings.ChatLines < Constants.MIN_CHAT_LINES || settings.ChatLines > Constants.MAX_CHAT_LINES)
        {
            player.SendNotification(NotificationType.Error, $"Quantidade de linhas do chat deve ser entre {Constants.MIN_CHAT_LINES} e {Constants.MAX_CHAT_LINES}.");
            return;
        }

        if (settings.FreezingTimePropertyEntrance < 0 || settings.FreezingTimePropertyEntrance > 10)
        {
            player.SendNotification(NotificationType.Error, "Segundos Congelamento Entrada Propriedade deve ser entre 0 e 10.");
            return;
        }

        if (settings.StaffToggle || settings.StaffChatToggle)
        {
            if (Global.StaffToggleBlocked)
            {
                player.SendNotification(NotificationType.Error, "Os togs da staff estão bloqueados, não é possível desativar no momento.");
                return;
            }
        }

        player.User.UpdateSettings(settings.TimeStampToggle, settings.AnnouncementToggle,
            player.GetCurrentPremium() != UserPremium.None && settings.PMToggle,
            settings.FactionChatToggle, settings.ChatFontType, settings.ChatLines, settings.ChatFontSize,
            settings.FactionToggle, settings.VehicleTagToggle, settings.ChatBackgroundColor, settings.ShowNametagId, settings.AmbientSoundToggle,
            settings.FreezingTimePropertyEntrance, settings.ShowOwnNametag, settings.ReceiveSMSDiscord);
        player.StaffChatToggle = settings.StaffChatToggle;
        player.StaffToggle = settings.StaffToggle;
        player.FactionWalkieTalkieToggle = settings.FactionWalkieTalkieToggle;
        player.ConfigChat();
        player.NametagsConfig();
        player.DlConfig();
        player.ToggleAmbientSound();
        player.SendNotification(NotificationType.Success, "Configurações gravadas com sucesso.");
    }

    [Command("doar", "/doar (quantidade)")]
    public async Task CMD_doar(MyPlayer player, int quantity)
    {
        if (quantity <= 0)
        {
            player.SendMessage(MessageType.Error, "Valor inválido.");
            return;
        }

        if (player.Money < quantity)
        {
            player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, quantity));
            return;
        }

        await player.RemoveMoney(quantity);
        await player.WriteLog(LogType.Donate, $"${quantity:N0}", null);
        player.SendMessage(MessageType.Success, $"Você doou ${quantity:N0}.");
    }

    [Command("modoanim")]
    public static void CMD_modoanim(MyPlayer player)
    {
        var flags = (AnimationFlags)(player.CurrentAnimFlag ?? 0);
        var hasAllowPlayerControl = (flags & AnimationFlags.AllowPlayerControl) == AnimationFlags.AllowPlayerControl;
        if (!player.CurrentAnimFlag.HasValue || hasAllowPlayerControl)
        {
            player.SendMessage(MessageType.Error, "Você não está usando uma animação estática.");
            return;
        }

        player.Emit("EditAnimation");
    }

    [Command("mascara")]
    public async Task CMD_mascara(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Globalization.ACTIONS_BLOCKED_MESSAGE);
            return;
        }

        player.Masked = !player.Masked;
        if (player.Masked && player.Character.Mask == 0)
        {
            var context = Functions.GetDatabaseContext();
            player.Character.SetMask((await context.Characters.MaxAsync(x => x.Mask)) + 1);
            await player.Save();

            await player.WriteLog(LogType.Mask, player.Character.Mask.ToString(), null);
        }

        player.SetNametag();
        player.SendMessageToNearbyPlayers($"{(player.Masked ? "coloca" : "remove")} a máscara.", MessageCategory.Ame);
    }

    [Command("corrigirvw")]
    public async Task CMD_corrigirvw(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Globalization.ACTIONS_BLOCKED_MESSAGE);
            return;
        }

        if (player.PropertyNoClip)
        {
            await Functions.SendServerMessage($"{player.Character.Name} ({player.User.Name}) tentou usar o comando /corrigirvw enquanto estava com o no-clip ativado.", UserStaff.JuniorServerAdmin, true);
            player.SendMessage(MessageType.Error, "Você não pode corrigir seu VW enquanto está com o no-clip ativado.");
            return;
        }

        player.SetPosition(player.GetPosition(), 0, false);
        await player.WriteLog(LogType.Fix, "/corrigirvw", null);
        player.SendMessage(MessageType.Success, "Você corrigiu seu VW.");
    }

    [Command("atributos")]
    public static void CMD_atributos(MyPlayer player)
    {
        player.Emit("Attributes:Show", player.Character.Attributes, player.Character.Age);
    }

    [RemoteEvent(nameof(SaveAttributes))]
    public async Task SaveAttributes(Player playerParam, string attributes, int age)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            attributes ??= string.Empty;
            if (attributes.Length > 500)
            {
                player.SendNotification(NotificationType.Error, "Atributos devem ter até 500 caracteres.");
                return;
            }

            if (age < Constants.MIN_AGE || age > Constants.MAX_AGE)
            {
                player.SendNotification(NotificationType.Error, $"Idade deve ser entre {Constants.MIN_AGE} e {Constants.MAX_AGE}.");
                return;
            }

            if (age < player.Character.Age)
            {
                player.SendNotification(NotificationType.Error, "Não é possível reduzir a idade do seu personagem.");
                return;
            }

            player.Character.SetAttributes(attributes, age);
            await player.WriteLog(LogType.Attributes, $"{attributes} {age}", null);
            player.SetNametag();
            player.SendNotification(NotificationType.Success, "Atributos alterados.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("examinar", "/examinar (ID ou nome)")]
    public static void CMD_examinar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        var ageRange = "Fim";
        var ageEnd = Convert.ToInt32(target.Character.Age.ToString().Substring(1, 1));
        if (ageEnd <= 3)
            ageRange = "Início";
        else if (ageEnd <= 6)
            ageRange = "Meio";

        ageRange += $" dos {target.Character.Age.ToString()[..1]}0";

        player.SendMessage(MessageType.None, $"Aparência de {target.ICName}", Constants.MAIN_COLOR);
        player.SendMessage(MessageType.None, $"Faixa etária: {{#FFF}}{ageRange}", Constants.MAIN_COLOR);
        if (!string.IsNullOrWhiteSpace(target.Character.Attributes))
            player.SendMessage(MessageType.None, target.Character.Attributes);
    }

    [Command("supporters")]
    public static void CMD_supporters(MyPlayer player)
    {
        var supports = Global.SpawnedPlayers.Where(x => x.User.Staff == UserStaff.ServerSupport).OrderBy(x => x.User.Name);
        if (!supports.Any())
        {
            player.SendMessage(MessageType.Error, "Não há nenhum support online.");
            return;
        }

        player.SendMessage(MessageType.Title, $"SUPPORTS: {Constants.SERVER_NAME}");
        foreach (var support in supports)
            player.SendMessage(MessageType.None, $"{support.User.Name}{(player.User.Staff >= UserStaff.ServerSupport ? $" ({support.SessionId})" : string.Empty)}");
        player.SendMessage(MessageType.None, "Se precisar de ajuda de um Supporter, utilize o /sos.");
    }

    [Command("pegarneve")]
    public async Task CMD_pegarneve(MyPlayer player)
    {
        if (Global.WeatherInfo.WeatherType != Weather.XMAS)
        {
            player.SendMessage(MessageType.Error, "Não há neve no chão.");
            return;
        }

        player.GiveWeapon(WeaponHash.Snowball, 1);
        player.SendMessageToNearbyPlayers("pega uma bola de neve do chão.", MessageCategory.Ame);
        await player.WriteLog(LogType.General, "/pegarneve", null);
    }

    [Command("cabelo")]
    public async Task CMD_cabelo(MyPlayer player)
    {
        if (!player.ValidPed)
        {
            player.SendMessage(MessageType.Error, Globalization.INVALID_SKIN_MESSAGE);
            return;
        }

        player.UsingHair = !player.UsingHair;
        player.SetPersonalization();
        player.SendMessage(MessageType.Success, $"Você {(player.UsingHair ? string.Empty : "des")}ativou seu cabelo.");
        await player.WriteLog(LogType.General, "/cabelo", null);
    }

    [Command("fixinvi")]
    public async Task CMD_fixinvi(MyPlayer player)
    {
        player.Visible = true;
        player.SendMessage(MessageType.Success, "Você corrigiu sua visibilidade.");
        await player.WriteLog(LogType.Fix, "/fixinvi", null);
        player.SendMessage(MessageType.Error, "ATENÇÃO! Abusar desse comando é inadmissível.");
    }
}