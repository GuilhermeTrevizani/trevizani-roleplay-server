using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class CommandScript : Script
{
    [Command(["id"], "Geral", "Procura o ID de um personagem", "(ID ou nome)", GreedyArg = true)]
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

    [Command(["aceitar", "ac"], "Geral", "Aceita um convite", "(tipo)")]
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
                    player.SendNotification(NotificationType.Error, Resources.YouAreNotCloseToThePlayer);
                    return;
                }

                if (!Guid.TryParse(invite.Value[0], out Guid propriedade) || !int.TryParse(invite.Value[1], out int value))
                    return;

                if (player.Money < value)
                {
                    player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, value));
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
                    player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
                    return;
                }

                player.CloseInventory();
                player.SendMessage(MessageType.Success, $"Você está sendo revistado por {target.ICName}.");
                target.ShowInventory(InventoryShowType.Inspect, player.ICName, player.GetInventoryItemsJson(), false, player.Character.Id);
                break;
            case InviteType.VehicleSell:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
                    return;
                }

                if (!Guid.TryParse(invite.Value[0], out Guid veiculo) || !int.TryParse(invite.Value[1], out int vehicleValue))
                    return;

                if (player.Money < vehicleValue)
                {
                    player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, vehicleValue));
                    break;
                }

                var veh = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == veiculo);
                if (veh is null)
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

                await veh.VehicleDB.ChangeOwner(player.Character.Id);

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
                if (company is null)
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
                    player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
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

                await transferVehicle.VehicleDB.ChangeOwner(player.Character.Id);

                context.Vehicles.Update(transferVehicle.VehicleDB);
                await context.SaveChangesAsync();

                player.SendMessage(MessageType.Success, $"O veículo {transferVehicle.VehicleDB.Model.ToUpper()} {transferVehicle.VehicleDB.Plate} de {target.ICName} foi transferido para você.");
                target.SendMessage(MessageType.Success, $"Você transferiu o veículo {transferVehicle.VehicleDB.Model.ToUpper()} {transferVehicle.VehicleDB.Plate} para {player.ICName}.");
                await target.WriteLog(LogType.Sell, $"/vtransferir {transferVehicle.Identifier} {transferVehicle.VehicleDB.Id}", player);
                break;
            case InviteType.Carry:
                if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
                {
                    player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
                    return;
                }

                player.StartBeingCarried(target);
                await player.WriteLog(LogType.General, "/carregar não ferido", target);
                break;
        }

        player.Invites.RemoveAll(x => x.Type == (InviteType)type);
    }

    [Command(["recusar", "rc"], "Geral", "Recusa um convite", "(tipo)")]
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

    [Command(["revistar"], "Geral", "Solicita uma revista em um personagem", "(ID ou nome)")]
    public static void CMD_revistar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
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

    [Command(["comprar"], "Geral", "Compra uma propriedade")]
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
            player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, property.Value));
            return;
        }

        await player.RemoveMoney(property.Value);
        await property.ChangeOwner(player.Character.Id);

        await player.WriteLog(LogType.Buy, $"Comprar Propriedade {property.FormatedAddress} ({property.Id}) por {property.Value}", null);
        player.SendMessage(MessageType.Success, $"Você comprou {property.FormatedAddress} por ${property.Value:N0}.");
    }

    [Command(["sos"], "Geral", "Envia uma dúvida", "(mensagem)", GreedyArg = true)]
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

        foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.Tester && !x.StaffToggle))
        {
            target.SendMessage(MessageType.Error, $"SOS de {player.Character.Name} ({player.SessionId}) ({player.User.Name})");
            target.SendMessage(MessageType.None, $"{message} {{{Constants.ERROR_COLOR}}}(/at {player.SessionId} /csos {player.SessionId})", "#B0B0B0");
        }

        player.SendMessage(MessageType.Success, "O seu SOS foi enviado e em breve será respondido por um Supporter!");
    }

    [Command(["cancelarsos"], "Geral", "Cancela o seu SOS")]
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

    [Command(["cancelarreport"], "Geral", "Cancela o seu report")]
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

    [Command(["reportar"], "Geral", "Envia um report aos administradores em serviço", "(mensagem)", GreedyArg = true)]
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

        foreach (var target in Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.GameAdmin && !x.StaffToggle))
        {
            target.SendMessage(MessageType.Error, $"Report de {player.Character.Name} ({player.SessionId}) ({player.User.Name})");
            target.SendMessage(MessageType.None, $"{message} {{{Constants.ERROR_COLOR}}}(/ar {player.SessionId} /creport {player.SessionId})", "#B0B0B0");
        }

        player.SendMessage(MessageType.Success, "O seu Report foi enviado e em breve será respondido por um administrador!");
    }

    [Command(["ferimentos"], "Geral", "Visualiza os ferimentos de um personagem", "(ID ou nome)")]
    public static void CMD_ferimentos(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
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

    [Command(["aceitarhospital"], "Geral", "Aceita o tratamento médico após estar ferido e é levado ao hospital")]
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

    [Command(["aceitarck"], "Geral", "Aceita o CK no personagem")]
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

            var bodyItems = new List<BodyItem>();
            var itemsToRemove = new List<CharacterItem>();

            if (player.DropItemsOnDeath)
            {
                var droppableCategories = new List<ItemCategory>
                {
                    ItemCategory.Weapon,
                    ItemCategory.Drug,
                    ItemCategory.WeaponComponent,
                };

                itemsToRemove = [.. player.Items.Where(x => droppableCategories.Contains(x.GetCategory())
                    || GlobalFunctions.CheckIfIsAmmo(x.GetCategory()))];
                foreach (var characterItem in itemsToRemove)
                {
                    var bodyItem = new BodyItem();
                    bodyItem.Create(characterItem.ItemTemplateId, characterItem.Subtype, characterItem.Quantity, characterItem.Extra);
                    bodyItem.SetSlot(characterItem.Slot);
                    bodyItems.Add(bodyItem);
                }
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

            if (itemsToRemove.Count > 0)
            {
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
            }

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

    [Command(["mostraridentidade", "mostrarid"], "Geral", "Mostra a identidade para um personagem", "(ID ou nome)")]
    public static void CMD_mostraridentidade(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        target.SendMessage(MessageType.Title, $"ID de {player.Character.Name}");
        target.SendMessage(MessageType.None, $"Sexo: {player.Character.Sex.GetDescription()}");
        target.SendMessage(MessageType.None, $"Idade: {player.Character.Age} anos");
        player.SendMessageToNearbyPlayers(player == target ? "olha sua própria ID." : $"mostra sua ID para {target.ICName}.", MessageCategory.Ame);
    }

    [Command(["mostrarlicenca", "ml"], "Geral", "Mostra a licença de motorista para um personagem", "(ID ou nome)")]
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
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
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

    [Command(["horas"], "Geral", "Exibe o horário")]
    public static void CMD_horas(MyPlayer player)
    {
        var horas = Convert.ToInt32(Math.Truncate(player.Character.ConnectedTime / 60M));
        var tempo = 60 - ((horas + 1) * 60 - player.Character.ConnectedTime);
        player.SendMessage(MessageType.None, $"{Constants.SERVER_INITIALS} | {DateTime.Now} | {{{Constants.MAIN_COLOR}}}{tempo} {{#FFFFFF}}de {{{Constants.MAIN_COLOR}}}60 {{#FFFFFF}}minutos para o próximo pagamento.");

        if (player.Character.JailFinalDate.HasValue)
            player.SendMessage(MessageType.None, $"Você está preso até {player.Character.JailFinalDate}.");
    }

    [Command(["limparchat"], "Geral", "Limpa o seu chat")]
    public static void CMD_limparchat(MyPlayer player) => player.ClearChat();

    [Command(["save"], "Geral", "Exibe sua posição e rotação ou do seu veículo no console")]
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

    [Command(["dados"], "Geral", "Joga um dado cujo o resultado será um número aleatório", "(2-20)")]
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

    [Command(["moeda"], "Geral", "Joga uma moeda cujo o resultado será cara ou coroa")]
    public static void CMD_moeda(MyPlayer player)
    {
        var number = new List<int> { 1, 2 }.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        var message = $@"[MOEDA] {player.ICName} joga a moeda e esta fica com a {(number == 1 ? "cara" : "coroa")} voltada para cima.";
        player.SendMessageToNearbyPlayers(message, MessageCategory.DiceCoin);
    }

    [Command(["levantar"], "Geral", "Levanta um jogador gravemente ferido somente de socos", "(ID ou nome)")]
    public static void CMD_levantar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        if (target.Character.Wound != CharacterWound.SeriouslyInjured || target.Wounds.Any(x => x.Weapon != GlobalFunctions.GetWeaponName((uint)WeaponModel.Fist)))
        {
            player.SendMessage(MessageType.Error, "Jogador não está gravemente ferido ou ferido somente com socos.");
            return;
        }

        target.Heal(true);
        player.SendMessageToNearbyPlayers($"ajuda {target.ICName} a se levantar.", MessageCategory.NormalMe);
    }

    [Command(["trocarpersonagem"], "Geral", "Desconecta do personagem atual e abre a seleção de personagens")]
    public async Task CMD_trocarpersonagem(MyPlayer player) => await player.ListCharacters("Troca de Personagem", string.Empty);

    [Command(["admins"], "Geral", "Exibe os administradores do servidor")]
    public static void CMD_admins(MyPlayer player)
    {
        player.SendMessage(MessageType.Title, $"STAFF: {Constants.SERVER_NAME}");

        var admins = Global.SpawnedPlayers.Where(x => x.User.Staff >= UserStaff.GameAdmin);
        foreach (var admin in admins
            .Where(x => x.OnAdminDuty || player.User.Staff >= UserStaff.GameAdmin)
            .OrderByDescending(x => x.OnAdminDuty)
            .ThenByDescending(x => x.User.Staff)
            .ThenBy(x => x.User.Name))
            player.SendMessage(MessageType.None, $"{admin.User.Staff.GetDescription()} {admin.User.Name}{(player.User.Staff >= UserStaff.GameAdmin ? $" ({admin.SessionId})" : string.Empty)}", admin.OnAdminDuty ? admin.StaffColor : "#B0B0B0");

        var adminsOffDuty = admins.Count(x => !x.OnAdminDuty);
        player.SendMessage(MessageType.None, $"{adminsOffDuty} administrador{(adminsOffDuty == 1 ? " está" : "es estão")} online em roleplay. Se precisar de ajuda de um administrador, utilize o /reportar.");
    }

    [Command(["historicocriminal"], "Geral", "Visualiza o histórico criminal do seu personagem")]
    public async Task CMD_historicocriminal(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        var faction = Global.Factions.FirstOrDefault(x => x.Id == property?.FactionId);
        if (faction?.Type != FactionType.Police)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma propriedade policial.");
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

    [Command(["cancelarconvite", "cc"], "Geral", "Cancela um convite", "(ID ou nome) (tipo)")]
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
            player.SendMessage(MessageType.Error, $"Você não enviou um convite do tipo {type.GetDescription()} para {target.ICName}.");
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

    [Command(["boombox"], "Geral", "Altera as configurações de uma boombox")]
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
            player.User.GetCurrentPremium() != UserPremium.None, Functions.Serialize(Global.AudioRadioStations.OrderBy(x => x.Name)));
    }

    [Command(["stopanim", "sa"], "Geral", "Para as animações")]
    public static void CMD_stopanim(MyPlayer player) => player.CheckAnimations(true);

    [Command(["animacoes"], "Geral", "Abre o painel de animações")]
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

    [Command(["dl"], "Geral", "Ativa/desativa a exibição de informações veículos na tela")]
    public static void CMD_dl(MyPlayer player)
    {
        player.User.SetVehicleTagToggle(!player.User.VehicleTagToggle);
        player.Emit("dl:Config", player.User.VehicleTagToggle);
        player.SendNotification(NotificationType.Success, $"Você {(!player.User.VehicleTagToggle ? "des" : string.Empty)}ativou o DL.");
    }

    [Command(["tela"], "Geral", "Exibe um fundo com a cor de fundo configurada na tela")]
    public static void CMD_tela(MyPlayer player)
    {
        player.ToggleChatBackgroundColor = !player.ToggleChatBackgroundColor;
        player.Emit(Constants.CHAT_PAGE_TOGGLE_SCREEN, player.ToggleChatBackgroundColor ? $"#{player.User.ChatBackgroundColor}" : "transparent");
    }

    [Command(["timestamp"], "Geral", "Ativa/desativa o timestamp")]
    public static void CMD_timestamp(MyPlayer player)
    {
        player.User.SetTimeStampToggle(!player.User.TimeStampToggle);
        player.ConfigChat();
        player.SendNotification(NotificationType.Success, $"Você {(!player.User.TimeStampToggle ? "des" : string.Empty)}ativou o timestamp.");
    }

    [Command(["modofoto"], "Geral", "Ativa/desativa o modo foto")]
    public static void CMD_modofoto(MyPlayer player)
    {
        player.PhotoMode = !player.PhotoMode;
        player.StartNoClip(true);
        player.SetNametag();
        player.SendNotification(NotificationType.Success, $"Você {(!player.PhotoMode ? "des" : string.Empty)}ativou o modo foto.");
    }

    [Command(["anuncios"], "Geral", "Exibe os anúncios em andamento")]
    public static void CMD_anuncios(MyPlayer player)
    {
        player.Emit("Announcements:Show", Functions.Serialize(Global.Announcements.OrderByDescending(x => x.Date)));
    }

    [Command(["carregar"], "Geral", "Carrega um jogador", "(ID ou nome)")]
    public static async Task CMD_carregar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
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

    [Command(["soltar"], "Geral", "Para de carregar um jogador")]
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

    [Command(["maquiagem"], "Geral", "Edita sua maquiagem")]
    public static void CMD_maquiagem(MyPlayer player)
    {
        player.Edit(4);
    }

    [Command(["fontsize"], "Geral", "Altera o tamanho da fonte do chat", "(tamanho)")]
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

    [Command(["pagesize"], "Geral", "Altera a quantidade de linhas do chat", "(tamanho)")]
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

    [Command(["alterarnumero"], "Geral", "Altera o número do seu celular equipado", "(número)")]
    public async Task CMD_alterarnumero(MyPlayer player, uint number)
    {
        if (number.ToString().Length < 4 || number.ToString().Length > 7)
        {
            player.SendMessage(MessageType.Error, "Número deve ter entre 4 e 7 caracteres.");
            return;
        }

        if (player.User.NumberChanges == 0)
        {
            player.SendMessage(MessageType.Error, "Você não possui uma mudança de número.");
            return;
        }

        var cellphoneItem = player.Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.Cellphone && x.InUse);
        if (cellphoneItem is null)
        {
            player.SendMessage(MessageType.Error, "Você não está com um celular equipado.");
            return;
        }

        if (await Functions.CheckIfCellphoneExists(number))
        {
            player.SendMessage(MessageType.Error, "Número já está sendo utilizado.");
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

    [Command(["caminhada"], "Geral", "Altera o estilo de caminhada", "(tipo)")]
    public static void CMD_caminhada(MyPlayer player, byte style)
    {
        if (player.User.GetCurrentPremium() == UserPremium.None)
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

    [Command(["booster"], "Geral", "Obtem recompensa por boostar o Discord principal do servidor")]
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

    [Command(["stats"], "Geral", "Exibe as informações do seu personagem")]
    public async Task CMD_stats(MyPlayer player)
    {
        var context = Functions.GetDatabaseContext();

        var paycheck = await player.Paycheck(true);

        var vehicles = await context.Vehicles.Where(x => x.CharacterId == player.Character.Id && !x.Sold).ToListAsync();
        var currentPremium = player.User.GetCurrentPremium();

        var response = new CharacterInfoResponse
        {
            FactionId = player.Character.FactionId,
            Staff = player.User.Staff,
            Premium = $"{currentPremium.GetDescription()}{(currentPremium != UserPremium.None ? $" (até {player.User.PremiumValidDate})" : string.Empty)}",
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
                Type = x.Type.GetDescription(),
                WaitingTime = Functions.GetTimespan(x.Date),
            }),
            Job = player.Character.Job.GetDescription(),
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
            StaffName = player.User.Staff.GetDescription(),
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

    [Command(["ajuda"], "Geral", "Exibe os comandos do servidor")]
    public static void CMD_ajuda(MyPlayer player)
    {
        player.Emit("Help:Open", Global.CommandsHelpJson);
    }

    [Command(["config"], "Geral", "Exibe as suas configurações")]
    public static void CMD_config(MyPlayer player)
    {
        var settings = new UCPSettingsRequest(player.User.TimeStampToggle, player.User.AnnouncementToggle,
            player.User.PMToggle, player.User.FactionChatToggle, player.StaffChatToggle, player.User.ChatFontType,
            player.User.ChatLines, player.User.ChatFontSize, player.User.FactionToggle, player.User.VehicleTagToggle,
            player.User.ChatBackgroundColor, player.User.ShowNametagId, player.User.AmbientSoundToggle,
            player.User.FreezingTimePropertyEntrance, player.User.ShowOwnNametag, player.StaffToggle,
            player.FactionWalkieTalkieToggle, player.User.ReceiveSMSDiscord, player.User.ReceiveNotificationsOnDiscord);
        player.Emit("Settings:Open", Functions.Serialize(settings), Functions.Serialize(new
        {
            IsPremium = player.User.GetCurrentPremium() != UserPremium.None,
            IsStaff = player.User.Staff >= UserStaff.GameAdmin,
            HasFaction = player.Character.FactionId.HasValue,
        }), Functions.Serialize(
                Enum.GetValues<UserReceiveSMSDiscord>()
                .Select(x => new
                {
                    Value = x,
                    Label = x.GetDescription(),
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
            player.User.GetCurrentPremium() != UserPremium.None && settings.PMToggle,
            settings.FactionChatToggle, settings.ChatFontType, settings.ChatLines, settings.ChatFontSize,
            settings.FactionToggle, settings.VehicleTagToggle, settings.ChatBackgroundColor, settings.ShowNametagId, settings.AmbientSoundToggle,
            settings.FreezingTimePropertyEntrance, settings.ShowOwnNametag, settings.ReceiveSMSDiscord, settings.ReceiveNotificationsOnDiscord);
        player.StaffChatToggle = settings.StaffChatToggle;
        player.StaffToggle = settings.StaffToggle;
        player.FactionWalkieTalkieToggle = settings.FactionWalkieTalkieToggle;
        player.ConfigChat();
        player.NametagsConfig();
        player.DlConfig();
        player.ToggleAmbientSound();
        player.SendNotification(NotificationType.Success, "Configurações gravadas com sucesso.");
    }

    [Command(["doar"], "Geral", "Remove a quantidade de dinheiro especificada", "(quantidade)")]
    public async Task CMD_doar(MyPlayer player, int quantity)
    {
        if (quantity <= 0)
        {
            player.SendMessage(MessageType.Error, "Valor inválido.");
            return;
        }

        if (player.Money < quantity)
        {
            player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, quantity));
            return;
        }

        await player.RemoveMoney(quantity);
        await player.WriteLog(LogType.Donate, $"${quantity:N0}", null);
        player.SendMessage(MessageType.Success, $"Você doou ${quantity:N0}.");
    }

    [Command(["modoanim"], "Geral", "Edita sua posição/rotação na animação")]
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

    [Command(["mascara"], "Geral", "Coloca/remove a máscara")]
    public async Task CMD_mascara(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
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

    [Command(["corrigirvw"], "Geral", "Corrige seu VW")]
    public async Task CMD_corrigirvw(MyPlayer player)
    {
        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.PropertyNoClip)
        {
            await Functions.SendServerMessage($"{player.Character.Name} ({player.User.Name}) tentou usar o comando /corrigirvw enquanto estava com o no-clip ativado.", UserStaff.GameAdmin, true);
            player.SendMessage(MessageType.Error, "Você não pode corrigir seu VW enquanto está com o no-clip ativado.");
            return;
        }

        player.SetPosition(player.GetPosition(), 0, false);
        await player.WriteLog(LogType.Fix, "/corrigirvw", null);
        player.SendMessage(MessageType.Success, "Você corrigiu seu VW.");
    }

    [Command(["atributos"], "Geral", "Define os seus atributos")]
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

    [Command(["examinar"], "Geral", "Visualiza os atributos de um personagem próximo", "(ID ou nome)")]
    public static void CMD_examinar(MyPlayer player, string idOrName)
    {
        var target = player.GetCharacterByIdOrName(idOrName);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
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

    [Command(["testers"], "Geral", "Exibe os testers do servidor")]
    public static void CMD_testers(MyPlayer player)
    {
        var supports = Global.SpawnedPlayers.Where(x => x.User.Staff == UserStaff.Tester).OrderBy(x => x.User.Name);
        if (!supports.Any())
        {
            player.SendMessage(MessageType.Error, "Não há nenhum tester online.");
            return;
        }

        player.SendMessage(MessageType.Title, $"TESTERS: {Constants.SERVER_NAME}");
        foreach (var support in supports)
            player.SendMessage(MessageType.None, $"{support.User.Name}{(player.User.Staff >= UserStaff.Tester ? $" ({support.SessionId})" : string.Empty)}");
        player.SendMessage(MessageType.None, "Se precisar de ajuda de um Tester, utilize o /sos.");
    }

    [Command(["pegarneve"], "Geral", "Pega uma bola de neve do chão")]
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

    [Command(["cabelo"], "Geral", "Ativa/desativa o cabelo")]
    public async Task CMD_cabelo(MyPlayer player)
    {
        if (!player.ValidPed)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAValidSkin);
            return;
        }

        player.UsingHair = !player.UsingHair;
        player.SetPersonalization();
        player.SendMessage(MessageType.Success, $"Você {(player.UsingHair ? string.Empty : "des")}ativou seu cabelo.");
        await player.WriteLog(LogType.General, "/cabelo", null);
    }

    [Command(["fixinvi"], "Geral", "Corrige a invisibilidade do seu personagem")]
    public async Task CMD_fixinvi(MyPlayer player)
    {
        player.Visible = true;
        player.SendMessage(MessageType.Success, "Você corrigiu sua visibilidade.");
        await player.WriteLog(LogType.Fix, "/fixinvi", null);
        player.SendMessage(MessageType.Error, "ATENÇÃO! Abusar desse comando é inadmissível.");
    }

    [Command(["notificacoes"], "Geral", "Visualiza suas notificações não lidas")]
    public async Task CMD_notificacoes(MyPlayer player)
    {
        var context = Functions.GetDatabaseContext();
        var unreadNotifications = await context.Notifications
            .Where(x => x.UserId == player.User.Id && !x.ReadDate.HasValue)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync();
        if (unreadNotifications.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Você não possui notificações não lidas.");
            return;
        }

        foreach (var unreadNotification in unreadNotifications)
            player.SendMessage(MessageType.None, $"{unreadNotification.Message} ({unreadNotification.RegisterDate})");

        player.SendMessage(MessageType.Title, "Use /lernotificacoes para marcar todas as notificações como lidas.");
    }

    [Command(["lernotificacoes"], "Geral", "Marca todas suas notificações como lidas")]
    public async Task CMD_lernotificacoes(MyPlayer player)
    {
        var context = Functions.GetDatabaseContext();
        var unreadNotifications = await context.Notifications
            .Where(x => x.UserId == player.User.Id && !x.ReadDate.HasValue)
            .ToListAsync();
        if (unreadNotifications.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Você não possui notificações não lidas.");
            return;
        }

        foreach (var unreadNotification in unreadNotifications)
        {
            unreadNotification.MarkAsRead();
            context.Notifications.Update(unreadNotification);
        }
        await context.SaveChangesAsync();
        player.SendMessage(MessageType.Success, "Todas as notificações foram marcadas como lidas.");
    }
}