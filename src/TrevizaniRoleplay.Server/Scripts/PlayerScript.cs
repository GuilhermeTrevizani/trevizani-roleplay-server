using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PlayerScript : Script
{
    [ServerEvent(Event.PlayerConnected)]
    public static void OnPlayerConnect(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            Functions.ConsoleLog($"OnPlayerConnect | {player.Id} | {player.Name} | {player.RealIp} | {player.RealSocialClubName}");
            player.SessionId = Convert.ToInt16(Enumerable.Range(0, 1000).FirstOrDefault(i => !Global.AllPlayers.Any(x => x.SessionId == i)));
            player.Dimension = player.ExclusiveDimension;
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [ServerEvent(Event.PlayerDisconnected)]
    public async void OnPlayerDisconnect(Player playerParam, DisconnectionType type, string reason)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            Functions.ConsoleLog($"OnPlayerDisconnect | {player.Name} | {player.Character?.Name} | {player.RealIp} | {player.RealSocialClubName} | {type} | {reason}");
            await player.Disconnect($"{type} | {reason}", true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [ServerEvent(Event.PlayerDeath)]
    public async void OnPlayerDead(Player playerParam, Player killerParam, uint reason)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Character is null)
                return;

            var killer = killerParam is null ? null : Functions.CastPlayer(killerParam);

            Functions.RunOnMainThread(() =>
            {
                player.VehicleDeath = player.IsInVehicle ? new(player.Vehicle.Id, player.VehicleSeat) : null;
            });
            await player.WriteLog(LogType.Death, Functions.Serialize(player.Wounds), killer);
            player.StopBleeding();
            player.SetWound(false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(OnPlayerDamage))]
    public static void OnPlayerDamage(Player sourceEntity, int targetEntityRemoteId, string weaponString, int boneIndex)
    {
        try
        {
            var shooter = Functions.CastPlayer(sourceEntity);
            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Id == targetEntityRemoteId);
            var weapon = Convert.ToUInt32(weaponString);
            if (weapon == 0
                || target?.Character is null
                || target.OnAdminDuty || shooter.OnAdminDuty
                || !target.Visible || !shooter.Visible)
                return;

            if (target.CancellationTokenSourceAcao is not null)
            {
                target.SendMessage(MessageType.Error, "Você sofreu dano e sua ação foi cancelada.");
                target.CancellationTokenSourceAcao?.Cancel();
                target.CancellationTokenSourceAcao = null;
                target.ToggleGameControls(true);
            }

            if (target.Character.Wound == CharacterWound.SeriouslyInjured)
            {
                target.SendMessage(MessageType.Error, Resources.YouDiedAndLostYourMemory);
                target.Character.SetWound(CharacterWound.PK);
                target.SetSharedDataEx(Constants.PLAYER_META_DATA_INJURED, (int)target.Character.Wound);
                target.Emit("DeathPage:ShowServer", (int)target.Character.Wound);
            }

            var bodyPart = (BodyPart)boneIndex;
            var damage = 0;
            var bodyPartName = Functions.GetBodyPartName(bodyPart);
            var weaponInfo = Global.WeaponsInfos.FirstOrDefault(x => x.Name.ToLower() == GlobalFunctions.GetWeaponName(weapon).ToLower());
            if (weaponInfo is not null)
            {
                damage = weaponInfo.Damage;

                var bodyPartDamage = Global.BodyPartsDamages.FirstOrDefault(x => x.Name.ToLower() == bodyPartName.ToLower());
                if (bodyPartDamage is not null)
                    damage = Convert.ToUInt16(damage * bodyPartDamage.DamageMultiplier);
            }

            target.Wounds.Add(new Wound
            {
                Weapon = GlobalFunctions.GetWeaponName(weapon),
                Damage = damage,
                BodyPart = bodyPartName,
                Attacker = $"{shooter.Character.Name} ({shooter.Character.Id})",
                Distance = target.GetPosition().DistanceTo(shooter.GetPosition()),
            });

            if (target.Character.Wound == CharacterWound.None
                && weapon != (uint)WeaponModel.Fist
                && weapon != (uint)WeaponModel.StunGun
                && weapon != (uint)WeaponModel.Snowballs
                && weapon != (uint)WeaponModel.Nightstick
                && weapon != (uint)WeaponModel.Flashlight)
                target.StartBleeding();

            target.SetNametagDamaged();

            var armorDamage = false;
            if (target.Armor > 0)
            {
                armorDamage = bodyPart == BodyPart.LowerTorso
                    || bodyPart == BodyPart.UpperTorso
                    || bodyPart == BodyPart.Chest;
            }

            var newArmor = target.Armor;
            var newHealth = target.Health;
            if (target.Character.Wound == CharacterWound.None)
            {
                if (armorDamage)
                {
                    damage = Convert.ToUInt16(damage * 1.5);
                    target.Armor = newArmor = Math.Max(0, target.Armor - damage);
                }
                else
                {
                    target.Health = newHealth = Math.Max(0, target.Health - damage);
                }
            }

            if (weapon == (uint)WeaponHash.Stungun)
                target.Emit("SetRagdoll");

            var realWeapon = Global.WeaponsInfos.FirstOrDefault(x => x.Name == GlobalFunctions.GetWeaponName(weapon));
            if (realWeapon?.AmmoItemTemplateId is not null)
            {
                var weaponType = realWeapon.AmmoItemTemplateId.ToString() switch
                {
                    Constants.PISTOL_AMMO_ITEM_TEMPLATE_ID => "pistola",
                    Constants.SUB_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID => "SMG",
                    Constants.LIGHT_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID => "metralhadora",
                    Constants.SHOTGUN_AMMO_ITEM_TEMPLATE_ID => "escopeta",
                    Constants.ASSAULT_RIFLE_AMMO_ITEM_TEMPLATE_ID => "fuzil",
                    Constants.SNIPER_RIFLE_AMMO_ITEM_TEMPLATE_ID => "rifle",
                    _ => "???",
                };

                var additionalInfo = armorDamage ?
                    $"Colete: {{{Constants.ERROR_COLOR}}}{newArmor}{{#FFFFFF}}" :
                    $"Vida: {{{Constants.ERROR_COLOR}}}{newHealth}{{#FFFFFF}}";

                target.SendMessage(MessageType.None, $"Você tomou um tiro de {{{Constants.ERROR_COLOR}}}{weaponType}{{#FFFFFF}} no {{{Constants.ERROR_COLOR}}}{bodyPartName}{{#FFFFFF}} com {{{Constants.ERROR_COLOR}}}{damage}{{#FFFFFF}} de dano. (( {additionalInfo} ))");
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(OnPlayerChat))]
    public async Task OnPlayerChat(Player playerParam, string message)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (string.IsNullOrWhiteSpace(message)
                || message.Contains("<script>")
                || message.Contains("{#")
                || player.Character is null
                || message.Length > 265)
            {
                await Functions.SendServerMessage($"{player.User.Name} tentou burlar o chat de alguma forma. Mensagem: {message}", UserStaff.GameAdmin, true);
                return;
            }

            if (message[0] == '/')
            {
                OnPlayerCommand(player, message);
                return;
            }

            if (message[0] == '.')
            {
                OnPlayerAnimation(player, message);
                return;
            }

            if (player.SPECPosition is not null)
            {
                player.SendMessage(MessageType.Error, "Você não pode enviar mensagens quando estiver de SPEC.");
                return;
            }

            if (player.OnAdminDuty)
            {
                player.SendMessageToNearbyPlayers(message, MessageCategory.OOC);
                await player.WriteLog(LogType.OOCChat, message, null);
                return;
            }

            message = Functions.CheckFinalDot(message);

            if (player.PhoneCall.Type == PhoneCallType.Answered)
            {
                if (player.Character.Wound != CharacterWound.None)
                {
                    player.SendMessage(MessageType.Error, Resources.YouCanNotExecuteThisCommandBecauseYouAreSeriouslyInjured);
                    return;
                }

                player.SendMessageToNearbyPlayers(message, MessageCategory.Cellphone);
                await player.WriteLog(LogType.ICChat, $"Celular com {player.PhoneCall.Number} | {message}", null);

                if (player.PhoneCall.Number == Constants.EMERGENCY_NUMBER)
                {
                    if (player.EmergencyCall is null)
                    {
                        EmergencyCallType? emergencyCallType = null;
                        if (message.ToLower().Contains("ambos"))
                            emergencyCallType = EmergencyCallType.PoliceAndFirefighter;
                        else if (message.ToLower().Contains("polícia") || message.Contains("policia"))
                            emergencyCallType = EmergencyCallType.Police;
                        else if (message.ToLower().Contains("bombeiro"))
                            emergencyCallType = EmergencyCallType.Firefighter;

                        if (!emergencyCallType.HasValue)
                        {
                            player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.EMERGENCY_NUMBER), "Não entendi sua mensagem. Deseja falar com polícia, bombeiros ou ambos?");
                            return;
                        }

                        player.EmergencyCall = new();
                        player.EmergencyCall.Create(emergencyCallType.Value, player.Character.Cellphone, player.ICPosition.X, player.ICPosition.Y, string.Empty, string.Empty, string.Empty);
                        player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.EMERGENCY_NUMBER), "Qual sua emergência?");
                    }
                    else if (string.IsNullOrWhiteSpace(player.EmergencyCall.Message))
                    {
                        player.EmergencyCall.Create(player.EmergencyCall.Type, player.Character.Cellphone, player.ICPosition.X, player.ICPosition.Y, message, string.Empty, string.Empty);
                        player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.EMERGENCY_NUMBER), "Qual sua localização?");
                    }
                    else
                    {
                        player.EmergencyCall.Create(player.EmergencyCall.Type, player.Character.Cellphone, player.ICPosition.X, player.ICPosition.Y, player.EmergencyCall.Message, message, string.Empty);
                        player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.EMERGENCY_NUMBER), "Nossas unidades foram alertadas.");
                        player.AreaNameType = AreaNameType.EmergencyCall;
                        player.Emit(Constants.SET_AREA_NAME);
                    }
                }
                else if (player.PhoneCall.Number == Constants.TAXI_NUMBER)
                {
                    player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.TAXI_NUMBER), "Nossos taxistas em serviço foram avisados e você receberá um SMS de confirmação.");
                    player.WaitingServiceType = CharacterJob.TaxiDriver;
                    player.AreaNameType = AreaNameType.TaxiCall;
                    player.AreaNameAuxiliar = message;
                    player.Emit(Constants.SET_AREA_NAME);
                }
                else if (player.PhoneCall.Number == Constants.MECHANIC_NUMBER)
                {
                    player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.MECHANIC_NUMBER), "Nossos mecânicos em serviço foram avisados e você receberá um SMS de confirmação.");
                    player.WaitingServiceType = CharacterJob.Mechanic;
                    player.AreaNameType = AreaNameType.MechanicCall;
                    player.AreaNameAuxiliar = message;
                    player.Emit(Constants.SET_AREA_NAME);
                }
                else if (player.PhoneCall.Number == Constants.INSURANCE_NUMBER)
                {
                    if (!int.TryParse(message.Replace(".", string.Empty), out var insuranceDays)
                        || insuranceDays <= 0)
                    {
                        player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.INSURANCE_NUMBER), "Não entendi sua mensagem. Quantos dias de seguro veicular você deseja contratar?");
                        return;
                    }

                    var vehicle = player.GetVehicle();
                    if (vehicle is null || vehicle.VehicleDB.CharacterId != player.Character.Id)
                    {
                        player.SendMessage(MessageType.Error, "Você não está dentro de um veículo seu.");
                        return;
                    }

                    var vehiclePrice = Functions.GetVehiclePrice(vehicle.VehicleDB.Model);
                    if (vehiclePrice is null)
                    {
                        player.SendMessage(MessageType.Error, Resources.VehiclePriceNotConfigured);
                        return;
                    }

                    var insuranceDayValue = Convert.ToInt32(vehiclePrice.Value.Item1 * (Global.Parameter.VehicleInsurancePercentage / 100f));
                    var insuranceValue = insuranceDayValue * insuranceDays;
                    if (player.Character.Bank < insuranceValue)
                    {
                        player.SendMessage(MessageType.Error, $"Você não possui saldo suficiente na sua conta bancária. (${insuranceValue:N0})");
                        return;
                    }

                    var context = Functions.GetDatabaseContext();
                    var financialTransaction = new FinancialTransaction();
                    financialTransaction.Create(FinancialTransactionType.Withdraw, player.Character.Id, insuranceValue,
                        $"Seguro Veicular {vehicle.Identifier} {insuranceDays} dia(s)");
                    await context.FinancialTransactions.AddAsync(financialTransaction);
                    await context.SaveChangesAsync();

                    player.RemoveBank(insuranceValue);

                    vehicle.VehicleDB.SetInsuranceDate(DateTime.Now.AddDays(insuranceDays));
                    context.Vehicles.Update(vehicle.VehicleDB);
                    await context.SaveChangesAsync();

                    await player.EndCellphoneCall();
                    player.SendCellphoneCallMessage(player.GetCellphoneContactName(Constants.INSURANCE_NUMBER), $"Você contratou {insuranceDays:N0} dia(s) de seguro veicular para {vehicle.Identifier} por ${insuranceValue:N0}.");
                }
                else
                {
                    var target = Global.SpawnedPlayers.FirstOrDefault(x => x != player && (x.PhoneCall.Number == player.Character.Cellphone || x.PhoneCall.Origin == player.Character.Cellphone));
                    target?.SendCellphoneCallMessage(player.ICName, message);
                }

                return;
            }

            var messageCategory = MessageCategory.NormalIC;
            if (player.IsInVehicle && player.VehicleSeat <= Constants.VEHICLE_SEAT_PASSENGER_BACK_RIGHT
                && player.Vehicle is MyVehicle veh && veh.HasWindows
                && !veh.IsWindowOpened(0) && !veh.IsWindowOpened(1) && !veh.IsWindowOpened(2) && !veh.IsWindowOpened(3))
            {
                messageCategory = MessageCategory.ClosedVehicleIC;
            }

            player.SendMessageToNearbyPlayers(message, messageCategory);
            await player.WriteLog(LogType.ICChat, message, null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void OnPlayerCommand(MyPlayer player, string message)
    {
        var split = message.Split(" ");
        var cmd = split[0].Replace("/", string.Empty).Trim().ToLower();
        var method = Global.Commands.FirstOrDefault(x => x.GetCustomAttribute<CommandAttribute>()!.Commands.Any(y => y.ToLower() == cmd.ToLower()));
        if (method?.DeclaringType is null)
        {
            player.SendMessage(MessageType.None, $"O comando {{{Constants.MAIN_COLOR}}}/{cmd}{{#FFFFFF}} não existe. Use {{{Constants.MAIN_COLOR}}}/ajuda{{#FFFFFF}} para visualizar os comandos disponíveis.");
            return;
        }

        var methodParams = method.GetParameters();
        var obj = Activator.CreateInstance(method.DeclaringType);
        var command = method.GetCustomAttribute<CommandAttribute>()!;

        var arr = new List<object?>();

        var list = methodParams.ToList();
        foreach (var x in list)
        {
            var index = list.IndexOf(x);
            if (index == 0)
            {
                arr.Add(player);
            }
            else
            {
                var p = split.Length <= index ? null : split[index];

                if (x.ParameterType == typeof(int))
                {
                    if (!int.TryParse(p, out int val))
                        break;

                    arr.Add(val);
                }
                else if (x.ParameterType == typeof(string))
                {
                    if (string.IsNullOrWhiteSpace(p) && !command.AllowEmptyStrings)
                        break;

                    if (command.GreedyArg && index + 1 == list.Count)
                        p = string.Join(" ", split.Skip(index).Take(split.Length - index));

                    arr.Add(p);
                }
                else if (x.ParameterType == typeof(float))
                {
                    if (!float.TryParse(p, out float val))
                        break;

                    arr.Add(val);
                }
                else if (x.ParameterType == typeof(byte))
                {
                    if (!byte.TryParse(p, out byte val))
                        break;

                    arr.Add(val);
                }
                else if (x.ParameterType == typeof(uint))
                {
                    if (!uint.TryParse(p, out uint val))
                        break;

                    arr.Add(val);
                }
                else if (x.ParameterType == typeof(uint?))
                {
                    arr.Add(uint.TryParse(p, out uint val) ? val : null);
                }
            }
        }

        if (methodParams.Length != arr.Count)
        {
            player.SendMessage(MessageType.None, $"Use: {{{Constants.MAIN_COLOR}}}/{cmd} {command.HelpText}");
            return;
        }

        method.Invoke(obj, [.. arr]);
    }

    private static void OnPlayerAnimation(MyPlayer player, string message)
    {
        message = message.Replace(".", string.Empty).ToLower();

        var animation = Global.Animations.FirstOrDefault(x => x.Display.ToLower() == message);
        if (animation is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhuma animação encontrada com o nome {message}.");
            return;
        }

        if (!player.CheckAnimations(onlyInVehicle: animation.OnlyInVehicle))
            return;

        if (string.IsNullOrWhiteSpace(animation.Scenario))
        {
            player.PlayAnimation(animation.Dictionary, animation.Name, animation.Flag);
        }
        else
        {
            player.StopAnimationEx();
            player.PlayScenario(animation.Scenario);
        }
    }

    [RemoteEvent(nameof(SetGameFocused))]
    public void SetGameFocused(Player playerParam, bool isGameFocused)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.SetSharedDataEx(Constants.PLAYER_META_DATA_GAME_UNFOCUSED, !isGameFocused);
            if (isGameFocused)
            {
                player.AFKSince = null;
            }
            else
            {
                if (!player.AFKSince.HasValue)
                    player.AFKSince = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SaveOutfits))]
    public async Task SaveOutfits(Player playerParam, int outfit, string outfitsJson, int type, bool success, int index, int length)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (success)
            {
                player.TemporaryOutfits.Add(new(index, outfitsJson));
                if (player.TemporaryOutfits.Count != length)
                    return;

                var temporaryOutfitsJson = string.Join(string.Empty, player.TemporaryOutfits.OrderBy(x => x.Item1).Select(x => x.Item2));

                if (type == 3)
                {
                    if (player.Faction is null)
                        return;

                    var creatingOutfit = Functions.Deserialize<IEnumerable<Outfit>>(temporaryOutfitsJson)
                        .FirstOrDefault(x => x.Slot == 1)!;

                    var factionUniform = new FactionUniform();
                    factionUniform.Create(player.Faction.Id, player.CreatingOutfitName!, Functions.Serialize(creatingOutfit), player.Character.Sex);

                    var context = Functions.GetDatabaseContext();
                    await context.FactionsUniforms.AddAsync(factionUniform);
                    await context.SaveChangesAsync();

                    Global.FactionsUniforms.Add(factionUniform);

                    await player.WriteLog(LogType.Faction, $"/criaruniforme {Functions.Serialize(factionUniform)}", null);
                    player.SendMessage(MessageType.Success, $"Você criou o uniforme {player.CreatingOutfitName}.");
                }
                else
                {
                    if (player.UsingOutfitsOnDuty)
                        player.Character.SetOutfitOnDuty((byte)outfit, temporaryOutfitsJson);
                    else
                        player.Character.SetOutfit((byte)outfit, temporaryOutfitsJson);
                }

                player.TemporaryOutfits = [];
            }

            player.SetOutfit();

            if (type == 0)
            {
                var context = Functions.GetDatabaseContext();
                player.Character.SetPersonalizationStep(CharacterPersonalizationStep.Ready);
                context.Characters.Update(player.Character);
                await context.SaveChangesAsync();
                await player.SpawnEx();
            }
            else
            {
                player.Visible = true;
                player.Invincible = false;
                player.Emit(Constants.CLOTHES_PAGE_CLOSE);

                if (type == 1 && success)
                {
                    await player.RemoveMoney(Global.Parameter.ClothesValue);
                    player.SendMessage(MessageType.Success, $"Você pagou ${Global.Parameter.ClothesValue:N0} na loja de roupas.");
                }
                player.SelectCharacter();
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmPersonalization))]
    public async Task ConfirmPersonalization(Player playerParam, string personalizationJson, int type, bool sucesso)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (sucesso)
                player.Personalization = Functions.Deserialize<Personalization>(personalizationJson);

            player.Visible = true;
            player.Invincible = false;

            if (type == 1)
            {
                var context = Functions.GetDatabaseContext();
                player.Character.SetPersonalizationStep(CharacterPersonalizationStep.Tattoos);
                player.Character.SetPersonalizationJSON(personalizationJson);
                context.Characters.Update(player.Character);
                await context.SaveChangesAsync();
            }
            else if (sucesso)
            {
                if (type == 2)
                {
                    await player.RemoveMoney(Global.Parameter.BarberValue);
                    player.SendMessage(MessageType.Success, $"Você pagou ${Global.Parameter.BarberValue:N0} na barbearia.");
                }
                else if (type == 3)
                {
                    await player.RemoveMoney(Global.Parameter.PlasticSurgeryValue);
                    player.SendMessage(MessageType.Success, $"Você pagou ${Global.Parameter.PlasticSurgeryValue:N0} na cirurgia plástica.");
                }
            }

            player.SetPersonalization();
            player.SelectCharacter();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(Chatting))]
    public void Chatting(Player playerParam, bool chatting)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.SetSharedDataEx(Constants.PLAYER_META_DATA_CHATTING, chatting);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StopAnimation))]
    public static void StopAnimation(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.StopAnimationEx();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(Waypoint))]
    public static void Waypoint(Player playerParam, float x, float y, float z)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.User.Staff < UserStaff.GameAdmin)
                return;

            player.SetPosition(new(x, y, z), player.GetDimension(), false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ShowPlayerList))]
    public static void ShowPlayerList(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var players = Global.SpawnedPlayers;
            var duty = players.Where(x => x.OnDuty);

            player.Emit("Server:ListarPlayers",
                Functions.Serialize(players.OrderBy(x => x.SessionId == player.SessionId ? 0 : 1).ThenBy(x => x.SessionId)
                .Select(x => new { Id = x.SessionId, Name = x.ICName, x.Ping }).ToList()),
                Functions.Serialize(new
                {
                    Police = duty.Count(x => x.Faction?.Type == FactionType.Police),
                    Fire = duty.Count(x => x.Faction?.Type == FactionType.Firefighter),
                    Mechanic = duty.Count(x => x.Character.Job == CharacterJob.Mechanic),
                    TaxiDriver = duty.Count(x => x.Character.Job == CharacterJob.TaxiDriver),
                }));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(KeyY))]
    public async Task KeyY(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsActionsBlocked())
            {
                player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.DismantlingVehicleId is not null)
            {
                player.SendNotification(NotificationType.Error, "Você está desmanchando um veículo.");
                return;
            }

            if (player.RobberingPropertyId is not null)
            {
                player.SendNotification(NotificationType.Error, "Você está roubando uma propriedade.");
                return;
            }

            var distance = 1.5f;

            var info = Global.Infos
                .Where(x => x.Image
                    && x.Dimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
                .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
            if (info is not null)
            {
                player.Emit("OpenImage", info.Message);
                return;
            }

            void OpenBuilding(Property property)
            {
                var properties = Global.Properties
                    .Where(x => x.ParentPropertyId == property.Id)
                    .OrderBy(x => x.FormatedAddress);
                if (!properties.Any())
                {
                    player.SendNotification(NotificationType.Error, "O prédio não possui nenhuma propriedade. Por favor, reporte o bug.");
                    return;
                }

                player.Emit(Constants.PROPERTY_BUILDING_PAGE_SHOW, property.Name, Functions.Serialize(properties.Select(x => new
                {
                    x.Id,
                    x.FormatedAddress,
                    x.Value,
                    CanBuy = x.Value > 0 && !x.CharacterId.HasValue,
                    CanAccess = x.CanAccess(player),
                    IsOwner = x.CharacterId == player.Character.Id,
                })));
            }

            var prox = Global.Properties
                .Where(x => x.EntranceDimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= distance)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
            if (prox is not null)
            {
                if (prox.Locked)
                {
                    player.SendNotification(NotificationType.Error, "A porta está trancada.");
                    return;
                }

                if (prox.RobberyValue > 0)
                {
                    player.SendNotification(NotificationType.Error, Resources.PropertyHasBeenStolen);
                    return;
                }

                if (player.IsInVehicle)
                {
                    player.SendNotification(NotificationType.Error, "Você está dentro de um veículo.");
                    return;
                }

                if (prox.Interior == PropertyInterior.Building)
                {
                    OpenBuilding(prox);
                    return;
                }

                player.SetPosition(prox.GetExitPosition(), prox.Number, false);
                player.SetRotation(prox.GetExitRotation());
                await CheckCompanyBenefit(player, prox);
                return;
            }

            prox = Global.Properties
                .Where(x => player.GetDimension() == x.Number
                    && player.GetPosition().DistanceTo(x.GetExitPosition()) <= distance)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetExitPosition()));
            if (prox is not null)
            {
                if (prox.Locked)
                {
                    player.SendNotification(NotificationType.Error, "A porta está trancada.");
                    return;
                }

                if (player.IsInVehicle)
                {
                    player.SendNotification(NotificationType.Error, "Você está dentro de um veículo.");
                    return;
                }

                if (player.PropertyNoClip)
                    await CMD_propnoclip(player);

                if (prox.ParentPropertyId.HasValue)
                {
                    var propertyParent = Global.Properties.FirstOrDefault(x => x.Id == prox.ParentPropertyId);
                    if (propertyParent?.Interior == PropertyInterior.Building)
                    {
                        player.SetPosition(propertyParent.GetEntrancePosition(), prox.EntranceDimension, false);
                        player.SetRotation(propertyParent.GetEntranceRotation());
                        return;
                    }
                }

                player.SetPosition(prox.GetEntrancePosition(), prox.EntranceDimension, false);
                player.SetRotation(prox.GetEntranceRotation());
                return;
            }

            var propertiesEntrances = Global.Properties.SelectMany(x => x.Entrances!);
            var propertyEntrance = propertiesEntrances
                .Where(x => x.GetProperty().EntranceDimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= distance)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
            if (propertyEntrance is not null)
            {
                if (propertyEntrance.GetProperty().Locked)
                {
                    player.SendNotification(NotificationType.Error, "A porta está trancada.");
                    return;
                }

                if (player.IsInVehicle)
                {
                    player.SendNotification(NotificationType.Error, "Você está dentro de um veículo.");
                    return;
                }

                if (propertyEntrance.GetProperty().Interior == PropertyInterior.Building)
                {
                    OpenBuilding(propertyEntrance.GetProperty());
                    return;
                }

                player.SetPosition(propertyEntrance.GetExitPosition(), propertyEntrance.GetProperty().Number, false);
                player.SetRotation(propertyEntrance.GetExitRotation());
                await CheckCompanyBenefit(player, propertyEntrance.GetProperty());
                return;
            }

            var propertyExit = propertiesEntrances
                .Where(x => x.GetProperty().Number == player.GetDimension()
                    && player.GetPosition().DistanceTo(x.GetExitPosition()) <= distance)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetExitPosition()));
            if (propertyExit is not null)
            {
                if (propertyExit.GetProperty().Locked)
                {
                    player.SendNotification(NotificationType.Error, "A porta está trancada.");
                    return;
                }

                if (player.IsInVehicle)
                {
                    player.SendNotification(NotificationType.Error, "Você está dentro de um veículo.");
                    return;
                }

                if (player.PropertyNoClip)
                    await CMD_propnoclip(player);

                if (propertyExit.GetProperty().ParentPropertyId.HasValue)
                {
                    var propertyParent = Global.Properties.FirstOrDefault(x => x.Id == propertyExit.GetProperty().ParentPropertyId);
                    if (propertyParent?.Interior == PropertyInterior.Building)
                    {
                        player.SetPosition(propertyParent.GetEntrancePosition(), propertyParent.EntranceDimension, false);
                        player.SetRotation(propertyParent.GetEntranceRotation());
                        return;
                    }
                }

                player.SetPosition(propertyExit.GetEntrancePosition(), propertyExit.GetProperty().EntranceDimension, false);
                player.SetRotation(propertyExit.GetEntranceRotation());
                return;
            }

            var spot = Global.Spots
                .Where(x => x.Dimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance)
                .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
            if (spot is not null)
            {
                if (spot.Type == SpotType.Company)
                {
                    var company = Global.Companies
                        .Where(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= 50)
                        .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
                    if (company is null && player.GetDimension() != 0)
                    {
                        var companyProperty = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
                        company = Global.Companies.FirstOrDefault(x => x.Id == companyProperty?.CompanyId);
                    }

                    if (company is null)
                    {
                        player.SendNotification(NotificationType.Error, "Nenhuma empresa encontrada em 50 metros. Por favor, reporte o bug.");
                        return;
                    }

                    if (company.Type == CompanyType.ConvenienceStore)
                    {
                        if (company.Items!.Count == 0)
                        {
                            player.SendNotification(NotificationType.Error, $"{company.Name} não possui itens. Por favor, reporte o bug.");
                            return;
                        }

                        if (company.EmployeeOnDuty.HasValue)
                        {
                            if (company.EmployeeOnDuty != player.Character.Id)
                            {
                                player.SendNotification(NotificationType.Error, $"{company.Name} possui um funcionário em serviço. Por favor, interaja com ele.");
                                return;
                            }
                        }
                        else
                        {
                            if (Global.Companies.Any(x => x.EmployeeOnDuty.HasValue))
                            {
                                player.SendNotification(NotificationType.Error, "Há uma loja de conveniência com um funcionário em serviço. Não é possível comprar aqui.");
                                return;
                            }
                        }

                        company.BuyItems(player);
                    }
                    else if (company.Type == CompanyType.Other)
                    {
                        if (!player.CheckCompanyPermission(company, null))
                        {
                            player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                            return;
                        }

                        if (company.Items!.Count == 0)
                        {
                            player.SendNotification(NotificationType.Error, $"{company.Name} não possui itens. Por favor, reporte o bug.");
                            return;
                        }

                        company.BuyItems(player);
                    }
                    else if (company.Type == CompanyType.MechanicWorkshop)
                    {
                        MyPlayer? target = null;
                        if (player.CheckCompanyPermission(company, null))
                        {
                            if (player.Character.Job != CharacterJob.Mechanic)
                            {
                                player.SendNotification(NotificationType.Error, "Você não é mecânico.");
                                return;
                            }

                            if (!player.OnDuty)
                            {
                                player.SendNotification(NotificationType.Error, "Você não está em serviço.");
                                return;
                            }
                        }
                        else
                        {
                            target = Global.SpawnedPlayers
                                .Where(x => x.OnDuty && x.Character.Job == CharacterJob.Mechanic
                                    && player.GetPosition().DistanceTo(x.GetPosition()) <= 10
                                    && x.CheckCompanyPermission(company, null))
                                .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
                            if (target is null)
                            {
                                player.SendNotification(NotificationType.Error, "Não há nenhum mecânico em serviço da empresa próximo de você.");
                                return;
                            }
                        }

                        Functions.CMDTuning(player, target, company, false);
                    }
                    else if (company.Type == CompanyType.Fishmonger)
                    {
                        if (company.Items!.Count == 0)
                        {
                            player.SendNotification(NotificationType.Error, $"{company.Name} não possui itens. Por favor, reporte o bug.");
                            return;
                        }

                        company.SellItems(player);
                    }
                    else if (company.Type == CompanyType.WeaponStore)
                    {
                        if (company.Items!.Count == 0)
                        {
                            player.SendNotification(NotificationType.Error, $"{company.Name} não possui itens. Por favor, reporte o bug.");
                            return;
                        }

                        company.BuyItems(player);
                    }
                    else
                    {
                        player.SendNotification(NotificationType.Error, "Tipo da empresa não implementado. Por favor, reporte o bug.");
                    }

                    return;
                }

                if (spot.Type == SpotType.BarberShop)
                {
                    if (!player.ValidPed)
                    {
                        player.SendMessage(MessageType.Error, Resources.YouDontHaveAValidSkin);
                        return;
                    }

                    if (player.Money < Global.Parameter.BarberValue)
                    {
                        player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.BarberValue));
                        return;
                    }

                    player.Edit(2);
                    return;
                }

                if (spot.Type == SpotType.ClothesStore)
                {
                    if (!player.ValidPed)
                    {
                        player.SendMessage(MessageType.Error, Resources.YouDontHaveAValidSkin);
                        return;
                    }

                    if (player.Money < Global.Parameter.ClothesValue)
                    {
                        player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.ClothesValue));
                        return;
                    }

                    player.EditOutfits(1);
                    return;
                }

                if (spot.Type == SpotType.TattooShop)
                {
                    if (!player.ValidPed)
                    {
                        player.SendMessage(MessageType.Error, Resources.YouDontHaveAValidSkin);
                        return;
                    }

                    if (player.Money < Global.Parameter.TattooValue)
                    {
                        player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.TattooValue));
                        return;
                    }

                    var outfit = new Outfit
                    {
                        Accessory0 = new() { Using = false },
                        Accessory1 = new() { Using = false },
                        Accessory2 = new() { Using = false },
                        Accessory6 = new() { Using = false },
                        Accessory7 = new() { Using = false },
                        Cloth1 = new() { Using = false },
                        Cloth3 = new() { Using = false },
                        Cloth4 = new() { Using = false },
                        Cloth5 = new() { Using = false },
                        Cloth6 = new() { Using = false },
                        Cloth7 = new() { Using = false },
                        Cloth8 = new() { Using = false },
                        Cloth9 = new() { Using = false },
                        Cloth10 = new() { Using = false },
                        Cloth11 = new() { Using = false },
                    };

                    player.Invincible = true;
                    player.Visible = false;
                    player.Emit("OpenTattoo", (int)player.Character.Sex, Functions.Serialize(player.Personalization), Functions.Serialize(outfit), true);
                    return;
                }

                if (spot.Type == SpotType.PlasticSurgery)
                {
                    if (!player.ValidPed)
                    {
                        player.SendMessage(MessageType.Error, Resources.YouDontHaveAValidSkin);
                        return;
                    }

                    if (player.Money < Global.Parameter.PlasticSurgeryValue)
                    {
                        player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.PlasticSurgeryValue));
                        return;
                    }

                    player.Edit(3);
                    return;
                }

                if (spot.Type == SpotType.ForensicLaboratory)
                {
                    if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
                    {
                        player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
                        return;
                    }

                    var jsonTypes = Functions.Serialize(
                        Enum.GetValues<ForensicTestItemType>()
                        .Select(x => new
                        {
                            Value = ((byte)x).ToString(),
                            Label = x.GetDescription(),
                        })
                        .OrderBy(x => x.Label)
                    );

                    player.Emit("ForensicLaboratory:Show", jsonTypes);
                    return;
                }

                if (spot.Type == SpotType.Morgue)
                {
                    if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
                    {
                        player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
                        return;
                    }

                    var context = Functions.GetDatabaseContext();
                    var bodies = await context.Bodies
                        .Where(x => x.MorgueDate.HasValue)
                        .OrderByDescending(x => x.RegisterDate)
                        .Take(50)
                        .ToListAsync();
                    if (bodies.Count == 0)
                    {
                        player.SendMessage(MessageType.Error, "Não há nenhum corpo no necrotério.");
                        return;
                    }

                    player.Emit("Morgue:Show", Functions.Serialize(bodies.Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.RegisterDate,
                        x.MorgueDate,
                        x.PlaceOfDeath,
                        IsInformationAvailable = x.IsInformationAvailable(),
                    })));

                    return;
                }

                if (spot.Type == SpotType.HealMe)
                {
                    if (player.Character.Wound >= CharacterWound.PK)
                    {
                        player.SendMessage(MessageType.Error, Resources.YouDiedAndLostYourMemory);
                        return;
                    }

                    if (!player.Wounded)
                    {
                        player.SendMessage(MessageType.Error, "Você não está ferido.");
                        return;
                    }

                    var valor = Global.Parameter.HospitalValue / 2;
                    if (player.Money < valor)
                    {
                        player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, valor));
                        return;
                    }

                    var onDuty = Global.SpawnedPlayers.Count(x => x.Faction?.Type == FactionType.Firefighter && x.OnDuty);
                    if (onDuty > Global.Parameter.FirefightersBlockHeal)
                    {
                        player.SendMessage(MessageType.Error, $"Não é possível curar pois há {onDuty} bombeiro(s) em trabalho.");
                        return;
                    }

                    player.ToggleGameControls(false);
                    player.SendMessage(MessageType.Success, $"Aguarde 30 segundos. Pressione DELETE para cancelar a ação.");
                    player.CancellationTokenSourceAcao?.Cancel();
                    player.CancellationTokenSourceAcao = new CancellationTokenSource();
                    await Task.Delay(TimeSpan.FromSeconds(30), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                            return;

                        Task.Run(async () =>
                        {
                            player.Heal();
                            await player.RemoveMoney(valor);
                            player.ToggleGameControls(true);
                            player.SendMessage(MessageType.Success, $"Você tratou seus ferimentos por ${valor:N0}.");
                            await player.WriteLog(LogType.HealMe, string.Empty, null);
                            player.CancellationTokenSourceAcao = null;
                        });
                    });

                    return;
                }

                if (spot.Type == SpotType.DMV)
                {
                    if (player.Character.DriverLicenseValidDate?.Date > DateTime.Now.Date
                        && !player.Character.PoliceOfficerBlockedDriverLicenseCharacterId.HasValue)
                    {
                        player.SendMessage(MessageType.Error, "Sua licença de motorista não vence hoje ou não está revogada.");
                        return;
                    }

                    if (player.Character.PoliceOfficerBlockedDriverLicenseCharacterId.HasValue
                        && player.Character.DriverLicenseBlockedDate?.Date > DateTime.Now.Date)
                    {
                        player.SendMessage(MessageType.Error, $"Sua licença de motorista está bloqueada até {player.Character.DriverLicenseBlockedDate?.Date.ToShortDateString()}.");
                        return;
                    }

                    var value = player.Character.DriverLicenseValidDate.HasValue ? Global.Parameter.DriverLicenseRenewValue : Global.Parameter.DriverLicenseBuyValue;
                    if (player.Money < value)
                    {
                        player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, value));
                        return;
                    }

                    player.Character.SetDriverLicense();
                    await player.Save();
                    await player.RemoveMoney(value);

                    player.SendMessage(MessageType.Success, $"Você comprou/renovou sua licença de motorista por ${value:N0}. A validade é {player.Character.DriverLicenseValidDate?.ToShortDateString()}.");

                    return;
                }

                if (spot.Type == SpotType.FactionVehicleSpawn)
                {
                    if (!(player.Faction?.HasVehicles ?? false))
                    {
                        player.SendMessage(MessageType.Error, "Você não está em uma facção habilitada.");
                        return;
                    }

                    var context = Functions.GetDatabaseContext();
                    var vehicles = (await context.Vehicles.Where(x => x.FactionId == player.Character.FactionId && !x.Sold).ToListAsync())
                        .OrderBy(x => Convert.ToInt32(Global.Vehicles.Any(y => y.VehicleDB.Id == x.Id)))
                            .ThenBy(x => x.Model)
                                .ThenBy(x => x.Plate)
                        .Select(x => new
                        {
                            x.Id,
                            Model = x.Model.ToUpper(),
                            x.Plate,
                            InChargeCharacterName = Global.Vehicles.FirstOrDefault(y => y.VehicleDB.Id == x.Id)?.NameInCharge ?? string.Empty,
                            SessionId = Global.Vehicles.FirstOrDefault(y => y.VehicleDB.Id == x.Id)?.Id,
                            x.Description,
                        }).ToList();
                    if (vehicles.Count == 0)
                    {
                        player.SendMessage(MessageType.Error, "Sua facção não possui veículos para spawnar.");
                        return;
                    }

                    player.Emit("SpawnFactionVehicle", player.Faction.Name, Functions.Serialize(vehicles));
                    return;
                }

                if (spot.Type == SpotType.Bank)
                {
                    await Functions.ShowBank(player, false, true, false);
                    return;
                }
            }

            var dealership = Global.Dealerships
                .FirstOrDefault(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= distance);
            if (dealership is not null)
            {
                var vehicles = Global.DealershipsVehicles.Where(x => x.DealershipId == dealership.Id)
                    .OrderBy(x => x.Model)
                    .Select(x => new
                    {
                        Model = x.Model.ToUpper(),
                        Price = x.Value,
                        Restriction = Functions.CheckPremiumVehicle(x.Model),
                    }).ToList();
                if (vehicles.Count == 0)
                {
                    player.SendMessage(MessageType.Error, "Nenhum veículo configurado para esta concessionária. Por favor, reporte o bug.");
                    return;
                }

                player.Visible = false;
                player.SetPosition(new(dealership.VehiclePosX, dealership.VehiclePosY, dealership.VehiclePosZ), player.ExclusiveDimension, false);
                player.SetHour(12);
                player.Emit("OpenDealership", dealership.Name, Functions.Serialize(vehicles),
                    dealership.VehiclePosX, dealership.VehiclePosY, dealership.VehiclePosZ);
                return;
            }

            if (player.Vehicle is MyVehicle vehicle && vehicle.Driver == player)
            {
                await CMD_motor(player);
                return;
            }

            player.SendNotification(NotificationType.Error, "Você não está próximo de um ponto de interação.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task CheckCompanyBenefit(MyPlayer player, Property property)
    {
        if (property.CompanyId is null)
            return;

        var company = Global.Companies.FirstOrDefault(x => x.Id == property.CompanyId);
        if (company is null || !company.GetIsOpen()
            || !company.CharacterId.HasValue
            || !company.EntranceBenefit
            || Global.Parameter.EntranceBenefitValue == 0
            || company.EntranceBenefitCooldown > DateTime.Now)
            return;

        var users = Functions.Deserialize<List<Guid>>(company.EntranceBenefitUsersJson);
        if (users.Contains(player.User.Id))
            return;

        await company.MovementSafe(player, company.CharacterId.Value, FinancialTransactionType.Deposit,
            Global.Parameter.EntranceBenefitValue, "Benefício Entrada");

        users.Add(player.User.Id);
        company.SetEntranceBenefit(users.Count == Global.Parameter.EntranceBenefitCooldownUsers ?
            DateTime.Now.AddHours(Global.Parameter.EntranceBenefitCooldownHours)
            :
            null,
            users.Count == Global.Parameter.EntranceBenefitCooldownUsers ?
            "[]"
            :
            Functions.Serialize(users));

        var context = Functions.GetDatabaseContext();

        context.Companies.Update(company);

        await context.SaveChangesAsync();
    }

    [Command(["propnoclip"], "Propriedades", "Ativa /desativa a câmera livre para mobiliar")]
    public async Task CMD_propnoclip(MyPlayer player)
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

        player.PropertyNoClip = !player.PropertyNoClip;
        player.SetCurrentWeapon((uint)WeaponModel.Fist);
        player.Invincible = player.PropertyNoClip;
        player.StartNoClip(false);
        await player.WriteLog(LogType.EditPropertyFurniture, $"/propnoclip {player.PropertyNoClip} {property.FormatedAddress} {property.Number}", null);
        player.SendNotification(NotificationType.Success, $"Você {(!player.PropertyNoClip ? "des" : string.Empty)}ativou a câmera livre.");
    }

    [Command(["motor"], "Veículos", "Liga/desliga o motor de um veículo")]
    public static async Task CMD_motor(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
        {
            player.SendNotification(NotificationType.Error, Resources.YouAreNotTheDriverOfTheVehicle);
            return;
        }

        if (vehicle.SpawnType == MyVehicleSpawnType.TestDrive)
        {
            vehicle.SetEngineStatus(!vehicle.GetEngineStatus());
            return;
        }

        if (!vehicle.CanAccess(player))
        {
            player.SendNotification(NotificationType.Error, Resources.YouDoNotHaveAccessToTheVehicle);
            return;
        }

        if (vehicle.HasFuelTank && vehicle.VehicleDB.Fuel == 0)
        {
            player.SendNotification(NotificationType.Error, "Veículo não possui combustível.");
            return;
        }

        if (vehicle.GetClass() == VehicleClass.Cycles)
        {
            player.SendNotification(NotificationType.Error, "Veículo não possui motor.");
            return;
        }

        if (vehicle.GetEngineHealth() < 700 && !vehicle.GetEngineStatus())
        {
            if (player.CancellationTokenSourceAcao is not null)
            {
                player.SendNotification(NotificationType.Error, "Você já está tentando ligar o motor.");
                return;
            }

            var seconds = new Random().Next(3, 6);
            var success = Convert.ToBoolean(new Random().Next(0, 2));

            player.SendMessage(MessageType.Success, $"Tentando ligar o motor. Aguarde {seconds} segundos. Pressione DELETE para cancelar a ação.");
            player.CancellationTokenSourceAcao?.Cancel();
            player.CancellationTokenSourceAcao = new CancellationTokenSource();
            await Task.Delay(TimeSpan.FromSeconds(seconds), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                if (success)
                {
                    player.SendMessageToNearbyPlayers("liga o motor do veículo.", MessageCategory.Ame);
                    vehicle.SetEngineStatus(true);
                }
                else
                {
                    player.SendMessage(MessageType.Error, "O motor do veículo não ligou. Tente novamente.");
                }

                player.CancellationTokenSourceAcao = null;
            });
            return;
        }

        vehicle.SetEngineStatus(!vehicle.GetEngineStatus());
        player.SendMessageToNearbyPlayers($"{(!vehicle.GetEngineStatus() ? "des" : string.Empty)}liga o motor do veículo.", MessageCategory.Ame);
    }

    [RemoteEvent(nameof(KeyK))]
    public static void KeyK(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotTheDriverOfTheVehicle);
                return;
            }

            var prox = Global.Properties
                .Where(x => x.EntranceDimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= 5)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
            if (prox is not null)
            {
                if (prox.Locked)
                {
                    player.SendNotification(NotificationType.Error, "A porta está trancada.");
                    return;
                }

                vehicle.Dimension = prox.Number;
                vehicle.Position = prox.GetExitPosition();
                vehicle.Rotation = prox.GetExitRotation();

                return;
            }

            prox = Global.Properties
                .Where(x => player.GetDimension() == x.Number
                    && player.GetPosition().DistanceTo(x.GetExitPosition()) <= 5)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetExitPosition()));
            if (prox is not null)
            {
                if (prox.Locked)
                {
                    player.SendNotification(NotificationType.Error, "A porta está trancada.");
                    return;
                }

                vehicle.Dimension = prox.EntranceDimension;
                vehicle.Position = prox.GetEntrancePosition();
                vehicle.Rotation = prox.GetEntranceRotation();

                return;
            }

            player.SendNotification(NotificationType.Error, "Você não está próximo de uma garagem.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(KeyDelete))]
    public static void KeyDelete(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.RobberingPropertyId = null;
            player.DismantlingVehicleId = null;
            if (player.CancellationTokenSourceAcao is null)
                return;

            player.CancellationTokenSourceAcao?.Cancel();
            player.CancellationTokenSourceAcao = null;
            player.ToggleGameControls(true);
            player.SendNotification(NotificationType.Success, "Você cancelou sua ação.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmTattoos))]
    public async Task ConfirmTattoos(Player playerParam, string strTattoos, bool studio, bool success)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (success)
            {
                var tattoos = Functions.Deserialize<List<Personalization.Tattoo>>(strTattoos);
                if (tattoos.Count > 30)
                {
                    player.SendNotification(NotificationType.Error, "O limite é de 30 tatuagens.");
                    return;
                }

                player.Personalization.Tattoos = tattoos;

                if (studio)
                {
                    await player.RemoveMoney(Global.Parameter.TattooValue);
                    player.SendMessage(MessageType.Success, $"Você pagou ${Global.Parameter.TattooValue:N0} no estúdio de tatuagens.");
                }
                else
                {
                    var context = Functions.GetDatabaseContext();
                    player.Character.SetPersonalizationStep(CharacterPersonalizationStep.Clothes);
                    player.Character.SetPersonalizationJSON(Functions.Serialize(player.Personalization));
                    context.Characters.Update(player.Character);
                    await context.SaveChangesAsync();
                }
            }

            player.Visible = true;
            player.Invincible = false;

            if (studio)
                player.SetOutfit();

            player.SetPersonalization();
            player.Emit(Constants.TATTOOS_PAGE_CLOSE);
            player.SelectCharacter();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmBoombox))]
    public async Task ConfirmBoombox(Player playerParam, string itemId, string url, float volume)
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

            var item = Global.Items.FirstOrDefault(x => x.Id == new Guid(itemId));
            if (item is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var audioSpot = item.GetAudioSpot();
            audioSpot ??= new AudioSpot
            {
                Position = new Vector3(item.PosX, item.PosY, item.PosZ),
                Dimension = item.Dimension,
                ItemId = item.Id,
                Range = item.GetItemType(),
            };

            audioSpot.Source = url;
            audioSpot.Volume = volume;

            audioSpot.SetupAllClients();

            player.SendMessageToNearbyPlayers("configura a boombox.", MessageCategory.Ame);
            player.Emit(Constants.BOOMBOX_PAGE_CLOSE);
            var context = Functions.GetDatabaseContext();
            await player.WriteLog(LogType.General, $"Configurar Boombox {item.Id} {item.PosX} {item.PosY} {item.PosZ} {url} {volume}", null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(TurnOffBoombox))]
    public async Task TurnOffBoombox(Player playerParam, string itemId)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var item = Global.Items.FirstOrDefault(x => x.Id == new Guid(itemId));
            if (item is null)
                return;

            var audioSpot = item.GetAudioSpot();
            if (audioSpot is not null)
            {
                audioSpot.RemoveAllClients();
                player.SendMessageToNearbyPlayers($"desliga a boombox.", MessageCategory.Ame);
            }

            player.Emit(Constants.BOOMBOX_PAGE_CLOSE);
            var context = Functions.GetDatabaseContext();
            await player.WriteLog(LogType.General, $"Desligar Boombox {item.Id} {item.PosX} {item.PosY} {item.PosZ}", null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(OnPlayerEvent))]
    private static void OnPlayerEvent(Player playerParam, string description)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            Functions.ConsoleLog($"OnPlayerEvent | {player.Character?.Name ?? player.User?.Name ?? player.RealIp} | {description}");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SetAreaName))]
    public async Task SetAreaName(Player playerParam, string areaName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.GetDimension() != 0)
            {
                var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
                if (property is not null)
                    areaName = property.FormatedAddress;
            }

            var context = Functions.GetDatabaseContext();
            switch (player.AreaNameType)
            {
                case AreaNameType.EmergencyCall:
                    if (player.EmergencyCall is null)
                        return;

                    player.EmergencyCall.Create(player.EmergencyCall.Type, player.EmergencyCall.Number,
                        player.EmergencyCall.PosX, player.EmergencyCall.PosY,
                        player.EmergencyCall.Message, player.EmergencyCall.Location, areaName);
                    await context.EmergencyCalls.AddAsync(player.EmergencyCall);
                    await context.SaveChangesAsync();
                    Global.EmergencyCalls.Add(player.EmergencyCall);
                    await player.EmergencyCall.SendMessage();

                    await player.EndCellphoneCall();
                    break;
                case AreaNameType.VehicleEmergencyCall:
                    if (player.VehicleEmergencyCall is null)
                        return;

                    player.VehicleEmergencyCall.Create(player.VehicleEmergencyCall.Type, player.VehicleEmergencyCall.Number,
                        player.VehicleEmergencyCall.PosX, player.VehicleEmergencyCall.PosY,
                        player.VehicleEmergencyCall.Message, areaName, areaName);

                    player.VehicleEmergencyCall.Create(player.VehicleEmergencyCall.Type, player.Character.Cellphone, player.ICPosition.X, player.ICPosition.Y, player.VehicleEmergencyCall.Location, player.VehicleEmergencyCall.Message, areaName);

                    await context.EmergencyCalls.AddAsync(player.VehicleEmergencyCall);
                    await context.SaveChangesAsync();
                    Global.EmergencyCalls.Add(player.VehicleEmergencyCall);
                    await player.VehicleEmergencyCall.SendMessage();
                    break;
                case AreaNameType.TaxiCall:
                    Functions.SendJobMessage(CharacterJob.TaxiDriver, $"{Resources.TaxiDriversCenter} | Solicitação de Táxi {{#FFFFFF}}#{player.SessionId}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.TaxiDriver, $"De: {{#FFFFFF}}{player.Character.Cellphone}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.TaxiDriver, $"Localização: {{#FFFFFF}}{areaName}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.TaxiDriver, $"Destino: {{#FFFFFF}}{player.AreaNameAuxiliar}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.TaxiDriver, $"Use /atcha {player.SessionId} para aceitar.");

                    await player.EndCellphoneCall();
                    break;
                case AreaNameType.MechanicCall:
                    Functions.SendJobMessage(CharacterJob.Mechanic, $"{Resources.MechanicsCenter} | Solicitação de Mecânico {{#FFFFFF}}#{player.SessionId}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.Mechanic, $"De: {{#FFFFFF}}{player.Character.Cellphone}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.Mechanic, $"Localização: {{#FFFFFF}}{areaName}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.Mechanic, $"Mensagem: {{#FFFFFF}}{player.AreaNameAuxiliar}", Constants.CELLPHONE_SECONDARY_COLOR);
                    Functions.SendJobMessage(CharacterJob.TaxiDriver, $"Use /atcha {player.SessionId} para aceitar.");

                    await player.EndCellphoneCall();
                    break;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CreateBlood))]
    public async Task CreateBlood(Player playerParam, Vector3 position)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);

            var item = new Item();
            item.Create(new Guid(Constants.BLOOD_SAMPLE_ITEM_TEMPLATE_ID), 0, 1, Functions.Serialize(new BloodSampleItem
            {
                BloodType = player.Character.BloodType,
                CharacterId = player.Character.Id,
            }));

            item.SetPosition(player.GetDimension(), position.X, position.Y, position.Z + 0.001f, -89.744356156f, -58.5174573127f, 179.9999912f);

            var context = Functions.GetDatabaseContext();
            await context.Items.AddAsync(item);
            await context.SaveChangesAsync();

            Global.Items.Add(item);
            item.CreateObject();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ToggleMarkers))]
    public static void ToggleMarkers(Player playerParam, bool state)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.Emit("ToggleMarkers", Functions.Serialize(
                Global.Markers
                .Where(x => x.Dimension == player.GetDimension() && x.Position.DistanceTo(player.GetPosition()) <= 150)
                .Select(x => x.Id)
                ), state);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}