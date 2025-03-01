using GTANetworkAPI;
using TrevizaniRoleplay.Core.Models.Server;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PropertyScript : Script
{
    [Command("pvender", "/pvender (ID ou nome) (valor)")]
    public static void CMD_pvender(MyPlayer player, string idOrName, int valor)
    {
        var property = Global.Properties
            .FirstOrDefault(x => x.CharacterId == player.Character.Id && x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnYourOwnProperty);
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        if (valor <= 0)
        {
            player.SendMessage(MessageType.Error, "Valor não é válido.");
            return;
        }

        var convite = new Invite()
        {
            Type = InviteType.PropertySell,
            SenderCharacterId = player.Character.Id,
            Value = [property.Id.ToString(), valor.ToString()],
        };
        target.Invites.RemoveAll(x => x.Type == InviteType.PropertySell);
        target.Invites.Add(convite);

        player.SendMessage(MessageType.Success, $"Você ofereceu {property.FormatedAddress} para {target.ICName} por ${valor:N0}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} ofereceu para você {property.FormatedAddress} por ${valor:N0}. (/ac {(int)convite.Type} para aceitar ou /rc {(int)convite.Type} para recusar)");
    }

    [Command("pvendergoverno")]
    public async Task CMD_pvendergoverno(MyPlayer player) => await CMDVenderPropriedadeGoverno(player, false);

    [Command("armazenamento")]
    public static void CMD_armazenamento(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        if (!property.CanAccess(player)
            && !(player.Faction?.Type == FactionType.Police && player.OnDuty))
        {
            player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        property.ShowInventory(player, false);
    }

    [Command("pfechadura")]
    public async Task CMD_pfechadura(MyPlayer player)
    {
        var property = Global.Properties
            .FirstOrDefault(x => x.CharacterId == player.Character.Id && x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnYourOwnProperty);
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        if (player.Money < Global.Parameter.LockValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.LockValue));
            return;
        }

        property.SetLockNumber(Global.Properties.Select(x => x.LockNumber).DefaultIfEmpty(0u).Max() + 1);
        var context = Functions.GetDatabaseContext();
        context.Properties.Update(property);
        await context.SaveChangesAsync();

        await player.RemoveMoney(Global.Parameter.LockValue);

        player.SendMessage(MessageType.Success, $"Você trocou a fechadura de {property.FormatedAddress} por ${Global.Parameter.LockValue:N0}.");
    }

    [Command("pchave")]
    public async Task CMD_pchave(MyPlayer player)
    {
        var property = Global.Properties
            .FirstOrDefault(x => x.CharacterId == player.Character.Id && x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnYourOwnProperty);
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        if (player.Money < Global.Parameter.KeyValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.KeyValue));
            return;
        }

        var characterItem = new CharacterItem();
        characterItem.Create(new Guid(Constants.PROPERTY_KEY_ITEM_TEMPLATE_ID), property.LockNumber, 1, null);
        var res = await player.GiveItem(characterItem);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendMessage(MessageType.Error, res);
            return;
        }

        await player.RemoveMoney(Global.Parameter.KeyValue);

        player.SendMessage(MessageType.Success, $"Você criou uma cópia da chave de {property.FormatedAddress} por ${Global.Parameter.KeyValue:N0}.");
    }

    [Command("pupgrade")]
    public static void CMD_pupgrade(MyPlayer player)
    {
        var property = Global.Properties
            .FirstOrDefault(x => x.CharacterId == player.Character.Id && x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotOnYourOwnProperty);
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendNotification(NotificationType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        var itemsJSON = Functions.Serialize(
            new List<dynamic>
            {
                new
                {
                    Name = Resources.ProtectionLevel1,
                    Price = Math.Truncate(property.Value * (Global.Parameter.PropertyProtectionLevelPercentageValue / 100)),
                },
                new
                {
                    Name = Resources.ProtectionLevel2,
                    Price = Math.Truncate(property.Value * (Global.Parameter.PropertyProtectionLevelPercentageValue / 100) * 2),
                },
                new
                {
                    Name = Resources.ProtectionLevel3,
                    Price = Math.Truncate(property.Value * (Global.Parameter.PropertyProtectionLevelPercentageValue / 100) * 3),
                },
            }
        );

        player.Emit("PropertyUpgrade", $"Upgrades • {property.FormatedAddress}", property.Id.ToString(), itemsJSON);
    }

    [Command("arrombar")]
    public async Task CMD_arrombar(MyPlayer player)
    {
        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você deve estar fora do veículo.");
            return;
        }

        var property = Global.Properties.Where(x =>
            player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= Constants.RP_DISTANCE
            && x.EntranceDimension == player.GetDimension()
            && x.Locked
            && x.CharacterId.HasValue)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhuma propriedade trancada que possua dono.");
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        if (player.OnDuty && player.Faction?.Type == FactionType.Police)
        {
            property.SetLocked(false);
            var context = Functions.GetDatabaseContext();
            context.Properties.Update(property);
            await context.SaveChangesAsync();
            player.SendMessageToNearbyPlayers("arromba a porta.", MessageCategory.Ame);
            await player.WriteLog(LogType.Breakin, $"{property.FormatedAddress} ({property.Number}) POLICE", null);
            return;
        }

        if (player.Character.ConnectedTime < Global.Parameter.PropertyRobberyConnectedTime)
        {
            player.SendMessage(MessageType.Error, $"É necessário que você tenha no mínimo {Global.Parameter.PropertyRobberyConnectedTime} minutos de tempo jogado em seu personagem para realizar essa ação.");
            return;
        }

        if (Global.SpawnedPlayers.Count(x => x.Faction?.Type == FactionType.Police && x.OnDuty) < Global.Parameter.PoliceOfficersPropertyRobbery)
        {
            player.SendMessage(MessageType.Error, $"É necessário {Global.Parameter.PoliceOfficersPropertyRobbery} policiais em serviço.");
            return;
        }

        var pins = property.ProtectionLevel switch
        {
            1 => 4,
            2 => 6,
            3 => 9,
            _ => 3,
        };

        var difficulty = 2;
        var attempts = 1;

        player.Emit("PickLock:Start", difficulty, pins, attempts, "Property");
        await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) começou a arrombar a propriedade {property.FormatedAddress}.", UserStaff.JuniorServerAdmin, false);
    }

    [Command("roubarpropriedade")]
    public async Task CMD_roubarpropriedade(MyPlayer player)
    {
        if ((player.User.PropertyRobberyCooldown ?? DateTime.MinValue) > DateTime.Now)
        {
            player.SendMessage(MessageType.Error, $"Aguarde o cooldown para roubar novamente. Será liberado em {player.User.PropertyRobberyCooldown}.");
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension()
            && !x.Locked
            && x.CharacterId.HasValue);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade destrancada que possua dono.");
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        if ((property.RobberyCooldown ?? DateTime.MinValue) > DateTime.Now)
        {
            player.SendMessage(MessageType.Error, $"Aguarde o cooldown da propriedade para roubar novamente. Será liberado em {property.RobberyCooldown}.");
            return;
        }

        if (Global.SpawnedPlayers.Count(x => x.Faction?.Type == FactionType.Police && x.OnDuty) < Global.Parameter.PoliceOfficersPropertyRobbery)
        {
            player.SendMessage(MessageType.Error, $"É necessário {Global.Parameter.PoliceOfficersPropertyRobbery} policiais em serviço.");
            return;
        }

        if (Global.SpawnedPlayers.Any(x => x.RobberingPropertyId == property.Id))
        {
            player.SendMessage(MessageType.Error, "Esta propriedade já está sendo roubada.");
            return;
        }

        await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) começou a roubar a propriedade {property.FormatedAddress}.", UserStaff.JuniorServerAdmin, false);
        await property.ActivateProtection();

        var waitSeconds = property.ProtectionLevel switch
        {
            1 => 200,
            2 => 300,
            3 => 600,
            _ => 150,
        };

        player.RobberingPropertyId = property.Id;
        player.ToggleGameControls(false);
        player.SendMessage(MessageType.Success, $"Aguarde {waitSeconds} segundos. Pressione DELETE para cancelar a ação.");
        player.CancellationTokenSourceAcao?.Cancel();
        player.CancellationTokenSourceAcao = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(waitSeconds), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            Task.Run(async () =>
            {
                var value = Convert.ToInt32(Math.Truncate(property.Value * 0.1));
                var res = await player.GiveMoney(value);
                player.RobberingPropertyId = null;
                if (!string.IsNullOrWhiteSpace(res))
                {
                    player.SendMessage(MessageType.Error, res);
                    return;
                }

                property.SetRobberyValue(value);
                property.SetRobberyCooldown(DateTime.Now.AddHours(Global.Parameter.CooldownPropertyRobberyPropertyHours));
                var context = Functions.GetDatabaseContext();
                context.Properties.Update(property);
                await context.SaveChangesAsync();
                player.User.SetPropertyRobberyCooldown(DateTime.Now.AddHours(Global.Parameter.CooldownPropertyRobberyRobberHours));

                player.ToggleGameControls(true);
                player.SendMessageToNearbyPlayers("rouba a propriedade.", MessageCategory.Ame);
                player.SendMessage(MessageType.Success, $"Você roubou a propriedade e recebeu ${value:N0}.");
                await player.WriteLog(LogType.StealProperty, $"{property.FormatedAddress} ({property.Number}) {value}", null);
                player.CancellationTokenSourceAcao = null;
            });
        });
    }

    [Command("pliberar")]
    public async Task CMD_pliberar(MyPlayer player)
    {
        var property = Global.Properties
            .Where(x => x.CharacterId == player.Character.Id
                && x.RobberyValue > 0
                && player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de uma propriedade sua que foi roubada.");
            return;
        }

        if (player.Money < property.RobberyValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, property.RobberyValue));
            return;
        }

        await player.RemoveMoney(property.RobberyValue);

        var context = Functions.GetDatabaseContext();
        player.SendMessage(MessageType.Success, $"Você liberou {property.FormatedAddress} por ${property.RobberyValue:N0}.");
        property.ResetRobberyValue();
        context.Properties.Update(property);
        await context.SaveChangesAsync();
        await player.WriteLog(LogType.Money, $"/pliberar {property.FormatedAddress} ({property.Number}) {property.RobberyValue}", null);
    }

    [RemoteEvent(nameof(VenderPropriedade))]
    public async Task VenderPropriedade(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await CMDVenderPropriedadeGoverno(player, true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task CMDVenderPropriedadeGoverno(MyPlayer player, bool confirm)
    {
        var property = Global.Properties
            .FirstOrDefault(x => x.CharacterId == player.Character.Id && x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotOnYourOwnProperty);
            return;
        }

        if (property.RobberyValue > 0)
        {
            player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
            return;
        }

        var value = Convert.ToInt32(property.Value / 2) - property.RobberyValue;

        if (confirm)
        {
            var res = await player.GiveMoney(value);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendMessage(MessageType.Error, res);
                return;
            }

            await property.ChangeOwner(null);

            player.SendMessage(MessageType.Success, $"Você vendeu {property.FormatedAddress} para o governo por ${value:N0}.");
            await player.WriteLog(LogType.Sell, $"/pvendergoverno {property.FormatedAddress} {value}", null);
        }
        else
        {
            player.ShowConfirm("Confirmar Venda", $"Confirma vender {property.FormatedAddress} para o governo por ${value:N0}?", "VenderPropriedade");
        }
    }

    [RemoteEvent(nameof(BuyPropertyUpgrade))]
    public async Task BuyPropertyUpgrade(Player playerParam, string idPropertyString, string name)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == idPropertyString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var value = property.Value;

            if (name == Resources.ProtectionLevel1)
                value = Convert.ToInt32(Math.Truncate(value * (Global.Parameter.PropertyProtectionLevelPercentageValue / 100)));
            else if (name == Resources.ProtectionLevel2)
                value = Convert.ToInt32(Math.Truncate(value * (Global.Parameter.PropertyProtectionLevelPercentageValue / 100) * 2));
            else if (name == Resources.ProtectionLevel3)
                value = Convert.ToInt32(Math.Truncate(value * (Global.Parameter.PropertyProtectionLevelPercentageValue / 100) * 3));

            if (player.Money < value)
            {
                player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, value));
                return;
            }

            if (name == Resources.ProtectionLevel1)
            {
                if (property.ProtectionLevel >= 1)
                {
                    player.SendNotification(NotificationType.Error, $"A propriedade já possui um nível de proteção igual ou maior que 1.");
                    return;
                }

                property.SetProtectionLevel(1);
            }
            else if (name == Resources.ProtectionLevel2)
            {
                if (property.ProtectionLevel >= 2)
                {
                    player.SendNotification(NotificationType.Error, $"A propriedade já possui um nível de proteção igual ou maior que 2.");
                    return;
                }

                property.SetProtectionLevel(2);
            }
            else if (name == Resources.ProtectionLevel3)
            {
                if (property.ProtectionLevel >= 3)
                {
                    player.SendNotification(NotificationType.Error, $"A propriedade já possui um nível de proteção igual ou maior que 3.");
                    return;
                }

                property.SetProtectionLevel(3);
            }

            var context = Functions.GetDatabaseContext();
            context.Properties.Update(property);
            await context.SaveChangesAsync();

            await player.RemoveMoney(value);
            await player.WriteLog(LogType.Money, $"{name} ${value:N0}", null);
            player.SendNotification(NotificationType.Success, $"Você comprou {name} por ${value:N0}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BuildingPropertyEnter))]
    public static void BuildingPropertyEnter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (property.Locked)
            {
                player.SendNotification(NotificationType.Error, "A porta está trancada.");
                return;
            }

            if (property.RobberyValue > 0)
            {
                player.SendNotification(NotificationType.Error, Resources.PropertyHasBeenStolen);
                return;
            }

            player.SetPosition(property.GetExitPosition(), property.Number, false);
            player.SetRotation(property.GetExitRotation());
            player.Emit(Constants.PROPERTY_BUILDING_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BuildingPropertyLockUnlock))]
    public async Task BuildingPropertyLockUnlock(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (!property.CanAccess(player))
            {
                player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            property.StopAlarm();
            property.SetLocked(!property.Locked);
            player.SendMessageToNearbyPlayers($"{(!property.Locked ? "des" : string.Empty)}tranca a porta.", MessageCategory.Ame);

            var context = Functions.GetDatabaseContext();
            context.Properties.Update(property);
            await context.SaveChangesAsync();
            player.Emit(Constants.PROPERTY_BUILDING_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BuildingPropertyRelease))]
    public async Task BuildingPropertyRelease(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (property.CharacterId != player.Character.Id)
            {
                player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            if (property.RobberyValue == 0)
            {
                player.SendNotification(NotificationType.Error, "Propriedade não foi roubada.");
                return;
            }

            if (player.Money < property.RobberyValue)
            {
                player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, property.RobberyValue));
                return;
            }

            await player.RemoveMoney(property.RobberyValue);

            player.SendMessage(MessageType.Success, $"Você liberou {property.FormatedAddress} por ${property.RobberyValue:N0}.");
            property.ResetRobberyValue();
            var context = Functions.GetDatabaseContext();
            context.Properties.Update(property);
            await context.SaveChangesAsync();
            await player.WriteLog(LogType.Money, $"/pliberar {property.FormatedAddress} ({property.Number}) {property.RobberyValue}", null);

            player.Emit(Constants.PROPERTY_BUILDING_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BuildingPropertyBreakIn))]
    public async Task BuildingPropertyBreakIn(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsInVehicle)
            {
                player.SendNotification(NotificationType.Error, "Você deve estar fora do veículo.");
                return;
            }

            if (player.CurrentWeapon != WeaponHash.Crowbar)
            {
                player.SendNotification(NotificationType.Error, "Você não está com um pé de cabra em mãos.");
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (!property.Locked || !property.CharacterId.HasValue)
            {
                player.SendMessage(MessageType.Error, "Propriedade não está trancada ou não possui dono.");
                return;
            }

            if (property.RobberyValue > 0)
            {
                player.SendMessage(MessageType.Error, Resources.PropertyHasBeenStolen);
                return;
            }

            await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) começou a arrombar a propriedade {property.FormatedAddress}.", UserStaff.JuniorServerAdmin, false);
            await property.ActivateProtection();

            player.ToggleGameControls(false);
            player.SendMessage(MessageType.Success, $"Aguarde 200 segundos. Pressione DELETE para cancelar a ação.");
            player.CancellationTokenSourceAcao?.Cancel();
            player.CancellationTokenSourceAcao = new();
            await Task.Delay(TimeSpan.FromSeconds(200), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                Task.Run(async () =>
                {
                    property.SetLocked(false);
                    var context = Functions.GetDatabaseContext();
                    context.Properties.Update(property);
                    await context.SaveChangesAsync();

                    player.ToggleGameControls(true);
                    player.SendMessageToNearbyPlayers("arromba a porta.", MessageCategory.Ame);
                    await player.WriteLog(LogType.Breakin, $"{property.FormatedAddress} ({property.Number})", null);
                    player.CancellationTokenSourceAcao = null;
                });
            });
            player.Emit(Constants.PROPERTY_BUILDING_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BuildingPropertyBuy))]
    public async Task BuildingPropertyBuy(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (property.Value < 0 || property.CharacterId.HasValue)
            {
                player.SendNotification(NotificationType.Error, "Propriedade não está a venda.");
                return;
            }

            if (player.Money < property.Value)
            {
                player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, property.Value));
                return;
            }

            await player.RemoveMoney(property.Value);
            await property.ChangeOwner(player.Character.Id);

            player.SendNotification(NotificationType.Success, $"Você comprou {property.FormatedAddress} por ${property.Value:N0}.");
            player.Emit(Constants.PROPERTY_BUILDING_PAGE_CLOSE);
            await player.WriteLog(LogType.Buy, $"Comprar Propriedade {property.FormatedAddress} ({property.Id}) por {property.Value}", null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("pboombox")]
    public static void CMD_pboombox(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        if (!property.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
            return;
        }

        var item = Global.Objects.Where(x => x.GetDimension() == player.GetDimension()
            && player.GetPosition().DistanceTo(x.Position) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(x.Position));

        var audioOutput = Global.Furnitures.FirstOrDefault(x => x.Model.ToLower() ==
            property.Furnitures!.FirstOrDefault(y => y.Id == item?.PropertyFurnitureId)?.Model?.ToLower())?.AudioOutput == true;
        if (!audioOutput)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de uma boombox.");
            return;
        }

        var audioSpot = property.GetAudioSpot();
        player.Emit("PropertyBoombox", item!.Id.ToString(), audioSpot?.Source ?? string.Empty, audioSpot?.Volume ?? 1,
            player.GetCurrentPremium() != UserPremium.None, Functions.Serialize(Global.AudioRadioStations.OrderBy(x => x.Name)));
    }

    [RemoteEvent(nameof(ConfirmPropertyBoombox))]
    public static void ConfirmPropertyBoombox(Player playerParam, string objectId, string url, float volume)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                && uriResult?.Scheme != Uri.UriSchemeHttp && uriResult?.Scheme != Uri.UriSchemeHttps)
            {
                player.SendNotification(NotificationType.Error, "URL está em um formato inválido.");
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
            if (property is null)
            {
                player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
                return;
            }

            if (!property.CanAccess(player))
            {
                player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            var item = Global.Objects.FirstOrDefault(x => x.GetDimension() == player.GetDimension()
                && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
                && x.Id.ToString() == objectId);
            if (item is null)
            {
                player.SendMessage(MessageType.Error, "Você não está próximo de uma boombox.");
                return;
            }

            var audioSpot = property.GetAudioSpot();
            audioSpot ??= new AudioSpot
            {
                Position = item.Position,
                Dimension = player.GetDimension(),
                PropertyId = property.Id,
                Range = 75,
            };

            audioSpot.Source = url;
            audioSpot.Volume = volume;

            audioSpot.SetupAllClients();

            player.SendMessageToNearbyPlayers("configura a boombox.", MessageCategory.Ame);
            player.Emit(Constants.BOOMBOX_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(TurnOffPropertyBoombox))]
    public static void TurnOffPropertyBoombox(Player playerParam, string objectId)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
            if (property is null)
            {
                player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
                return;
            }

            if (!property.CanAccess(player))
            {
                player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            var item = Global.Objects.FirstOrDefault(x => x.GetDimension() == player.GetDimension()
                && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
                && x.Id.ToString() == objectId);
            if (item is null)
            {
                player.SendMessage(MessageType.Error, "Você não está próximo de uma boombox.");
                return;
            }

            var audioSpot = property.GetAudioSpot();
            if (audioSpot is not null)
            {
                audioSpot.RemoveAllClients();
                player.SendMessageToNearbyPlayers($"desliga a boombox.", MessageCategory.Ame);
            }

            player.Emit(Constants.BOOMBOX_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("tv")]
    public async Task CMD_tv(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        if (!property.CanAccess(player))
        {
            player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
            return;
        }

        var propertyFurniture = property.Furnitures!.Where(x => x.Interior
            && !string.IsNullOrWhiteSpace(x.GetFurniture()?.TVTexture))
            .MinBy(x => player.Position.DistanceTo(x.GetPosition()));
        if (propertyFurniture is null)
        {
            player.SendNotification(NotificationType.Error, "Não existe uma TV na propriedade.");
            return;
        }

        var item = Global.Objects.FirstOrDefault(x => x.PropertyFurnitureId == propertyFurniture.Id);
        if (item is null)
        {
            player.SendNotification(NotificationType.Error, "Não existe um objeto de TV na propriedade.");
            return;
        }

        if (Global.Objects.Any(x => !string.IsNullOrWhiteSpace(x.TVSource)
            && x.Dimension == player.Dimension
            && x.Id != item.Id))
        {
            player.SendNotification(NotificationType.Error, "Já existe uma TV ligada na propriedade.");
            return;
        }

        var ownerPremium = await property.GetOwnerPremium();
        if (ownerPremium.Item1 != UserPremium.Gold)
        {
            player.SendNotification(NotificationType.Error, "Dono da propriedade não é Premium Ouro.");
            return;
        }

        player.Emit("TV:Config", item.Id, item.TVSource, item.TVVolume);
    }

    [RemoteEvent(nameof(TVConfigSave))]
    public static void TVConfigSave(Player playerParam, uint id, string source, float volume)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, "Você não está no interior de uma propriedade.");
                return;
            }

            if (!property.CanAccess(player))
            {
                player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                return;
            }

            var myObject = Global.Objects.FirstOrDefault(x => x.Id == id);
            if (myObject is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var propertyFurniture = property.Furnitures!.FirstOrDefault(x => x.Id == myObject.PropertyFurnitureId);
            if (propertyFurniture is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var furniture = Global.Furnitures.FirstOrDefault(x => x.Model.ToLower() == propertyFurniture.Model.ToLower());
            if (furniture is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                myObject.TVSource = string.Empty;
                myObject.ResetSharedDataEx("TV");
                player.SendNotification(NotificationType.Success, "Você desligou a TV.");
                return;
            }

            myObject.TVSource = source;
            myObject.TVVolume = volume;

            myObject.SetSharedDataEx("TV", Functions.Serialize(new
            {
                Texture = furniture.TVTexture,
                Source = source,
                Volume = volume,
            }));
            player.SendNotification(NotificationType.Success, "Você configurou a TV.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("ptempo", "/ptempo (tempo)", Aliases = ["pweather"])]
    public async Task CMD_ptempo(MyPlayer player, uint weather)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        if (!property.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
            return;
        }

        if (!Enum.IsDefined(typeof(Weather), weather))
        {
            player.SendMessage(MessageType.Error, "Tempo inválido.");
            return;
        }

        var userPremium = await property.GetOwnerPremium();
        if (userPremium.Item1 == UserPremium.None)
        {
            player.SendMessage(MessageType.Error, "Dono da propriedade não é Premium.");
            return;
        }

        property.SetWeather(Convert.ToByte(weather));

        var context = Functions.GetDatabaseContext();
        context.Properties.Update(property);
        await context.SaveChangesAsync();

        var weatherType = (Weather)property.Weather!;
        foreach (var target in Global.SpawnedPlayers.Where(x => x.GetDimension() == player.GetDimension()))
            target.SyncWeather(weatherType);

        player.SendMessage(MessageType.Success, $"Você alterou o clima da propriedade para {weatherType}.");
    }

    [Command("phora", "/phora (hora)", Aliases = ["ptime"])]
    public async Task CMD_phora(MyPlayer player, byte hour)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        if (!property.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, "Você não possui acesso a esta propriedade.");
            return;
        }

        if (hour < 0 || hour > 23)
        {
            player.SendMessage(MessageType.Error, "Hora deve ser entre 0 e 23.");
            return;
        }

        var userPremium = await property.GetOwnerPremium();
        if (userPremium.Item1 == UserPremium.None)
        {
            player.SendMessage(MessageType.Error, "Dono da propriedade não é Premium.");
            return;
        }

        property.SetTime(hour);

        var context = Functions.GetDatabaseContext();
        context.Properties.Update(property);
        await context.SaveChangesAsync();

        foreach (var target in Global.SpawnedPlayers.Where(x => x.GetDimension() == player.GetDimension()))
            target.SetHour(hour);

        player.SendMessage(MessageType.Success, $"Você alterou a hora da propriedade para {hour}.");
    }

    [Command("fixloc")]
    public async Task CMD_fixloc(MyPlayer player)
    {
        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
        if (property is null)
        {
            player.SendMessage(MessageType.Error, "Você não está no interior de uma propriedade.");
            return;
        }

        if (player.Vehicle is MyVehicle vehicle)
        {
            vehicle.SetPosition(property.GetExitPosition());
            vehicle.Rotation = property.GetExitRotation();
        }
        else
        {
            player.SetPosition(property.GetExitPosition(), player.GetDimension(), false);
        }

        await player.WriteLog(LogType.Fix, "/fixloc", null);
        player.SendMessage(MessageType.Error, "ATENÇÃO! Abusar desse comando é inadmissível.");
    }
}