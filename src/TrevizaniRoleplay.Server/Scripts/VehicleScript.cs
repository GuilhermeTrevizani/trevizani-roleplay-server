using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TrevizaniRoleplay.Core.Extesions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class VehicleScript : Script
{
    [Command("vestacionar")]
    public async Task CMD_vestacionar(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (vehicle.VehicleDB.CharacterId == player.Character.Id
            || player.Items.Any(x => x.GetCategory() == ItemCategory.VehicleKey && x.Subtype == vehicle.VehicleDB.LockNumber))
        {
            if (vehicle.VehicleDB.Items!.Any(x => (x.GetCategory() == ItemCategory.Weapon && Functions.IsWeaponWithAmmo(x.GetItemType()))
                || x.GetCategory() == ItemCategory.WeaponComponent
                || Functions.CheckIfIsAmmo(x.GetCategory())
                || x.GetCategory() == ItemCategory.Drug))
            {
                player.SendMessage(MessageType.Error, "Você não pode estacionar o veículo com armas, componentes de armas, munições ou drogas.");
                return;
            }

            vehicle.VehicleDB.ChangePosition(vehicle.GetPosition().X, vehicle.GetPosition().Y, vehicle.GetPosition().Z,
                vehicle.Rotation.X, vehicle.Rotation.Y, vehicle.Rotation.Z, vehicle.GetDimension());
            await vehicle.Park(player);
            player.SendMessage(MessageType.Success, $"Você estacionou {vehicle.Identifier}.");
            return;
        }

        if (vehicle.VehicleDB.FactionId == player.Character.FactionId && vehicle.VehicleDB.FactionId.HasValue)
        {
            if (player.Faction?.HasDuty ?? false)
                vehicle.SetFuel(vehicle.VehicleDB.GetMaxFuel());
            await vehicle.Park(player);
            player.SendMessage(MessageType.Success, $"Você estacionou {vehicle.Identifier}.");
            return;
        }

        if (vehicle.NameInCharge == player.Character.Name)
        {
            var job = Global.Jobs.FirstOrDefault(x => x.CharacterJob == player.Character.Job)!;
            if (vehicle.GetPosition().DistanceTo(new(job.VehicleRentPosX, job.VehicleRentPosY, job.VehicleRentPosZ)) > Constants.RP_DISTANCE)
            {
                player.SendMessage(MessageType.Error, "Você não está no aluguel de veículos de seu emprego.");
                return;
            }

            await vehicle.Park(player);
            player.SendMessage(MessageType.Success, $"Você estacionou {vehicle.Identifier}.");
            return;
        }

        player.SendMessage(MessageType.Error, Globalization.VEHICLE_ACCESS_ERROR_MESSAGE);
    }

    [Command("vlista")]
    public async Task CMD_vlista(MyPlayer player)
    {
        var context = Functions.GetDatabaseContext();
        var spawnedVehicles = Global.Vehicles.ToList();
        var vehicles = (await context.Vehicles.Where(x => x.CharacterId == player.Character.Id && !x.Sold)
            .ToListAsync())
            .OrderByDescending(x => Convert.ToInt32(spawnedVehicles.Any(y => y.VehicleDB.Id == x.Id)))
            .ThenBy(x => x.Model)
            .Select(x => new
            {
                x.Id,
                Model = x.Model.ToUpper(),
                x.Plate,
                SessionId = spawnedVehicles.FirstOrDefault(y => y.VehicleDB.Id == x.Id)?.Id,
                x.SeizedValue,
                x.SeizedDismantling,
                x.SeizedDate,
                x.ProtectionLevel,
                player.User.PlateChanges,
                SellPrice = Functions.GetVehicleSellPrice(x.Model)?.Item1 ?? 0,
                x.XMR,
                Insurance = x.GetInsuranceInfo(),
            })
            .ToList();

        player.Emit("Server:SpawnarVeiculos", $"Veículos de {player.Character.Name} ({DateTime.Now})", Functions.Serialize(vehicles));
    }

    [Command("vvender", "/vvender (ID ou nome) (valor)")]
    public static void CMD_vvender(MyPlayer player, string idOrName, int valor)
    {
        var vehicle = Global.Vehicles
            .Where(x => x.VehicleDB.CharacterId == player.Character.Id
                && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
                && x.GetDimension() == player.GetDimension())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo seu.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        if (valor <= 0)
        {
            player.SendMessage(MessageType.Error, "Valor não é válido.");
            return;
        }

        var restriction = Functions.CheckVIPVehicle(vehicle.VehicleDB.Model);
        if (restriction != UserPremium.None && restriction > target.GetCurrentPremium())
        {
            player.SendMessage(MessageType.Error, $"O veículo é restrito para VIP {restriction}.");
            return;
        }

        var invite = new Invite
        {
            Type = InviteType.VehicleSell,
            SenderCharacterId = player.Character.Id,
            Value = [vehicle.VehicleDB.Id.ToString(), valor.ToString()],
        };
        target.Invites.RemoveAll(x => x.Type == InviteType.VehicleSell);
        target.Invites.Add(invite);

        player.SendMessage(MessageType.Success, $"Você ofereceu seu veículo {vehicle.VehicleDB.Model.ToUpper()} {vehicle.VehicleDB.Plate} para {target.ICName} por ${valor:N0}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} ofereceu para você o veículo {vehicle.VehicleDB.Model.ToUpper()} {vehicle.VehicleDB.Plate} por ${valor:N0}. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
    }

    [Command("vtransferir", "/vtransferir (ID ou nome)")]
    public static void CMD_vtransferir(MyPlayer player, string idOrName)
    {
        var vehicle = Global.Vehicles
            .Where(x => x.VehicleDB.CharacterId == player.Character.Id
                && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
                && x.GetDimension() == player.GetDimension())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo seu.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        var restriction = Functions.CheckVIPVehicle(vehicle.VehicleDB.Model);
        if (restriction != UserPremium.None && restriction > target.GetCurrentPremium())
        {
            player.SendMessage(MessageType.Error, $"O veículo é restrito para VIP {restriction}.");
            return;
        }

        var invite = new Invite
        {
            Type = InviteType.VehicleTransfer,
            SenderCharacterId = player.Character.Id,
            Value = [vehicle.VehicleDB.Id.ToString()],
        };
        target.Invites.RemoveAll(x => x.Type == InviteType.VehicleTransfer);
        target.Invites.Add(invite);

        player.SendMessage(MessageType.Success, $"Você ofereceu seu veículo {vehicle.VehicleDB.Model.ToUpper()} {vehicle.VehicleDB.Plate} para {target.ICName}.");
        target.SendMessage(MessageType.Success, $"{player.ICName} ofereceu para você o veículo {vehicle.VehicleDB.Model.ToUpper()} {vehicle.VehicleDB.Plate}. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
    }

    [RemoteEvent(nameof(ReleaseVehicle))]
    public async Task ReleaseVehicle(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!Global.Spots.Any(x => x.Type == SpotType.VehicleRelease
                && x.Dimension == player.GetDimension()
                && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE))
            {
                player.SendNotification(NotificationType.Error, "Você não está em ponto de liberação de veículos apreendidos.");
                return;
            }

            var id = idString.ToGuid();
            if (Global.Vehicles.Any(x => x.VehicleDB.Id == id))
            {
                player.SendNotification(NotificationType.Error, "Veículo está spawnado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.CharacterId == player.Character.Id && x.Id == id);
            if (vehicle is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.VEHICLE_OWNER_ERROR_MESSAGE);
                return;
            }

            if (vehicle.SeizedValue == 0)
            {
                player.SendNotification(NotificationType.Error, "Veículo não está apreendido.");
                return;
            }

            if (player.Money < vehicle.SeizedValue)
            {
                player.SendNotification(NotificationType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, vehicle.SeizedValue));
                return;
            }

            await player.RemoveMoney(vehicle.SeizedValue);

            var seizedVehicle = await context.SeizedVehicles.Where(x => x.VehicleId == vehicle.Id).OrderByDescending(x => x.RegisterDate).FirstOrDefaultAsync();
            if (seizedVehicle is not null)
            {
                seizedVehicle.Pay();
                context.SeizedVehicles.Update(seizedVehicle);
            }

            player.SendNotification(NotificationType.Success, $"Você liberou seu veículo por ${vehicle.SeizedValue:N0}.");
            vehicle.ResetSeized();
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync();

            await CMD_vlista(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("vporta", "/vporta (porta [1-4])", Aliases = ["vp"])]
    public static void CMD_vporta(MyPlayer player, int porta)
    {
        if (porta < 1 || porta > 4)
        {
            player.SendMessage(MessageType.Error, "Porta deve ser entre 1 e 4.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x => player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo destrancado.");
            return;
        }

        porta--;
        vehicle.DoorsStates[porta] = !vehicle.DoorsStates[porta];
        (vehicle.Controller ?? player).TriggerEvent("SetVehicleDoorState", vehicle, porta, vehicle.DoorsStates[porta]);
    }

    [Command("capo")]
    public static void CMD_capo(MyPlayer player)
    {
        var vehicle = Global.Vehicles.Where(x => player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle == null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo destrancado.");
            return;
        }

        vehicle.DoorsStates[4] = !vehicle.DoorsStates[4];
        player.SendMessageToNearbyPlayers($"{(vehicle.DoorsStates[4] ? "fecha" : "abre")} o capô do veículo.", MessageCategory.Ame);
        (vehicle.Controller ?? player).TriggerEvent("SetVehicleDoorState", vehicle, 4, vehicle.DoorsStates[4]);
    }

    [Command("portamalas", "/portamalas (opção [abrir, fechar, ver])")]
    public static async Task CMD_portamalas(MyPlayer player, string option)
    {
        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso estando dentro do veículo.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x => x.GetDimension() == player.GetDimension()
            && player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null || !vehicle.HasStorage)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um veículo que possui armazenamento.");
            return;
        }

        if (vehicle.SpawnType != MyVehicleSpawnType.Normal)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso nesse veículo.");
            return;
        }

        option = option.ToLower();
        if (option == "abrir")
        {
            if (vehicle.GetLocked() && !(player.Faction?.Type == FactionType.Police && player.OnDuty))
            {
                player.SendMessage(MessageType.Error, "O veículo está trancado.");
                return;
            }

            if (!vehicle.DoorsStates[5])
            {
                player.SendMessage(MessageType.Error, "O porta-malas está aberto.");
                return;
            }

            vehicle.DoorsStates[5] = false;
            player.SendMessageToNearbyPlayers($"abre o porta-malas do veículo.", MessageCategory.Ame);
            (vehicle.Controller ?? player).TriggerEvent("SetVehicleDoorState", vehicle, 5, vehicle.DoorsStates[5]);
            await player.WriteLog(LogType.General, $"/portamalas abrir {vehicle.Identifier}", null);
        }
        else if (option == "fechar")
        {
            if (vehicle.DoorsStates[5])
            {
                player.SendMessage(MessageType.Error, "O porta-malas está fechado.");
                return;
            }

            vehicle.DoorsStates[5] = true;
            player.SendMessageToNearbyPlayers("fecha o porta-malas do veículo.", MessageCategory.Ame);
            (vehicle.Controller ?? player).TriggerEvent("SetVehicleDoorState", vehicle, 5, vehicle.DoorsStates[5]);
            await player.WriteLog(LogType.General, $"/portamalas fechar {vehicle.Identifier}", null);
        }
        else if (option == "ver")
        {
            if (vehicle.DoorsStates[5])
            {
                player.SendMessage(MessageType.Error, "O porta-malas está fechado.");
                return;
            }

            if (vehicle.VehicleDB.FactionId.HasValue && vehicle.VehicleDB.FactionId != player.Character.FactionId)
            {
                player.SendMessage(MessageType.Error, Globalization.VEHICLE_ACCESS_ERROR_MESSAGE);
                return;
            }

            vehicle.ShowInventory(player, false);
            await player.WriteLog(LogType.General, $"/portamalas ver {vehicle.Identifier}", null);
        }
        else
        {
            player.SendMessage(MessageType.Error, "Opção inválida. Opções disponíveis: abrir, fechar, ver");
        }
    }

    [Command("abastecer", "/abastecer (porcentagem)")]
    public static void CMD_abastecer(MyPlayer player, int percentage)
    {
        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você deve estar fora do veículo.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x => player.GetPosition().DistanceTo(x.GetPosition()) <= 5
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo destrancado.");
            return;
        }

        if (vehicle.VehicleDB.Fuel == vehicle.VehicleDB.GetMaxFuel())
        {
            player.SendMessage(MessageType.Error, "Veículo está com tanque cheio.");
            return;
        }

        player.Emit("RefuelVehicle", vehicle.Id, percentage);
    }

    [RemoteEvent(nameof(ChangeVehiclePlate))]
    public async Task ChangeVehiclePlate(Player playerParam, string idString, string plate)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (plate.Length != 4 && plate.Length != 6 && plate.Length != 8)
            {
                player.SendNotification(NotificationType.Error, "Placa deve ter entre 4, 6 ou 8 caracteres.");
                return;
            }

            if (player.User.PlateChanges == 0)
            {
                player.SendNotification(NotificationType.Error, "Você não possui uma mudança de placa.");
                return;
            }

            var id = idString.ToGuid();
            if (Global.Vehicles.Any(x => x.VehicleDB.Id == id))
            {
                player.SendNotification(NotificationType.Error, $"Veículo {id} está spawnado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.CharacterId == player.Character.Id && x.Id == id);
            if (vehicle is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.VEHICLE_OWNER_ERROR_MESSAGE);
                return;
            }

            if (await context.Vehicles.AnyAsync(x => x.Plate.ToUpper() == plate.ToUpper() || x.NewPlate.ToUpper() == plate.ToUpper()))
            {
                player.SendNotification(NotificationType.Error, $"Já existe um veículo com a placa {plate.ToUpper()}.");
                return;
            }

            vehicle.SetNewPlate(plate.ToUpper());
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync();

            await Functions.SendServerMessage($"{player.User.Name} solicitou alterar a placa do veículo {vehicle.Model.ToUpper()} para {plate.ToUpper()}. Use /aprovarplaca {plate.ToUpper()} ou /reprovarplaca {plate.ToUpper()}.", UserStaff.SeniorServerAdmin, false);
            player.SendNotification(NotificationType.Success, $"Você solicitou alterar a placa do veículo {vehicle.Model.ToUpper()} para {plate.ToUpper()}.");
            await CMD_vlista(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SellVehicle))]
    public async Task SellVehicle(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            if (Global.Vehicles.Any(x => x.VehicleDB.Id == id))
            {
                player.SendNotification(NotificationType.Error, $"Veículo {id} está spawnado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var veh = await context.Vehicles.FirstOrDefaultAsync(x => x.CharacterId == player.Character.Id && x.Id == id && !x.Sold);
            if (veh is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.VEHICLE_OWNER_ERROR_MESSAGE);
                return;
            }

            var price = Functions.GetVehicleSellPrice(veh.Model);
            if (price is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.VEHICLE_PRICE_NOT_SET);
                return;
            }

            var dealership = Global.Dealerships.FirstOrDefault(x => x.Id == price.Value.Item2)!;
            if (player.GetPosition().DistanceTo(new(dealership.PosX, dealership.PosY, dealership.PosZ)) > Constants.RP_DISTANCE)
            {
                player.SendNotification(NotificationType.Error, "Você não está na concessionária que vende este veículo. Posição foi marcada no GPS.");
                player.SetWaypoint(dealership.PosX, dealership.PosY);
                return;
            }

            var res = await player.GiveMoney(price.Value.Item1);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            veh.SetSold();
            context.Vehicles.Update(veh);
            await context.SaveChangesAsync();

            player.SendNotification(NotificationType.Success, $"Você vendeu seu veículo {veh.Model.ToUpper()} ({veh.Plate.ToUpper()}) para a concessionária por ${price.Value.Item1:N0}.");
            await player.WriteLog(LogType.Sell, $"Vender Veículo Concessionária {veh.Id} {price.Value.Item1}", null);
            await CMD_vlista(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("danos", "/danos (veículo)")]
    public static void CMD_danos(MyPlayer player, int id)
    {
        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Nenhum veículo encontrado com o ID {id}.");
            return;
        }

        if (player.GetPosition().DistanceTo(vehicle.GetPosition()) > Constants.RP_DISTANCE
            || player.GetDimension() != vehicle.GetDimension()
            || vehicle.Damages.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Veículo não está próximo ou não possui danos.");
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
                })), false);
    }

    [Command("velmax", "/velmax (velocidade)")]
    public static void CMD_velmax(MyPlayer player, int velocidade)
    {
        if (velocidade < 5 && velocidade != 0)
        {
            player.SendMessage(MessageType.Error, "Velocidade precisa ser maior que 5 ou igual a 0.");
            return;
        }

        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (!veh.CanAccess(player))
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_ACCESS_ERROR_MESSAGE);
            return;
        }

        if (veh.Speed > velocidade && velocidade != 0)
        {
            player.SendMessage(MessageType.Error, "A velocidade do veículo está acima da velocidade máxima pretendida.");
            return;
        }

        player.Emit("SetVehicleMaxSpeed", velocidade / 3.6F);
        player.SendMessage(MessageType.Success, velocidade == 0 ? "Você removeu a limitação de velocidade do veículo." :
            $"Você alterou a velocidade máxima do veículo para {velocidade} km/h.");
    }

    [Command("janela", "/janela (opção [fe, fd, te, td, todas])", Aliases = ["janelas", "ja"], AllowEmptyStrings = true)]
    public static void CMD_janela(MyPlayer player, string? janela)
    {
        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, Globalization.NOT_INSIDE_VEHICLE_MESSAGE);
            return;
        }

        if (!vehicle.HasWindows)
        {
            player.SendMessage(MessageType.Error, "Veículo não possui janelas.");
            return;
        }

        if (string.IsNullOrWhiteSpace(janela))
        {
            janela = player.VehicleSeat switch
            {
                Constants.VEHICLE_SEAT_PASSENGER_FRONT_RIGHT => "fd",
                Constants.VEHICLE_SEAT_PASSENGER_BACK_LEFT => "te",
                Constants.VEHICLE_SEAT_PASSENGER_BACK_RIGHT => "td",
                _ => "fe",
            };
        }

        string text;
        byte idJanela;

        switch (janela.ToLower())
        {
            case "todas":
                if (vehicle.Driver != player)
                {
                    player.SendMessage(MessageType.Error, "Você não é o motorista do veículo.");
                    return;
                }

                text = "todas as janelas";
                idJanela = 255;
                break;
            case "fe":
                if (vehicle.Driver != player)
                {
                    player.SendMessage(MessageType.Error, "Você não é o motorista do veículo.");
                    return;
                }

                text = "a janela frontal esquerda";
                idJanela = 0;
                break;
            case "fd":
                if (vehicle.Driver != player && player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_FRONT_RIGHT)
                {
                    player.SendMessage(MessageType.Error, "Você não é o motorista do veículo ou não está no banco dianteiro direito.");
                    return;
                }

                text = "a janela frontal direita";
                idJanela = 1;
                break;
            case "te":
                if (vehicle.Driver != player && player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_BACK_LEFT)
                {
                    player.SendMessage(MessageType.Error, "Você não é o motorista do veículo ou não está no banco traseiro esquerdo.");
                    return;
                }

                text = "a janela traseira esquerda";
                idJanela = 2;
                break;
            case "td":
                if (vehicle.Driver != player && player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_BACK_RIGHT)
                {
                    player.SendMessage(MessageType.Error, "Você não é o motorista do veículo ou não está no banco traseiro direito.");
                    return;
                }

                text = "a janela traseira direita";
                idJanela = 3;
                break;
            default:
                player.SendMessage(MessageType.Error, "O parâmetro informado é inválido. Opções: fe, fd, te, td, todas");
                return;
        }

        var status = !vehicle.IsWindowOpened(idJanela == 255 ? (byte)0 : idJanela);
        if (idJanela == 255)
        {
            vehicle.SetWindowOpened(0, status);
            vehicle.SetWindowOpened(1, status);
            vehicle.SetWindowOpened(2, status);
            vehicle.SetWindowOpened(3, status);
        }
        else
        {
            vehicle.SetWindowOpened(idJanela, status);
        }

        var hasWindowOpened = vehicle.IsWindowOpened(0) || vehicle.IsWindowOpened(1)
            || vehicle.IsWindowOpened(2) || vehicle.IsWindowOpened(3);

        vehicle.SetSharedDataEx(Constants.VEHICLE_META_DATA_HAS_WINDOW_OPENED, hasWindowOpened);

        foreach (var target in vehicle.Occupants)
            (target as MyPlayer)!.SetCanDoDriveBy((target as MyPlayer)!.VehicleSeat, target == player || idJanela == 255 ? status : null);

        player.SendMessageToNearbyPlayers($"{(status ? "abaixa" : "levanta")} {text} do veículo.", MessageCategory.Ame);
    }

    [Command("vfechadura")]
    public async Task CMD_vfechadura(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (veh.VehicleDB.CharacterId != player.Character.Id)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_OWNER_ERROR_MESSAGE);
            return;
        }

        if (player.Money < Global.Parameter.LockValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, Global.Parameter.LockValue));
            return;
        }

        var context = Functions.GetDatabaseContext();
        veh.VehicleDB.SetLockNumber(await context.Vehicles.MaxAsync(x => x.LockNumber) + 1);
        context.Vehicles.Update(veh.VehicleDB);
        await context.SaveChangesAsync();

        await player.RemoveMoney(Global.Parameter.LockValue);

        player.SendMessage(MessageType.Success, $"Você trocou a fechadura do veículo {veh.Identifier} por ${Global.Parameter.LockValue:N0}.");
    }

    [Command("vchave")]
    public async Task CMD_vchave(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (veh.VehicleDB.CharacterId != player.Character.Id)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_OWNER_ERROR_MESSAGE);
            return;
        }

        if (player.Money < Global.Parameter.KeyValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, Global.Parameter.KeyValue));
            return;
        }

        var characterItem = new CharacterItem();
        characterItem.Create(new Guid(Constants.VEHICLE_KEY_ITEM_TEMPLATE_ID), veh.VehicleDB.LockNumber, 1, null);
        var res = await player.GiveItem(characterItem);
        if (!string.IsNullOrWhiteSpace(res))
        {
            player.SendMessage(MessageType.Error, res);
            return;
        }

        await player.RemoveMoney(Global.Parameter.KeyValue);

        player.SendMessage(MessageType.Success, $"Você criou uma cópia da chave para o veículo {veh.Identifier} por ${Global.Parameter.KeyValue:N0}.");
    }

    [RemoteEvent(nameof(ActiveVehicleDriftMode))]
    public static void ActiveVehicleDriftMode(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player || !vehicle.VehicleDB.Drift)
                return;

            vehicle.SetSharedDataEx("DriftMode", true);
            player.Emit("HUDPage:UpdateDriftMode", true);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(DeactiveVehicleDriftMode))]
    public static void DeactiveVehicleDriftMode(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
                return;

            vehicle.SetSharedDataEx("DriftMode", false);
            player.Emit("HUDPage:UpdateDriftMode", false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(TrackVehicle))]
    public static async Task TrackVehicle(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var vehicle = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == id);
            if (vehicle is null)
            {
                player.SendNotification(NotificationType.Error, "Veículo não está spawnado.");
                return;
            }

            if (vehicle.VehicleDB.ProtectionLevel < 1)
            {
                player.SendNotification(NotificationType.Error, "Veículo não possui rastreador.");
                return;
            }

            if (vehicle.Occupants.Count > 0)
            {
                player.SendNotification(NotificationType.Error, "Veículo não pode ser rastreado pois está com ocupantes.");
                return;
            }

            player.Emit(Constants.VEHICLE_LIST_PAGE_CLOSE);
            player.SendMessage(MessageType.Success, "Aguarde 15 segundos. Pressione DELETE para cancelar a ação.");
            player.CancellationTokenSourceAcao?.Cancel();
            player.CancellationTokenSourceAcao = new();
            await Task.Delay(TimeSpan.FromSeconds(15), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                player.SetWaypoint(vehicle.ICPosition.X, vehicle.ICPosition.Y);
                player.SendMessage(MessageType.Success, "A posição do veículo foi marcada no GPS.");
                player.CancellationTokenSourceAcao = null;
            });
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("xmr")]
    public static void CMD_xmr(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, Globalization.NOT_INSIDE_VEHICLE_MESSAGE);
            return;
        }

        if (!vehicle.VehicleDB.XMR)
        {
            player.SendMessage(MessageType.Error, "O veículo não possui XMR.");
            return;
        }

        if (player.VehicleSeat != Constants.VEHICLE_SEAT_DRIVER && player.VehicleSeat != Constants.VEHICLE_SEAT_PASSENGER_FRONT_RIGHT)
        {
            player.SendMessage(MessageType.Error, "Você não está nos bancos dianteiros do veículo.");
            return;
        }

        player.Emit("XMR", vehicle.AudioSpot?.Source ?? string.Empty, vehicle.AudioSpot?.Volume ?? 1,
            player.GetCurrentPremium() != UserPremium.None, Functions.Serialize(Global.AudioRadioStations.OrderBy(x => x.Name)));
    }

    [Command("quebrartrava", Aliases = ["picklock"])]
    public static async Task CMD_quebrartrava(MyPlayer player)
    {
        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você deve estar fora do veículo.");
            return;
        }

        if (!player.Items.Any(x => x.GetCategory() == ItemCategory.PickLock))
        {
            player.SendMessage(MessageType.Error, "Você não possui uma gazua.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x =>
            player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked()
            && x.VehicleDB.CharacterId.HasValue)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um veículo trancado.");
            return;
        }

        var pins = vehicle.VehicleDB.ProtectionLevel switch
        {
            1 => 4,
            2 => 6,
            3 => 9,
            _ => 3,
        };

        var difficulty = 2;
        var attempts = 1;

        player.Emit("PickLock:Start", difficulty, pins, attempts, "Vehicle");
        await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) começou a quebrar a trava do veículo {vehicle.Identifier}.", UserStaff.JuniorServerAdmin, false);
    }

    [RemoteEvent(nameof(FinishPickLock))]
    public async Task FinishPickLock(Player playerParam, bool success, string type)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.IsInVehicle)
            {
                player.SendMessage(MessageType.Error, "Você deve estar fora do veículo.");
                return;
            }

            var item = player.Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.PickLock);
            if (item is null)
            {
                player.SendMessage(MessageType.Error, "Você não possui uma gazua.");
                return;
            }

            if (type == "Vehicle")
            {
                var vehicle = Global.Vehicles
                    .Where(x => player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
                        && x.GetDimension() == player.GetDimension()
                        && !x.GetLocked()
                        && x.VehicleDB.CharacterId.HasValue)
                    .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
                if (vehicle is null)
                {
                    player.SendMessage(MessageType.Error, "Você não está próximo de um veículo trancado.");
                    return;
                }

                await vehicle.ActivateProtection(player);

                if (success)
                {
                    vehicle.SetLocked(false);
                    player.SendMessageToNearbyPlayers("quebra a trava do veículo.", MessageCategory.Ame);
                }
                else
                {
                    await player.RemoveStackedItem(item.ItemTemplateId, 1);
                    player.SendMessageToNearbyPlayers("quebra a gazua.", MessageCategory.Ame);
                }

                await player.WriteLog(LogType.LockBreak, $"{vehicle.VehicleDB.Id} {(success ? "SUCESSO" : "FALHA")}", null);
            }
            else if (type == "Property")
            {
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
                    player.SendMessage(MessageType.Error, Globalization.ROBBED_PROPERTY_ERROR_MESSAGE);
                    return;
                }

                await property.ActivateProtection();

                if (success)
                {
                    property.SetLocked(false);
                    var context = Functions.GetDatabaseContext();
                    context.Properties.Update(property);
                    await context.SaveChangesAsync();
                    player.SendMessageToNearbyPlayers("arromba a porta.", MessageCategory.Ame);
                }
                else
                {
                    await player.RemoveStackedItem(item.ItemTemplateId, 1);
                    player.SendMessageToNearbyPlayers("quebra a gazua.", MessageCategory.Ame);
                }

                await player.WriteLog(LogType.BreakIn, $"{property.FormatedAddress} ({property.Number}) {(success ? "SUCESSO" : "FALHA")}", null);
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("reparar")]
    public async Task CMD_reparar(MyPlayer player)
    {
        if (Global.Companies.Any(x => x.GetIsOpen() && x.Type == CompanyType.MechanicWorkshop))
        {
            player.SendMessage(MessageType.Error, "Não é possível reparar pois há ao menos uma oficina mecânica aberta.");
            return;
        }

        var price = Global.Companies
            .SelectMany(x => x.TuningPrices!)
            .OrderByDescending(x => x.SellPercentagePrice)
            .FirstOrDefault(x => x.Type == CompanyTuningPriceType.Repair);
        if (price is null)
        {
            player.SendMessage(MessageType.Error, "Não foi configurado o preço de reparo em nenhuma oficina mecânica. Por favor, reporte o bug.");
            return;
        }

        if (player.Vehicle is not MyVehicle veh || veh.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        var job = Global.Jobs.FirstOrDefault(x => x.CharacterJob == CharacterJob.Mechanic)!;
        if (veh.GetPosition().DistanceTo(new(job.PosX, job.PosY, job.PosZ)) > 5)
        {
            player.SendMessage(MessageType.Error, "Você não está na Central de Mecânicos.");
            return;
        }

        var vehiclePrice = Functions.GetVehiclePrice(veh.VehicleDB.Model);
        if (vehiclePrice is null)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_PRICE_NOT_SET);
            return;
        }

        var value = Convert.ToInt32(Math.Abs(vehiclePrice.Value.Item1 * (price.SellPercentagePrice / 100f) * 2.5f));
        if (value <= 0)
        {
            player.SendMessage(MessageType.Error, $"Não foi configurado corretamente o preço de reparo (${value:N0}). Por favor, reporte o bug.");
            return;
        }

        if (player.Money < value)
        {
            player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, value));
            return;
        }

        player.ToggleGameControls(false);
        player.SendMessage(MessageType.Success, $"Você irá reparar seu veículo por ${value:N0}. Aguarde 30 segundos. Pressione DELETE para cancelar a ação.");
        player.CancellationTokenSourceAcao?.Cancel();
        player.CancellationTokenSourceAcao = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(30), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            Task.Run(async () =>
            {
                veh.RepairEx();
                await player.RemoveMoney(value);
                player.SendMessage(MessageType.Success, $"Você consertou o veículo e pagou ${value:N0}.");
                player.ToggleGameControls(true);

                await player.WriteLog(LogType.PlayerVehicleRepair, veh.VehicleDB.Id.ToString(), null);
                player.CancellationTokenSourceAcao = null;
            });
        });
    }

    [Command("ligacaodireta", Aliases = ["hotwire"])]
    public static async Task CMD_ligacaodireta(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (vehicle.VehicleDB.CharacterId is null)
        {
            player.SendMessage(MessageType.Error, "Veículo não pertence a um jogador.");
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

        if (player.HotwireWords.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você já está fazendo ligação direta em um veículo.");
            return;
        }

        player.HotwireWords = vehicle.VehicleDB.ProtectionLevel switch
        {
            1 => 4,
            2 => 6,
            3 => 8,
            _ => 2,
        };
        player.HotwireVehicle = vehicle;

        await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) começou a fazer ligação direta no veículo {vehicle.Identifier}.", UserStaff.JuniorServerAdmin, false);
        await GenerateHotwireWord(player);
    }

    private static async Task GenerateHotwireWord(MyPlayer player)
    {
        var random = new Random().Next(0, Global.WordsToShuffle.Count);
        player.HotwireWord = Global.WordsToShuffle[random];

        player.SendMessage(MessageType.Error, $"Use /desem para desembaralhar a seguinte palavra: {ShuffleWord(player.HotwireWord)}. Você tem 15 segundos antes de ser eletrocutado.");

        player.HotwireCancellationTokenSource?.Cancel();
        player.HotwireCancellationTokenSource = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(15), player.HotwireCancellationTokenSource.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            FailHotwire(player);
        });
    }

    private static string ShuffleWord(string word)
    {
        var random = new Random();
        var caracteres = word.ToCharArray();

        for (int i = caracteres.Length - 1; i > 0; i--)
        {
            var j = random.Next(0, i + 1);
            (caracteres[j], caracteres[i]) = (caracteres[i], caracteres[j]);
        }

        return new string(caracteres);
    }

    private static void FailHotwire(MyPlayer player)
    {
        player.EndHotwire();
        player.Health -= 10;
        player.SendMessageToNearbyPlayers("falhou na ligação direta e foi eletrocutado.", MessageCategory.Ame);
    }

    [Command("desembaralhar", "/desembaralhar (palavra)", Aliases = ["desem"])]
    public async Task CMD_desembaralhar(MyPlayer player, string word)
    {
        if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (!player.HotwireWords.HasValue || vehicle != player.HotwireVehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está fazendo ligação direta em um veículo.");
            return;
        }

        if (word.ToLower() != player.HotwireWord!.ToLower())
        {
            FailHotwire(player);
            return;
        }

        player.HotwireWords--;
        if (player.HotwireWords > 0)
        {
            await GenerateHotwireWord(player);
            return;
        }

        vehicle.SetEngineStatus(true);
        player.EndHotwire();
        player.SendMessageToNearbyPlayers("faz uma ligação direta no veículo.", MessageCategory.Ame);
        await player.WriteLog(LogType.HotWire, vehicle.Identifier, null);
    }

    [Command("desmanchar")]
    public async Task CMD_desmanchar(MyPlayer player)
    {
        if (!Global.Spots.Any(x => x.Type == SpotType.VehicleDismantling
            && x.Dimension == player.GetDimension()
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE))
        {
            player.SendNotification(NotificationType.Error, "Você não está próximo de nenhum ponto de desmanche de veículos.");
            return;
        }

        if ((player.User.CooldownDismantle ?? DateTime.MinValue) > DateTime.Now)
        {
            player.SendMessage(MessageType.Error, $"Aguarde o cooldown para desmanchar novamente. Será liberado em {player.User.CooldownDismantle}.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x =>
            player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked()
            && x.VehicleDB.CharacterId.HasValue)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo destrancado.");
            return;
        }

        var price = Functions.GetVehiclePrice(vehicle.VehicleDB.Model);
        if (price is null)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_PRICE_NOT_SET);
            return;
        }

        if (Global.SpawnedPlayers.Any(x => x.DismantlingVehicleId == vehicle.Id))
        {
            player.SendMessage(MessageType.Error, "Este veículo já está sendo desmanchado.");
            return;
        }

        var vehicleParts = 0;
        var randomNumber = new Random().Next(0, 100);
        var sum = 0;
        foreach (var chance in Global.VehicleDismantlingPartsChances.OrderByDescending(x => x.Percentage))
        {
            sum += chance.Percentage;
            if (randomNumber < sum)
            {
                vehicleParts = chance.Quantity;
                break;
            }
        }

        if (vehicleParts == 0)
        {
            player.SendMessage(MessageType.Error, "Probabilidades de peças de veículo em um desmanche não configuradas. Por favor, reporte o bug.");
            return;
        }

        player.DismantlingVehicleId = vehicle.Id;
        player.ToggleGameControls(false);
        player.SendMessage(MessageType.Success, $"Aguarde {Global.Parameter.VehicleDismantlingMinutes} minutos. Pressione DELETE para cancelar a ação.");
        player.CancellationTokenSourceAcao?.Cancel();
        player.CancellationTokenSourceAcao = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromMinutes(Global.Parameter.VehicleDismantlingMinutes), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            Task.Run(async () =>
            {
                var characterItem = new CharacterItem();
                characterItem.Create(new Guid(Constants.VEHICLE_PART_ITEM_TEMPLATE_ID), 0, vehicleParts, null);
                var res = await player.GiveItem(characterItem);
                player.DismantlingVehicleId = null;
                if (!string.IsNullOrWhiteSpace(res))
                {
                    player.SendMessage(MessageType.Error, res);
                    return;
                }

                var spot = Global.Spots.FirstOrDefault(x => x.Type == SpotType.VehicleSeizure);
                if (spot is not null)
                    vehicle.VehicleDB.ChangePosition(spot.PosX, spot.PosY, spot.PosZ, 0, 0, 0, spot.Dimension);

                var value = Convert.ToInt32(Math.Truncate(price.Value.Item1 * (Global.Parameter.VehicleDismantlingPercentageValue / 100)));
                vehicle.VehicleDB.SetSeized(value, true, DateTime.Now.AddDays(Global.Parameter.VehicleDismantlingSeizedDays));
                await vehicle.Park(null);

                player.User.SetCooldownDismantle(DateTime.Now.AddHours(Global.Parameter.CooldownDismantleHours));
                player.ToggleGameControls(true);
                player.SendMessageToNearbyPlayers("desmancha o veículo.", MessageCategory.Ame);
                player.SendMessage(MessageType.Success, $"Você desmanchou o veículo e recebeu {characterItem.Quantity}x {characterItem.GetName()}.");
                await player.WriteLog(LogType.Dismantling, $"{vehicle.VehicleDB.Id} {value} {characterItem.Quantity}", null);
                player.CancellationTokenSourceAcao = null;
            });
        });
    }

    [Command("rebocar", "/rebocar (veículo)")]
    public static void CMD_rebocar(MyPlayer player, int id)
    {
        if (player.Vehicle is not MyVehicle vehicle
            || vehicle.VehicleDB.Model.ToUpper() != VehicleModel.Flatbed.ToString().ToUpper()
            || vehicle.Driver != player)
        {
            player.SendMessage(MessageType.Error, "Você não está dirigindo um FLATBED.");
            return;
        }

        if (vehicle.Attached is not null)
        {
            player.SendMessage(MessageType.Error, "Você já está rebocando um veículo.");
            return;
        }

        var attachedVehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id
            && x != player.Vehicle
            && x.GetPosition().DistanceTo(vehicle.GetPosition()) <= 15);
        if (attachedVehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Você não está próximo do veículo {id}.");
            return;
        }

        vehicle.Attached = attachedVehicle;
        attachedVehicle.AttachToVehicle(vehicle, 0, new(0, -2.5f, 1.1f), new(0, 0, 0));
        player.SendMessage(MessageType.Success, $"Você rebocou o veículo {attachedVehicle.Identifier}.");
    }

    [Command("rebocaroff")]
    public static void CMD_rebocaroff(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo.");
            return;
        }

        if (vehicle.Attached is not MyVehicle attachedVehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está rebocando nenhum veículo.");
            return;
        }

        attachedVehicle.AttachToVehicle(vehicle, 0, new(0, -10f, 0), new(0, 0, 0));
        attachedVehicle.Detach();
        vehicle.Attached = null;

        player.SendMessage(MessageType.Success, $"Você soltou o veículo {attachedVehicle.Identifier}.");
    }

    [ServerEvent(Event.PlayerExitVehicle)]
    public static void OnPlayerLeaveVehicle(Player playerParam, GTANetworkAPI.Vehicle vehicleParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var vehicle = Functions.CastVehicle(vehicleParam);
            if (player.VehicleAnimation)
                player.StopAnimationEx();

            player.Emit("Spotlight:Toggle", false);

            if (vehicle.VehicleDB.Model.ToLower() == VehicleModelMods.AS332.ToString().ToLower()
                || vehicle.VehicleDB.Model.ToLower() == VehicleModel.Polmav.ToString().ToLower())
            {
                player.Emit("Helicam:Toggle", true);
                var spotlight = Global.Spotlights.FirstOrDefault(x => x.Id == vehicle.Id && x.Player == player.SessionId);
                if (spotlight is not null)
                {
                    Global.Spotlights.Remove(spotlight);
                    NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Spotlight:Remove", spotlight.Id);
                }
            }

            player.EndHotwire();
            player.SeatBelt = false;

            if (vehicle.SpawnType == MyVehicleSpawnType.TestDrive)
            {
                player.EndTestDrive();
                vehicle.Delete();
                return;
            }

            if (vehicle.SpawnType == MyVehicleSpawnType.Rent
                && player.VehicleSeat == Constants.VEHICLE_SEAT_DRIVER
                && !vehicle.RentExpirationDate.HasValue)
            {
                vehicle.Delete();
                return;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [ServerEvent(Event.PlayerEnterVehicle)]
    public static void OnPlayerEnterVehicle(Player playerParam, GTANetworkAPI.Vehicle vehicleParam, sbyte seat)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var vehicle = Functions.CastVehicle(vehicleParam);
            if (vehicle.VehicleDB.CharacterId == player.Character.Id
                && (vehicle.VehicleDB.InsuranceDate ?? DateTime.MinValue) <= DateTime.Now)
                player.SendMessage(MessageType.Error, "Seu veículo está sem seguro. Ligue para o número 205.");

            player.SetCurrentWeapon((uint)WeaponModel.Fist);

            if (seat == Constants.VEHICLE_SEAT_DRIVER)
                player.Emit("Spotlight:Toggle", vehicle.SpotlightActive);

            player.SetCanDoDriveBy(seat);
            vehicle.SetSharedDataEx(Constants.VEHICLE_META_DATA_RADIO_ENABLED, !vehicle.VehicleDB.FactionId.HasValue);

            if (vehicle.GetEngineStatus())
            {
                if (!string.IsNullOrWhiteSpace(vehicle.VehicleDB.Model) && vehicle.VehicleDB.Fuel == 0)
                    vehicle.SetEngineStatus(false);
            }
            else
            {
                if (vehicle.GetClass() == VehicleClass.Cycles)
                    vehicle.SetEngineStatus(true);
            }

            if (vehicle.NameInCharge == player.Character.Name && vehicle.RentExpirationDate.HasValue)
                player.SendMessage(MessageType.Error, $"O aluguel do veículo irá expirar {vehicle.RentExpirationDate}.");

            if (!player.OnAdminDuty
                && seat == Constants.VEHICLE_SEAT_DRIVER
                && vehicle.VehicleDB.FactionId.HasValue
                && vehicle.VehicleDB.FactionId != player.Character.FactionId)
            {
                player.SendMessage(MessageType.Error, "Você não possui acesso ao veículo.");
                player.RemoveFromVehicle();
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(OnVehicleDamage))]
    public static void OnVehicleDamage(Player sourceEntity, int targetEntityRemoteId, string weaponString, int boneIndex, float damage)
    {
        try
        {
            var attacker = Functions.CastPlayer(sourceEntity);
            var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == targetEntityRemoteId);
            if (vehicle is null || damage <= 0)
                return;

            if (damage > 120)
                vehicle.SetEngineStatus(false);

            var vehicleDamage = new VehicleDamage
            {
                BodyHealthDamage = damage,
                EngineHealthDamage = damage,
                Weapon = Functions.GetWeaponName(Convert.ToUInt32(weaponString)),
                Attacker = $"{attacker.Character.Name} ({attacker.Character.Id})",
                Distance = vehicle.GetPosition().DistanceTo(attacker.GetPosition()),
            };

            vehicle.Damages.Add(vehicleDamage);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CloseDealership))]
    public static void CloseDealership(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var dealership = Global.Dealerships.MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
            if (dealership is not null)
                player.SetPosition(new(dealership.PosX, dealership.PosY, dealership.PosZ), player.GetDimension(), false);

            player.SetHour(DateTime.Now.Hour);
            player.SetPosition(player.GetPosition(), 0, false);
            player.Visible = true;
            player.Frozen = false;
            player.Emit(Constants.DEALERSHIP_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(VehicleTestDrive))]
    public async Task VehicleTestDrive(Player playerParam, string model, byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.Frozen = false;
            player.Visible = true;
            player.Invincible = true;
            player.Emit(Constants.DEALERSHIP_PAGE_CLOSE);

            var price = Functions.GetVehiclePrice(model);
            if (price is null)
            {
                player.SetPosition(player.GetPosition(), 0, false);
                player.SendNotification(NotificationType.Error, Globalization.VEHICLE_PRICE_NOT_SET);
                return;
            }

            var dealership = Global.Dealerships.FirstOrDefault(x => x.Id == price.Value.Item2);
            if (dealership is null)
            {
                player.SetPosition(player.GetPosition(), 0, false);
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.TestDriveDealership = dealership;
            var testDriveVehicle = Functions.CreateVehicle(model,
                new(dealership.VehiclePosX, dealership.VehiclePosY, dealership.VehiclePosZ),
                new(dealership.VehicleRotR, dealership.VehicleRotP, dealership.VehicleRotY),
                MyVehicleSpawnType.TestDrive);
            testDriveVehicle.PearlescentColor = 0;
            testDriveVehicle.Dimension = player.ExclusiveDimension;
            testDriveVehicle.CustomPrimaryColor = new Color(r1, g1, b1);
            testDriveVehicle.CustomSecondaryColor = new Color(r2, g2, b2);
            testDriveVehicle.NumberPlate = "TEST";
            player.SetIntoVehicle(testDriveVehicle, Constants.VEHICLE_SEAT_DRIVER);
            testDriveVehicle.EngineStatus = true;
            player.SendMessage(MessageType.Error, "O test drive terminará quando você sair do veículo.");
            await player.WriteLog(LogType.SpawnVehicle, "Test Drive", null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(BuyDealershipVehicle))]
    public async Task BuyDealershipVehicle(Player playerParam, string model, byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var price = Functions.GetVehiclePrice(model);
            if (price is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.VEHICLE_PRICE_NOT_SET);
                return;
            }

            var value = price.Value.Item1;
            if (player.Money < value)
            {
                player.SendNotification(NotificationType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, value));
                return;
            }

            var vip = Functions.CheckVIPVehicle(model);
            if (vip != UserPremium.None && vip > player.GetCurrentPremium())
            {
                player.SendNotification(NotificationType.Error, $"O veículo é restrito para VIP {vip.GetDisplay()}.");
                return;
            }

            var dealership = Global.Dealerships.FirstOrDefault(x => x.Id == price.Value.Item2);
            if (dealership is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var vehicle = new Domain.Entities.Vehicle();
            var context = Functions.GetDatabaseContext();
            vehicle.Create(model, await Functions.GenerateVehiclePlate(false), r1, g1, b1, r2, g2, b2);
            vehicle.ChangePosition(dealership.VehiclePosX, dealership.VehiclePosY, dealership.VehiclePosZ,
                dealership.VehicleRotR, dealership.VehicleRotP, dealership.VehicleRotY, 0);
            vehicle.SetOwner(player.Character.Id);
            vehicle.SetFuel(vehicle.GetMaxFuel());
            vehicle.SetLockNumber((await context.Vehicles.Select(x => x.LockNumber).DefaultIfEmpty().MaxAsync()) + 1);

            await context.Vehicles.AddAsync(vehicle);
            await context.SaveChangesAsync();
            await player.RemoveMoney(value);

            player.SendMessage(MessageType.Success, $"Você comprou {vehicle.Model.ToUpper()} por ${value:N0}. Use /vlista para spawnar.");
            CloseDealership(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SetVehicleHasMutedSirens))]
    public void SetVehicleHasMutedSirens(Player playerParam, bool hasMutedSirens)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var vehicle = Functions.CastVehicle(player.Vehicle);
            vehicle.SetSharedDataEx(Constants.VEHICLE_META_DATA_HAS_MUTED_SIRENS, hasMutedSirens);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SetVehicleSpotlightX))]
    public void SetVehicleSpotlightX(Player playerParam, float spotlightX)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var vehicle = Functions.CastVehicle(player.Vehicle);
            vehicle.SetSharedDataEx(Constants.VEHICLE_META_DATA_SPOTLIGHT_X, spotlightX);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SetVehicleSpotlightZ))]
    public void SetVehicleSpotlightZ(Player playerParam, float spotlightZ)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var vehicle = Functions.CastVehicle(player.Vehicle);
            vehicle.SetSharedDataEx(Constants.VEHICLE_META_DATA_SPOTLIGHT_Z, spotlightZ);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(RefuelVehicle))]
    public async Task RefuelVehicle(Player playerParam, uint id, int percentage)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (percentage <= 0)
            {
                player.SendMessage(MessageType.Error, "Porcentagem inválida.");
                return;
            }

            var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
            if (vehicle is null)
            {
                player.SendMessage(MessageType.Error, "Veículo não encontrado.");
                return;
            }

            var fuelRequired = vehicle.VehicleDB.GetMaxFuel() - vehicle.VehicleDB.Fuel;
            fuelRequired = Math.Min(fuelRequired, percentage);
            var value = fuelRequired * Global.Parameter.FuelValue;
            if ((player.Faction?.HasDuty ?? false) && vehicle.VehicleDB.FactionId == player.Character.FactionId)
                value = 0;

            if (value > player.Money)
            {
                player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, value));
                return;
            }

            var seconds = Math.Max(Convert.ToInt32(Math.Ceiling(fuelRequired / 10f)), 1);
            player.ToggleGameControls(false);
            player.SendMessage(MessageType.Success, $"Aguarde {seconds} segundo{(seconds != 1 ? "s" : string.Empty)}. Pressione DELETE para cancelar a ação.");
            player.CancellationTokenSourceAcao?.Cancel();
            player.CancellationTokenSourceAcao = new CancellationTokenSource();
            await Task.Delay(TimeSpan.FromSeconds(seconds), player.CancellationTokenSourceAcao.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                Task.Run(async () =>
                {
                    vehicle.SetFuel(Math.Min(vehicle.VehicleDB.Fuel + fuelRequired, vehicle.VehicleDB.GetMaxFuel()));
                    if (value == 0)
                    {
                        player.SendMessage(MessageType.Success, $"Você abasteceu seu veículo com {fuelRequired} litro{(fuelRequired > 1 ? "s" : string.Empty)} de combustível e a conta foi paga pelo estado.");
                    }
                    else
                    {
                        await player.RemoveMoney(value);
                        player.SendMessage(MessageType.Success, $"Você abasteceu seu veículo com {fuelRequired} litro{(fuelRequired > 1 ? "s" : string.Empty)} de combustível por ${value:N0}.");
                    }
                    player.SendMessageToNearbyPlayers("abastece o veículo.", MessageCategory.Ame);
                    player.ToggleGameControls(true);
                    player.CancellationTokenSourceAcao = null;
                    await player.WriteLog(LogType.General, $"/abastecer {percentage} {vehicle.Identifier} {value}", null);
                });
            });
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SpawnVehicle))]
    public async Task SpawnVehicle(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            if (Global.Vehicles.Any(x => x.VehicleDB.Id == id))
            {
                player.SendNotification(NotificationType.Error, "Veículo já está spawnado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var veh = await context.Vehicles
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (veh is null)
            {
                player.SendNotification(NotificationType.Error, "Veículo não encontrado.");
                return;
            }

            if (veh.SeizedValue > 0)
            {
                player.SendNotification(NotificationType.Error, "Veículo está apreendido. Vá até o pátio de apreensão para realizar a liberação.");
                return;
            }

            var vehicle = await veh.Spawnar(player);
            player.SetWaypoint(veh.PosX, veh.PosY);
            player.SendNotification(NotificationType.Success, $"Você spawnou {veh.Model.ToUpper()} ({vehicle.Id}).");
            player.Emit(Constants.VEHICLE_LIST_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("vtrancar")]
    public static void CMDvtrancar(MyPlayer player)
    {
        var veh = Global.Vehicles
            .Where(x => x.GetDimension() == player.GetDimension()
                && player.GetPosition().DistanceTo(x.GetPosition()) <= 5)
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (veh is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de um veículo");
            return;
        }

        if (!veh.CanAccess(player) && !player.OnAdminDuty)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_ACCESS_ERROR_MESSAGE);
            return;
        }

        if (veh.GetLocked() && !player.IsInVehicle)
        {
            if (!player.CheckAnimations(true))
                return;
        }

        veh.StopAlarm();
        veh.SetLocked(!veh.GetLocked());
        player.SendMessageToNearbyPlayers($"{(!veh.GetLocked() ? "des" : string.Empty)}tranca o veículo.", MessageCategory.Ame);
    }

    [RemoteEvent(nameof(KeyL))]
    public async Task KeyL(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var context = Functions.GetDatabaseContext();
            var property = Global.Properties
                .Where(x => x.EntranceDimension == player.GetDimension()
                    && player.GetPosition().DistanceTo(x.GetEntrancePosition()) <= Constants.RP_DISTANCE)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetEntrancePosition()));
            if (property is null && player.GetDimension() != 0)
                property = Global.Properties
                    .Where(x => x.Number == player.GetDimension()
                        && player.GetPosition().DistanceTo(x.GetExitPosition()) <= Constants.RP_DISTANCE)
                    .MinBy(x => player.GetPosition().DistanceTo(x.GetExitPosition()));

            if (property is not null)
            {
                if (!property.CanAccess(player) && !player.OnAdminDuty)
                {
                    player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                    return;
                }

                property.StopAlarm();
                property.SetLocked(!property.Locked);
                player.SendMessageToNearbyPlayers($"{(!property.Locked ? "des" : string.Empty)}tranca a porta.", MessageCategory.Ame);

                context.Properties.Update(property);
                await context.SaveChangesAsync();
                return;
            }

            if (player.GetDimension() == 0)
            {
                var companies = player.Companies.Select(x => x.Id);
                var door = Global.Doors
                    .Where(x => (
                        x.FactionId.HasValue && x.FactionId == player.Character.FactionId
                        || x.CompanyId.HasValue && companies.Contains(x.CompanyId.Value)
                        || player.OnAdminDuty
                        )
                        && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= 10)
                    .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
                if (door is not null)
                {
                    door.SetLocked(!door.Locked);
                    player.SendMessageToNearbyPlayers($"{(!door.Locked ? "des" : string.Empty)}tranca a porta.", MessageCategory.Ame);
                    door.SetupAllClients();

                    context.Doors.Update(door);
                    await context.SaveChangesAsync();
                    return;
                }
            }

            var veh = Global.Vehicles
                .Where(x => x.GetDimension() == player.GetDimension()
                    && player.GetPosition().DistanceTo(x.GetPosition()) <= 5)
                .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));

            if (veh is not null)
            {
                if (!veh.CanAccess(player) && !player.OnAdminDuty)
                {
                    player.SendNotification(NotificationType.Error, Globalization.VEHICLE_ACCESS_ERROR_MESSAGE);
                    return;
                }

                if (veh.GetLocked() && !player.IsInVehicle)
                {
                    if (!player.CheckAnimations(true))
                        return;
                }

                veh.StopAlarm();
                veh.SetLocked(!veh.GetLocked());
                player.SendMessageToNearbyPlayers($"{(!veh.GetLocked() ? "des" : string.Empty)}tranca o veículo.", MessageCategory.Ame);
                return;
            }

            if (player.GetDimension() != 0)
            {
                property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension());
                if (property is not null)
                {
                    if (!property.CanAccess(player) && !player.OnAdminDuty)
                    {
                        player.SendNotification(NotificationType.Error, "Você não possui acesso a esta propriedade.");
                        return;
                    }

                    var furniture = property.Furnitures!
                        .Where(x => x.GetFurniture()?.Door == true
                            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE)
                        .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)));
                    if (furniture is null)
                    {
                        player.SendNotification(NotificationType.Error, "Você não está próximo de nenhuma porta.");
                        return;
                    }

                    furniture.SetLocked(!furniture.Locked);
                    player.SendMessageToNearbyPlayers($"{(!furniture.Locked ? "des" : string.Empty)}tranca a porta.", MessageCategory.Ame);

                    var myObject = Global.Objects.FirstOrDefault(x => x.PropertyFurnitureId == furniture.Id);
                    myObject?.SetSharedDataEx("DoorLocked", furniture.Locked);

                    context.PropertiesFurnitures.Update(furniture);
                    await context.SaveChangesAsync();

                    return;
                }
            }

            player.SendNotification(NotificationType.Error, "Você não tem acesso a nenhuma propriedade, veículo ou porta próximos.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [ServerEvent(Event.VehicleDeath)]
    public async void OnVehicleDestroy(GTANetworkAPI.Vehicle vehicleParam)
    {
        try
        {
            var vehicle = Functions.CastVehicle(vehicleParam);
            await Functions.WriteLog(LogType.VehicleDestruction, $"{vehicle.VehicleDB.Id} | {Functions.Serialize(vehicle.Damages)}");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmXMR))]
    public async Task ConfirmXMR(Player playerParam, string url, float volume)
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

            if (player.Vehicle is not MyVehicle vehicle)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            vehicle.AudioSpot ??= new AudioSpot
            {
                Dimension = vehicle.GetDimension(),
                VehicleId = vehicle.Id,
                Range = 15,
            };

            vehicle.AudioSpot.Source = url;
            vehicle.AudioSpot.Volume = volume;

            vehicle.AudioSpot.SetupAllClients();

            player.SendMessageToNearbyPlayers("configura o XMR.", MessageCategory.Ame);
            player.Emit(Constants.BOOMBOX_PAGE_CLOSE);

            var context = Functions.GetDatabaseContext();
            await player.WriteLog(LogType.General, $"Configurar XMR {vehicle.Identifier} {url} {volume}", null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(TurnOffXMR))]
    public async Task TurnOffXMR(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Vehicle is not MyVehicle vehicle)
                return;

            if (vehicle.AudioSpot is not null)
            {
                vehicle.AudioSpot.RemoveAllClients();
                player.SendMessageToNearbyPlayers($"desliga o XMR.", MessageCategory.Ame);
                vehicle.AudioSpot = null;
            }

            player.Emit(Constants.BOOMBOX_PAGE_CLOSE);
            var context = Functions.GetDatabaseContext();
            await player.WriteLog(LogType.General, $"Desligar XMR {vehicle.Identifier}", null);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ConfirmTuning))]
    public async Task ConfirmTuning(Player playerParam, bool confirm, string vehicleTuningJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
            {
                player.SendMessage(MessageType.Error, "Você não está dirigindo um veículo.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var vehicleTuning = Functions.Deserialize<VehicleTuning>(vehicleTuningJSON);

            async Task SetMods()
            {
                var realMods = Functions.Deserialize<List<VehicleMod>>(vehicle.VehicleDB.ModsJSON);
                foreach (var mod in vehicleTuning.Mods.Where(x => x.Type <= (byte)CompanyTuningPriceType.TrimColor))
                {
                    var realMod = realMods.FirstOrDefault(x => x.Type == mod.Type);
                    if (realMod is null)
                    {
                        realMods.Add(new VehicleMod
                        {
                            Type = mod.Type,
                            Id = mod.Selected,
                        });
                    }
                    else
                    {
                        realMod.Id = mod.Selected;
                    }
                }

                var drawingColor1 = System.Drawing.ColorTranslator.FromHtml(vehicleTuning.Color1);
                var drawingColor2 = System.Drawing.ColorTranslator.FromHtml(vehicleTuning.Color2);
                var drawingNeonColor = System.Drawing.ColorTranslator.FromHtml(vehicleTuning.NeonColor);
                var drawingTireSmokeColor = System.Drawing.ColorTranslator.FromHtml(vehicleTuning.TireSmokeColor);

                vehicle.VehicleDB.SetColor(drawingColor1.R, drawingColor1.G, drawingColor1.B, drawingColor2.R, drawingColor2.G, drawingColor2.B);
                vehicle.VehicleDB.SetTunning(vehicleTuning.WheelType, vehicleTuning.WheelVariation, vehicleTuning.WheelColor,
                    drawingNeonColor.R, drawingNeonColor.G, drawingNeonColor.B,
                    Convert.ToBoolean(vehicleTuning.NeonLeft), Convert.ToBoolean(vehicleTuning.NeonRight),
                    Convert.ToBoolean(vehicleTuning.NeonFront), Convert.ToBoolean(vehicleTuning.NeonBack),
                    vehicleTuning.HeadlightColor, vehicleTuning.LightsMultiplier,
                    vehicleTuning.WindowTint, drawingTireSmokeColor.R, drawingTireSmokeColor.G, drawingTireSmokeColor.B,
                    Functions.Serialize(realMods), vehicleTuning.ProtectionLevel,
                    Convert.ToBoolean(vehicleTuning.XMR), vehicleTuning.Livery, Functions.Serialize(vehicleTuning.Extras),
                    Convert.ToBoolean(vehicleTuning.Drift));

                if (vehicle.SpawnType == MyVehicleSpawnType.Normal)
                {
                    context.Vehicles.Update(vehicle.VehicleDB);
                    await context.SaveChangesAsync();
                }

                if (vehicleTuning.Repair == 1)
                    vehicle.RepairEx();
            }

            if (confirm)
            {
                if (vehicleTuning.Staff)
                {
                    await SetMods();
                    await player.WriteLog(LogType.Staff, $"Tunar Veículo | {vehicle.VehicleDB.Id} | {vehicleTuningJSON}", null);
                    player.SendMessage(MessageType.Success, $"Você aplicou as modificações no veículo {vehicle.VehicleDB.Model.ToUpper()} {vehicle.VehicleDB.Plate.ToUpper()}.");
                }
                else
                {
                    if (vehicleTuning.TargetId.HasValue)
                    {
                        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == vehicleTuning.TargetId);
                        if (target is null)
                        {
                            player.SendMessage(MessageType.Error, "O jogador não está mais conectado.");
                        }
                        else
                        {
                            vehicleTuning.VehicleId = vehicle.VehicleDB.Id;
                            vehicleTuning.TargetId = null;
                            vehicleTuning.Staff = false;

                            var invite = new Invite
                            {
                                Type = InviteType.Mechanic,
                                SenderCharacterId = player.Character.Id,
                                Value = [Functions.Serialize(vehicleTuning)],
                            };
                            target.Invites.RemoveAll(x => x.Type == InviteType.Mechanic);
                            target.Invites.Add(invite);

                            player.SendMessage(MessageType.Success, $"Você solicitou enviar o catálogo de modificações veiculares para {target.ICName}.");
                            target.SendMessage(MessageType.Success, $"{player.ICName} solicitou enviar o catálogo de modificações veiculares para você. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");
                        }
                    }
                    else
                    {
                        var vehiclePrice = Functions.GetVehiclePrice(vehicle.VehicleDB.Model);
                        if (vehiclePrice is null)
                        {
                            player.SendNotification(NotificationType.Error, Globalization.VEHICLE_PRICE_NOT_SET);
                            return;
                        }

                        var company = Global.Companies.FirstOrDefault(x => x.Id == vehicleTuning.CompanyId);
                        if (company is null)
                        {
                            player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                            return;
                        }

                        var totalValue = 0;
                        var totalCostValue = 0;
                        foreach (var mod in vehicleTuning.Mods)
                        {
                            var companyTuningPriceType = (CompanyTuningPriceType)mod.Type;
                            var tuningPrice = Functions.GetTuningPrice(vehiclePrice.Value.Item1, company, companyTuningPriceType).Item1;

                            var costPercentagePrice = company.TuningPrices!.FirstOrDefault(x => x.Type == companyTuningPriceType)?.CostPercentagePrice ?? 0;
                            var costTuningPrice = Convert.ToInt32(vehiclePrice.Value.Item1 * (costPercentagePrice / 100f));
                            if (tuningPrice < costTuningPrice)
                            {
                                player.SendMessage(MessageType.Error, $"{companyTuningPriceType.GetDisplay()} de {company.Name} possui Preço de Venda menor que o Preço de Custo. Por favor, avise ao proprietário.");
                                vehicle.SetDefaultMods();
                                player.Emit(Constants.VEHICLE_TUNING_PAGE_CLOSE);
                                return;
                            }

                            var selected = mod.Selected + 1;
                            if (Functions.CheckCompanyTuningPriceTypeMultiplyValue((CompanyTuningPriceType)mod.Type) && selected > 0)
                            {
                                tuningPrice *= selected;
                                costTuningPrice *= selected;
                            }

                            totalValue += tuningPrice;
                            totalCostValue += costTuningPrice;
                        }

                        if (player.Money < totalValue)
                        {
                            player.SendNotification(NotificationType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, totalValue));
                            vehicle.SetDefaultMods();
                            player.Emit(Constants.VEHICLE_TUNING_PAGE_CLOSE);
                            return;
                        }

                        await SetMods();
                        await player.RemoveMoney(totalValue);

                        var safeValue = totalValue - totalCostValue;
                        if (safeValue > 0)
                        {
                            company.DepositSafe(safeValue);
                            context.Companies.Update(company);
                            await context.SaveChangesAsync();
                        }

                        player.SendMessage(MessageType.Success, $"Você aplicou as modificações no veículo {vehicle.VehicleDB.Model.ToUpper()} {vehicle.VehicleDB.Plate.ToUpper()} por ${totalValue:N0}.");
                        await player.WriteLog(LogType.MechanicTunning, $"{vehicle.Identifier} ({vehicle.VehicleDB.Id}) | {company.Name} ({company.Id}) | {totalValue} {totalCostValue} {safeValue} | {vehicleTuningJSON}", null);
                    }
                }
            }

            vehicle.SetDefaultMods();

            player.Emit(Constants.VEHICLE_TUNING_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command("colocar", "/colocar (ID ou nome) (assento (1-3))")]
    public async Task CMD_colocar(MyPlayer player, string idOrName, byte seat)
    {
        if (seat < 1 || seat > 3)
        {
            player.SendMessage(MessageType.Error, "Assento deve ser entre 1 e 3.");
            return;
        }

        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso dentro de um veículo.");
            return;
        }

        var vehicle = Global.Vehicles.Where(x =>
            player.GetPosition().DistanceTo(x.GetPosition()) <= Constants.RP_DISTANCE
            && x.GetDimension() == player.GetDimension()
            && !x.GetLocked())
            .MinBy(x => player.GetPosition().DistanceTo(x.GetPosition()));
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, "Você não está próximo de nenhum veículo destrancado.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        if (target.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Jogador está dentro de um veículo.");
            return;
        }

        if (!target.Cuffed && !target.PlayerCarrying.HasValue)
        {
            player.SendMessage(MessageType.Error, "Jogador não está algemado ou sendo carregado.");
            return;
        }

        if (vehicle.Occupants.Any(x => (x as MyPlayer)!.VehicleSeat == seat))
        {
            player.SendMessage(MessageType.Error, "Este assento está ocupado.");
            return;
        }

        target.StopBeingCarried();
        await Task.Delay(500);
        target.SetIntoVehicleEx(vehicle, seat);

        player.SendMessage(MessageType.Success, $"Você colocou {target.ICName} dentro do veículo.");
        target.SendMessage(MessageType.Success, $"{player.ICName} colocou você dentro do veículo.");
        await player.WriteLog(LogType.PutInVehicle, vehicle.Identifier, target);
    }

    [Command("retirar", "/retirar (ID ou nome)")]
    public async Task CMD_retirar(MyPlayer player, string idOrName)
    {
        if (player.IsInVehicle)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso dentro de um veículo.");
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Globalization.YOU_ARE_NOT_CLOSE_TO_THE_PLAYER);
            return;
        }

        if (!target.Cuffed || target.Vehicle is not MyVehicle vehicle || vehicle.GetLocked())
        {
            player.SendMessage(MessageType.Error, "Jogador não está algemado em um veículo destrancado.");
            return;
        }

        var pos = player.GetPosition();
        pos.Y += 1.5f;
        target.SetPosition(pos, target.GetDimension(), false);

        player.SendMessage(MessageType.Success, $"Você retirou {target.ICName} do veículo.");
        target.SendMessage(MessageType.Success, $"{player.ICName} retirou você de dentro do veículo.");
        await player.WriteLog(LogType.RemoveFromVehicle, vehicle.Identifier, target);
    }

    [Command("helif")]
    public static void CMD_helif(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
        {
            player.SendMessage(MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        if (!vehicle.IsHelicopter)
        {
            player.SendMessage(MessageType.Error, "Você não está em um helicóptero.");
            return;
        }

        if (vehicle.Speed > 10)
        {
            player.SendMessage(MessageType.Error, "Você precisa estar no máximo a 10 km/h.");
            return;
        }

        vehicle.Frozen = !vehicle.Frozen;
        player.SendMessage(MessageType.Success, $"Você {(!vehicle.Frozen ? "des" : string.Empty)}congelou o helicóptero.");
    }

    [Command("valugar")]
    public async Task CMD_valugar(MyPlayer player)
    {
        if (player.Character.Job == CharacterJob.None || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não tem um emprego ou não está em serviço.");
            return;
        }

        var job = Global.Jobs.FirstOrDefault(x => x.CharacterJob == player.Character.Job)!;
        if (!player.IsInVehicle)
        {
            if (player.GetPosition().DistanceTo(new(job.VehicleRentPosX, job.VehicleRentPosY, job.VehicleRentPosZ)) > Constants.RP_DISTANCE)
            {
                player.SendMessage(MessageType.Error, "Você não está no aluguel de veículos para seu emprego.");
                return;
            }
        }

        if (Global.Vehicles.Any(x => x.SpawnType == MyVehicleSpawnType.Rent && x.NameInCharge == player.Character.Name))
        {
            player.SendMessage(MessageType.Error, "Você já possui um veículo alugado.");
            return;
        }

        if (player.Money < job.VehicleRentValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Globalization.INSUFFICIENT_MONEY_ERROR_MESSAGE, job.VehicleRentValue));
            return;
        }

        MyVehicle? vehicle = null;
        if (player.IsInVehicle)
        {
            vehicle = Global.Vehicles.FirstOrDefault(x => x == player.Vehicle
                && x.SpawnType == MyVehicleSpawnType.Rent
                && !x.RentExpirationDate.HasValue);
            if (vehicle is null)
            {
                player.SendMessage(MessageType.Error, "Você não está dentro de um veículo disponível para aluguel.");
                return;
            }
        }
        else
        {
            var vehicleDB = new Domain.Entities.Vehicle();
            vehicleDB.Create(job.VehicleRentModel, "RENT", 0, 0, 0, 0, 0, 0);
            vehicleDB.ChangePosition(job.VehicleRentPosX, job.VehicleRentPosY, job.VehicleRentPosZ,
                job.VehicleRentRotR, job.VehicleRentRotP, job.VehicleRentRotY, player.GetDimension());
            vehicleDB.SetFuel(vehicleDB.GetMaxFuel());
            vehicle = await vehicleDB.Spawnar(null);
            player.SetIntoVehicleEx(vehicle, Constants.VEHICLE_SEAT_DRIVER);
        }

        vehicle.NameInCharge = player.Character.Name;
        vehicle.RentExpirationDate = DateTime.Now.AddHours(1);

        if (job.VehicleRentValue > 0)
            await player.RemoveMoney(job.VehicleRentValue);

        player.SendMessage(MessageType.Success, $"Você alugou um {job.VehicleRentModel.ToUpper()} por ${job.VehicleRentValue:N0} até {vehicle.RentExpirationDate}.");
    }

    [Command("cinto")]
    public static void CMD_cinto(MyPlayer player)
    {
        if (player.Vehicle is not MyVehicle vehicle)
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo.");
            return;
        }

        if (vehicle.GetClass() == VehicleClass.Boats
            || vehicle.GetClass() == VehicleClass.Cycles
            || vehicle.GetClass() == VehicleClass.Motorcycles
            || vehicle.GetClass() == VehicleClass.Helicopters
            || vehicle.GetClass() == VehicleClass.Planes)
        {
            player.SendMessage(MessageType.Error, "Você não está em um veículo que possui cinto de segurança.");
            return;
        }

        player.SeatBelt = !player.SeatBelt;
        player.SendMessageToNearbyPlayers($"{(player.SeatBelt ? "coloca" : "retira")} o cinto de segurança.", MessageCategory.Ame);
    }
}