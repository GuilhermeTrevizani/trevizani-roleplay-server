using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class CellphoneScript : Script
{
    [Command(["celular", "cel"], "Celular", "Abre o celular")]
    public static void CMD_celular(MyPlayer player)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.GetAttachedObjects().Any(x => x.Model == Constants.CELLPHONE_OBJECT))
        {
            player.Emit("CloseCellphone");
            return;
        }

        player.AttachObject(Constants.CELLPHONE_OBJECT, 57005, new(0.15f, 0.01f, -0.02f), new(114.59, 114.59, -18.90));
        player.PlayPhoneBaseAnimation();
        player.Emit("OpenCellphone");
    }

    [RemoteEvent(nameof(OpenCellphone))]
    public static void OpenCellphone(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            CMD_celular(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CloseCellphone))]
    public static void CloseCellphone(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.DetachObject(Constants.CELLPHONE_OBJECT);
            player.StopAnimationEx();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["sms"], "Celular", "Envia um SMS", "(número ou nome do contato) (mensagem)", GreedyArg = true)]
    public async Task CMD_sms(MyPlayer player, string numberOrName, string message)
    {
        if (string.IsNullOrWhiteSpace(message) || message.Length > 255)
        {
            player.SendMessage(MessageType.Error, "Mensagem inválida.");
            return;
        }

        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        if (!uint.TryParse(numberOrName, out uint number) || number == 0)
        {
            if (Guid.TryParse(numberOrName, out Guid phoneGroupId))
            {
                var phoneGroup = Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneGroupId);
                if (phoneGroup is null)
                {
                    player.SendMessage(MessageType.Error, "Grupo não encontrado.");
                    return;
                }

                if (!phoneGroup.Users!.Any(x => x.Number == player.Character.Cellphone))
                {
                    player.SendMessage(MessageType.Error, "Você não está no grupo.");
                    return;
                }

                var phoneMessageGroup = new PhoneMessage();
                phoneMessageGroup.CreateTextToGroup(player.Character.Cellphone, phoneGroup.Id, message);

                await Functions.SendSMS(player,
                    phoneGroup.Users!.Where(x => x.Number != player.Character.Cellphone).Select(x => x.Number).ToArray(),
                    phoneMessageGroup);
                return;
            }

            var contact = player.Contacts.FirstOrDefault(x => x.Name.ToLower().Contains(numberOrName.ToLower()));
            if (contact is null)
            {
                player.SendMessage(MessageType.Error, $"Nenhum contato encontrado contendo {numberOrName}.");
                return;
            }

            number = contact.Number;
        }

        if (number == player.Character.Cellphone)
        {
            player.SendMessage(MessageType.Error, "Você não pode enviar um SMS para você mesmo.");
            return;
        }

        if (number <= 0)
        {
            player.SendMessage(MessageType.Error, "Número inválido.");
            return;
        }

        if (player.Contacts.Any(x => x.Number == number && x.Blocked))
        {
            player.SendMessage(MessageType.Error, "Você bloqueou esse contato.");
            return;
        }

        var phoneMessage = new PhoneMessage();
        phoneMessage.CreateTextToContact(player.Character.Cellphone, number, message);

        await Functions.SendSMS(player, [number], phoneMessage);
    }

    [RemoteEvent(nameof(SendSMS))]
    public async Task SendSMS(Player playerParam, string numberOrName, string message)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await CMD_sms(player, numberOrName, message);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["desligar", "des"], "Celular", "Desliga uma ligação em andamento")]
    public async Task CMD_desligar(MyPlayer player)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.PhoneCall.Number == 0)
        {
            player.SendMessage(MessageType.Error, "Você não está uma ligação.");
            return;
        }

        var target = player.PhoneCall.Number == player.Character.Cellphone ? player.PhoneCall.Origin : player.PhoneCall.Number;
        player.SendMessageToNearbyPlayers("desliga a ligação.", MessageCategory.Ame);
        player.SendMessage(MessageType.None, $"[CELULAR] Você desligou a ligação de {player.GetCellphoneContactName(target)}.", Constants.CELLPHONE_SECONDARY_COLOR);
        await player.EndCellphoneCall();
    }

    [RemoteEvent(nameof(EndCall))]
    public async Task EndCall(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await CMD_desligar(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["atender"], "Celular", "Atende a ligação")]
    public static void CMD_atender(MyPlayer player)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.PhoneCall.Number != player.Character.Cellphone || player.PhoneCall.Type == PhoneCallType.Answered)
        {
            player.SendMessage(MessageType.Error, "Seu celular não está tocando.");
            return;
        }

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x != player && x.PhoneCall.Origin == player.PhoneCall.Origin);
        if (target is null)
        {
            player.SendMessage(MessageType.Error, "Jogador não está online.");
            return;
        }

        player.PhoneAnswerCall(player.PhoneCall.Origin);
        player.PhoneCall.SetType(PhoneCallType.Answered);
        player.SendMessageToNearbyPlayers("atende a ligação.", MessageCategory.Ame);
        player.SendMessage(MessageType.None, $"[CELULAR] Você atendeu a ligação de {player.GetCellphoneContactName(player.PhoneCall.Origin)}.", Constants.CELLPHONE_SECONDARY_COLOR);
        player.CellphoneAudioSpot?.RemoveAllClients();
        player.CellphoneAudioSpot = null;

        target.PhoneAnswerCall(player.Character.Cellphone);
        target.SendMessage(MessageType.None, $"[CELULAR] Sua ligação para {target.GetCellphoneContactName(player.Character.Cellphone)} foi atendida.", Constants.CELLPHONE_SECONDARY_COLOR);
        target.CancellationTokenSourceCellphone?.Cancel();
        target.CancellationTokenSourceCellphone = null;
    }

    [RemoteEvent(nameof(AnswerCall))]
    public static void AnswerCall(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            CMD_atender(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["an"], "Celular", "Envia um anúncio", "(mensagem)", GreedyArg = true)]
    public async Task CMD_an(MyPlayer player, string message)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        if (player.Money < Global.Parameter.AnnouncementValue)
        {
            player.SendMessage(MessageType.Error, string.Format(Resources.YouDontHaveEnoughMoney, Global.Parameter.AnnouncementValue));
            return;
        }

        var minutes = player.User.GetCurrentPremium() switch
        {
            UserPremium.Gold => 5,
            UserPremium.Silver => 10,
            UserPremium.Bronze => 20,
            _ => 30,
        };

        var cooldown = (player.Character.AnnouncementLastUseDate ?? DateTime.MinValue).AddMinutes(minutes);
        if (cooldown > DateTime.Now)
        {
            player.SendMessage(MessageType.Error, $"O uso da central de anúncios estará disponível em {cooldown}.");
            return;
        }

        player.Character.SetAnnouncementLastUseDate();
        var context = Functions.GetDatabaseContext();
        await player.RemoveMoney(Global.Parameter.AnnouncementValue);

        message = Functions.CheckFinalDot(message);
        foreach (var target in Global.SpawnedPlayers.Where(x => !x.User.AnnouncementToggle))
            target.SendMessage(MessageType.None, $"[ANÚNCIO] {message} [CONTATO: {player.Character.Cellphone}]", Constants.ANNOUNCEMENT_COLOR);
        await Functions.SendServerMessage($"{player.Character.Name} ({player.SessionId}) ({player.User.Name}) enviou o anúncio.", UserStaff.GameAdmin, false);

        Global.Announcements.Add(new()
        {
            Type = AnnouncementType.Person,
            Sender = player.Character.Cellphone.ToString(),
            Message = message,
        });

        await player.WriteLog(LogType.Advertisement, message, null);

        if (Global.DiscordClient is null
            || Global.DiscordClient.GetChannel(Global.AnnouncementDiscordChannel) is not SocketTextChannel channel)
            return;

        var embedBuilder = new Discord.EmbedBuilder
        {
            Title = $"Anúncio de #{player.Character.Cellphone}",
            Description = message,
            Color = new Discord.Color(Global.MainRgba.Red, Global.MainRgba.Green, Global.MainRgba.Blue),
        };
        embedBuilder.WithFooter($"Enviado em {DateTime.Now}.");

        await channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    [RemoteEvent(nameof(AddCellphoneContact))]
    public async Task AddCellphoneContact(Player playerParam, uint number, string name, bool favorite, bool blocked)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var context = Functions.GetDatabaseContext();
            var contact = player.Contacts.FirstOrDefault(x => x.Number == number);
            if (contact is null)
            {
                contact = new();
                contact.Create(player.Character.Cellphone, number, name);

                await context.PhonesContacts.AddAsync(contact);
                await context.SaveChangesAsync();

                player.Contacts.Add(contact);
            }
            else
            {
                contact.Update(name, favorite, blocked);

                context.PhonesContacts.Update(contact);
                await context.SaveChangesAsync();
            }

            await player.UpdatePhoneLastMessages();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(RemoveCellphoneContact))]
    public async Task RemoveCellphoneContact(Player playerParam, uint number)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var contact = player.Contacts.FirstOrDefault(x => x.Number == number);
            if (contact is null)
                return;

            var context = Functions.GetDatabaseContext();
            context.PhonesContacts.Remove(contact);
            await context.SaveChangesAsync();

            player.Contacts.Remove(contact);
            await player.UpdatePhoneLastMessages();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["ligar"], "Celular", "Liga para um número", "(número ou nome do contato)")]
    public async Task CMD_ligar(MyPlayer player, string numberOrName)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        if (player.PhoneCall.Number > 0)
        {
            player.SendMessage(MessageType.Error, "Você já está em uma ligação.");
            return;
        }

        if (!uint.TryParse(numberOrName, out uint cellphone) || cellphone == 0)
        {
            var contact = player.Contacts.FirstOrDefault(x => x.Name.ToLower().Contains(numberOrName.ToLower()));
            if (contact is null)
            {
                player.SendMessage(MessageType.Error, $"Nenhum contato encontrado contendo {numberOrName}.");
                return;
            }

            cellphone = contact.Number;
        }

        if (cellphone == player.Character.Cellphone || cellphone == 0)
        {
            player.SendMessage(MessageType.Error, "Você não pode ligar para você mesmo.");
            return;
        }

        player.EmergencyCall = null;

        if (cellphone == Constants.EMERGENCY_NUMBER)
        {
            player.PhoneCall = new PhoneCall();
            player.PhoneCall.Create(player.Character.Cellphone, cellphone);
            player.PhoneCall.SetType(PhoneCallType.Answered);
            player.PhoneAnswerCall(player.PhoneCall.Number);
            player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), $"{Resources.EmergencyCenter}, deseja falar com polícia, bombeiros ou ambos?");
            return;
        }

        if (cellphone == Constants.TAXI_NUMBER)
        {
            player.PhoneCall = new PhoneCall();
            player.PhoneCall.Create(player.Character.Cellphone, cellphone);
            player.PhoneCall.SetType(PhoneCallType.Answered);
            player.PhoneAnswerCall(player.PhoneCall.Number);

            if (!Global.SpawnedPlayers.Any(x => x.Character.Job == CharacterJob.TaxiDriver && x.OnDuty))
            {
                await player.EndCellphoneCall();
                player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), "Desculpe, não temos nenhum taxista em serviço no momento.");
                return;
            }

            player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), $"{Resources.TaxiDriversCenter}, para onde deseja ir?");
            return;
        }

        if (cellphone == Constants.MECHANIC_NUMBER)
        {
            player.PhoneCall = new PhoneCall();
            player.PhoneCall.Create(player.Character.Cellphone, cellphone);
            player.PhoneCall.SetType(PhoneCallType.Answered);
            player.PhoneAnswerCall(player.PhoneCall.Number);

            if (!Global.SpawnedPlayers.Any(x => x.Character.Job == CharacterJob.Mechanic && x.OnDuty))
            {
                await player.EndCellphoneCall();
                player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), "Desculpe, não temos nenhum mecânico em serviço no momento.");
                return;
            }

            player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), $"{Resources.MechanicsCenter}, como podemos ajudar?");
            return;
        }


        if (cellphone == Constants.INSURANCE_NUMBER)
        {
            if (player.Vehicle is not MyVehicle vehicle || vehicle.VehicleDB.CharacterId != player.Character.Id)
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

            player.PhoneCall = new PhoneCall();
            player.PhoneCall.Create(player.Character.Cellphone, cellphone);
            player.PhoneCall.SetType(PhoneCallType.Answered);
            player.PhoneAnswerCall(player.PhoneCall.Number);

            var insuranceDayValue = Convert.ToInt32(vehiclePrice.Value.Item1 * (Global.Parameter.VehicleInsurancePercentage / 100f));

            player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone),
                $"{Resources.Insurance}, quantos dias de seguro veicular você deseja contratar? O valor por dia é ${insuranceDayValue:N0}.");
            return;
        }

        var smuggler = Global.Smugglers.FirstOrDefault(x => x.Cellphone == cellphone
            && (!x.GetAllowedCharacters().Any() || x.GetAllowedCharacters().Contains(player.Character.Name)));
        if (smuggler is not null)
        {
            if (smuggler.CooldownDate > DateTime.Now)
            {
                player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), $"Não consigo agora. Me liga {smuggler.CooldownDate}.");
                return;
            }

            smuggler.CreateIdentifier();
            player.SetWaypoint(smuggler.PosX, smuggler.PosY);
            player.SendCellphoneCallMessage(player.GetCellphoneContactName(cellphone), "Te enviei minha localização, não demora.");
            return;
        }

        if (player.Contacts.Any(x => x.Number == cellphone && x.Blocked))
        {
            player.SendMessage(MessageType.Error, "Você bloqueou esse contato.");
            return;
        }

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == cellphone);
        if (target is null || target.CellphoneItem.FlightMode || target.PhoneCall.Number > 0)
        {
            player.SendMessage(MessageType.Error, $"{player.GetCellphoneContactName(cellphone)} está indisponível.");
            return;
        }

        if (target.Contacts.Any(x => x.Number == player.Character.Cellphone && x.Blocked))
        {
            player.SendMessage(MessageType.Error, $"Você foi bloqueado por {player.GetCellphoneContactName(cellphone)}.");
            return;
        }

        var phoneCall = new PhoneCall();
        phoneCall.Create(player.Character.Cellphone, cellphone);

        target.PhoneCall = player.PhoneCall = phoneCall;

        if (target.CellphoneItem.RingtoneVolume > 0)
        {
            target.CellphoneAudioSpot = new AudioSpot
            {
                Source = Constants.URL_AUDIO_CELLPHONE_RINGTONE,
                Dimension = target.GetDimension(),
                Loop = true,
                Position = target.GetPosition(),
                Range = 5,
                PlayerId = target.Id,
                Volume = target.CellphoneItem.RingtoneVolume / 100,
            };

            target.CellphoneAudioSpot.SetupAllClients();
        }

        player.PlayPhoneCallAnimation();
        player.Emit("PhonePage:CallContactServer", player.GetCellphoneContactName(player.PhoneCall.Number));
        player.CancellationTokenSourceCellphone = new();
        player.SendMessage(MessageType.None, $"[CELULAR] Você está ligando para {player.GetCellphoneContactName(cellphone)}.", Constants.CELLPHONE_SECONDARY_COLOR);
        target.Emit("PhonePage:ReceiveCallServer", target.GetCellphoneContactName(player.Character.Cellphone));
        target.SendMessage(MessageType.None, $"[CELULAR] Ligação de {target.GetCellphoneContactName(player.Character.Cellphone)}. (/atender ou /des)", Constants.CELLPHONE_SECONDARY_COLOR);
        await Task.Delay(TimeSpan.FromSeconds(20), player.CancellationTokenSourceCellphone.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            Task.Run(async () =>
            {
                player.CancellationTokenSourceCellphone = null;
                player.SendMessage(MessageType.None, $"[CELULAR] Sua ligação para {player.GetCellphoneContactName(player.PhoneCall.Number)} não foi atendida.", Constants.CELLPHONE_SECONDARY_COLOR);
                await player.EndCellphoneCall();
            });
        });
    }

    [RemoteEvent(nameof(CallCellphone))]
    public async Task CallCellphone(Player playerParam, string numberOrName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await CMD_ligar(player, numberOrName);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["eloc"], "Celular", "Envia sua localização", "(número ou nome do contato)")]
    public async Task CMD_eloc(MyPlayer player, string numberOrName)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.Character.JailFinalDate.HasValue)
        {
            player.SendMessage(MessageType.Error, "Você não pode fazer isso pois está preso.");
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        var context = Functions.GetDatabaseContext();
        if (!uint.TryParse(numberOrName, out uint number) || number == 0)
        {
            if (Guid.TryParse(numberOrName, out Guid phoneGroupId))
            {
                var phoneGroup = Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneGroupId);
                if (phoneGroup is null)
                {
                    player.SendMessage(MessageType.Error, "Grupo não encontrado.");
                    return;
                }

                if (!phoneGroup.Users!.Any(x => x.Number == player.Character.Cellphone))
                {
                    player.SendMessage(MessageType.Error, "Você não está no grupo.");
                    return;
                }

                var phoneMessageGroup = new PhoneMessage();
                phoneMessageGroup.CreateLocationToGroup(player.Character.Cellphone, phoneGroup.Id, player.ICPosition.X, player.ICPosition.Y);

                await Functions.SendSMS(player,
                    phoneGroup.Users!.Where(x => x.Number != player.Character.Cellphone).Select(x => x.Number).ToArray(),
                    phoneMessageGroup);

                return;
            }

            var contact = player.Contacts.FirstOrDefault(x => x.Name.ToLower().Contains(numberOrName.ToLower()));
            if (contact is null)
            {
                player.SendMessage(MessageType.Error, $"Nenhum contato encontrado contendo {numberOrName}.");
                return;
            }

            number = contact.Number;
        }

        if (number <= 0)
        {
            player.SendMessage(MessageType.Error, "Número inválido.");
            return;
        }

        if (number == player.Character.Cellphone)
        {
            player.SendMessage(MessageType.Error, "Você não pode enviar uma localização para você mesmo.");
            return;
        }

        if (player.Contacts.Any(x => x.Number == number && x.Blocked))
        {
            player.SendMessage(MessageType.Error, "Você bloqueou esse contato.");
            return;
        }

        var phoneMessage = new PhoneMessage();
        phoneMessage.CreateLocationToContact(player.Character.Cellphone, number, player.ICPosition.X, player.ICPosition.Y);

        await Functions.SendSMS(player, [number], phoneMessage);
    }

    [RemoteEvent(nameof(SendCellphoneLocation))]
    public async Task SendCellphoneLocation(Player playerParam, string numberOrName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await CMD_eloc(player, numberOrName);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CellphoneSaveSettings))]
    public async Task CellphoneSaveSettings(Player playerParam, bool flightMode, string wallpaper, string password,
        int ringtoneVolume, int scale)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.CellphoneItem.FlightMode = flightMode;
            if (player.CellphoneItem.FlightMode)
                await player.EndCellphoneCall();
            player.CellphoneItem.Wallpaper = wallpaper;
            player.CellphoneItem.Password = password;
            player.CellphoneItem.RingtoneVolume = ringtoneVolume;
            player.CellphoneItem.Scale = scale;
            await player.UpdateCellphoneDatabase();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["gps"], "Celular", "Busca a localização de uma propriedade", "(endereço)", GreedyArg = true)]
    public static void CMD_gps(MyPlayer player, string address)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.FormatedAddress == address);
        if (property is null)
        {
            player.SendMessage(MessageType.Error, $"Propriedade {address} não existe.");
            return;
        }

        player.SetWaypoint(property.EntrancePosX, property.EntrancePosY);
        player.SendMessage(MessageType.None, $"[CELULAR] Propriedade {property.FormatedAddress} foi marcada no GPS.", Constants.CELLPHONE_SECONDARY_COLOR);
    }

    [Command(["temperatura"], "Celular", "Visualiza a temperatura e o clima atual")]
    public static void CMD_temperatura(MyPlayer player)
    {
        if (player.Character.Cellphone == 0)
        {
            player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        if (player.CellphoneItem.FlightMode)
        {
            player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
            return;
        }

        var weather = Global.WeatherInfo.WeatherType switch
        {
            Weather.EXTRASUNNY => "Ensolarado",
            Weather.CLEAR => "Limpo",
            Weather.SMOG => "Enfumaçado",
            Weather.FOGGY => "Neblina",
            Weather.OVERCAST or Weather.CLOUDS => "Nublado",
            Weather.RAIN => "Chuva",
            Weather.THUNDER => "Tempestade",
            Weather.CLEARING => "Garoa",
            Weather.SNOW or Weather.BLIZZARD or Weather.SNOWLIGHT or Weather.XMAS => "Neve",
            _ => "Desconhecido",
        };

        player.SendMessage(MessageType.None, $"[CELULAR] Temperatura: {Global.WeatherInfo.Main.Temp:N0}°C Tempo: {weather}");
    }

    [RemoteEvent(nameof(CellphoneCreateGroup))]
    public async Task CellphoneCreateGroup(Player playerParam, string name, string participantsJson)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Character.Cellphone == 0)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
                return;
            }

            if (player.IsActionsBlocked())
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.CellphoneItem.FlightMode)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
                return;
            }

            if (name.Length < 1 || name.Length > 25)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Nome do grupo deve ter entre 1 e 25 caracteres.");
                return;
            }

            var participants = Functions.Deserialize<List<uint>>(participantsJson).Where(x => x > 0).ToList();
            if (participants.Count == 0)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Nenhum participante selecionado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var phonesGroupsUsers = new List<PhoneGroupUser>();
            foreach (var participant in participants)
            {
                if (player.Contacts.Any(x => x.Number == participant && x.Blocked))
                {
                    player.EndLoading();
                    player.SendMessage(MessageType.Error, $"Você bloqueou {player.GetCellphoneContactName(participant)}, não é possível adicioná-lo no grupo.");
                    return;
                }

                if (await context.PhonesContacts.AnyAsync(x => x.Origin == participant && x.Number == player.Character.Cellphone && x.Blocked))
                {
                    player.EndLoading();
                    player.SendMessage(MessageType.Error, $"Você foi bloqueado por {player.GetCellphoneContactName(participant)}.");
                    return;
                }

                var phoneGroupUser = new PhoneGroupUser();
                phoneGroupUser.Create(participant, PhoneGroupUserPermission.User);
                phonesGroupsUsers.Add(phoneGroupUser);
            }

            var phoneGroupUserAdmin = new PhoneGroupUser();
            phoneGroupUserAdmin.Create(player.Character.Cellphone, PhoneGroupUserPermission.Admin);
            phonesGroupsUsers.Add(phoneGroupUserAdmin);

            var phoneGroup = new PhoneGroup();
            phoneGroup.Create(name, phonesGroupsUsers);

            await context.PhonesGroups.AddAsync(phoneGroup);
            await context.SaveChangesAsync();

            Global.PhonesGroups.Add(phoneGroup);

            foreach (var participant in participants)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == participant);
                target?.UpdatePhoneGroups();
            }

            await player.UpdatePhoneGroups();
            player.Emit("PhonePage:CreateGroupServer");
            await player.WriteLog(LogType.Cellphone, $"Criou o grupo {phoneGroup.Name} | {Functions.Serialize(phoneGroup.Users!)}", null);
            player.EndLoading();
            player.SendMessage(MessageType.Success, "Você criou o grupo.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CellphoneGroupExit))]
    public async Task CellphoneGroupExit(Player playerParam, string groupId)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Character.Cellphone == 0)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
                return;
            }

            if (player.IsActionsBlocked())
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.CellphoneItem.FlightMode)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
                return;
            }

            var phoneGroup = Global.PhonesGroups.FirstOrDefault(x => x.Id == groupId.ToGuid());
            if (phoneGroup is null)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.RecordNotFound);
                return;
            }

            var phoneGroupUser = phoneGroup.Users!.FirstOrDefault(x => x.Number == player.Character.Cellphone);
            if (phoneGroupUser is null)
            {
                await player.UpdatePhoneGroups();
                player.Emit("PhonePage:CreateGroupServer");
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não está nesse grupo.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.PhonesGroupsUsers.Remove(phoneGroupUser);
            await context.SaveChangesAsync();

            phoneGroup.Users!.Remove(phoneGroupUser);

            foreach (var participant in phoneGroup.Users!)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == participant.Number);
                target?.UpdatePhoneGroups();
            }

            await player.UpdatePhoneGroups();
            player.Emit("PhonePage:CreateGroupServer");
            await player.WriteLog(LogType.Cellphone, $"Saiu do grupo {phoneGroup.Name} ({phoneGroup.Id})", null);
            player.EndLoading();
            player.SendMessage(MessageType.Success, "Você saiu do grupo.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CellphoneGroupAddMembers))]
    public async Task CellphoneGroupAddMembers(Player playerParam, string groupId, string participantsJson)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Character.Cellphone == 0)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
                return;
            }

            if (player.IsActionsBlocked())
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.CellphoneItem.FlightMode)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
                return;
            }

            var phoneGroup = Global.PhonesGroups.FirstOrDefault(x => x.Id == groupId.ToGuid());
            if (phoneGroup is null)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.RecordNotFound);
                return;
            }

            var phoneGroupUser = phoneGroup.Users!.FirstOrDefault(x => x.Number == player.Character.Cellphone);
            if (phoneGroupUser is null)
            {
                await player.UpdatePhoneGroups();
                player.Emit("PhonePage:CreateGroupServer");
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não está nesse grupo.");
                return;
            }

            if (phoneGroupUser.Permission != PhoneGroupUserPermission.Admin)
            {
                await player.UpdatePhoneGroups();
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não é administrador do grupo.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var participants = Functions.Deserialize<List<uint>>(participantsJson).Where(x => x > 0).ToList();
            if (participants.Count == 0)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Nenhum participante selecionado.");
                return;
            }

            var phonesGroupsUsers = new List<PhoneGroupUser>();
            foreach (var participant in participants)
            {
                if (player.Contacts.Any(x => x.Number == participant && x.Blocked))
                {
                    player.EndLoading();
                    player.SendMessage(MessageType.Error, $"Você bloqueou {player.GetCellphoneContactName(participant)}, não é possível adicioná-lo no grupo.");
                    return;
                }

                if (await context.PhonesContacts.AnyAsync(x => x.Origin == participant && x.Number == player.Character.Cellphone && x.Blocked))
                {
                    player.EndLoading();
                    player.SendMessage(MessageType.Error, $"Você foi bloqueado por {player.GetCellphoneContactName(participant)}.");
                    return;
                }

                var phoneGroupUserAdd = new PhoneGroupUser();
                phoneGroupUserAdd.Create(phoneGroup.Id, participant, PhoneGroupUserPermission.User);
                phonesGroupsUsers.Add(phoneGroupUserAdd);
            }

            await context.PhonesGroupsUsers.AddRangeAsync(phonesGroupsUsers);
            await context.SaveChangesAsync();

            phonesGroupsUsers.ForEach(x => phoneGroup.Users!.Add(x));

            foreach (var participant in phoneGroup.Users!)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == participant.Number);
                target?.UpdatePhoneGroups();
            }

            player.Emit("PhonePage:CreateGroupServer");
            await player.WriteLog(LogType.Cellphone, $"Adicionou membros no grupo {phoneGroup.Name} | {participantsJson}", null);
            player.SendMessage(MessageType.Success, "Você adicionou membros no grupo.");
            player.EndLoading();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CellphoneGroupRemoveMember))]
    public async Task CellphoneGroupRemoveMember(Player playerParam, string groupId, uint number)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Character.Cellphone == 0)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
                return;
            }

            if (player.IsActionsBlocked())
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.CellphoneItem.FlightMode)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
                return;
            }

            var phoneGroup = Global.PhonesGroups.FirstOrDefault(x => x.Id == groupId.ToGuid());
            if (phoneGroup is null)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.RecordNotFound);
                return;
            }

            var phoneGroupUser = phoneGroup.Users!.FirstOrDefault(x => x.Number == player.Character.Cellphone);
            if (phoneGroupUser is null)
            {
                await player.UpdatePhoneGroups();
                player.Emit("PhonePage:CreateGroupServer");
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não está nesse grupo.");
                return;
            }

            if (phoneGroupUser.Permission != PhoneGroupUserPermission.Admin)
            {
                await player.UpdatePhoneGroups();
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não é administrador do grupo.");
                return;
            }

            var phoneGroupUserTarget = phoneGroup.Users!.FirstOrDefault(x => x.Number == number);
            if (phoneGroupUserTarget is null)
            {
                await player.UpdatePhoneGroups();
                player.EndLoading();
                player.SendMessage(MessageType.Error, $"{player.GetCellphoneContactName(number)} não está no grupo.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.PhonesGroupsUsers.Remove(phoneGroupUserTarget);
            await context.SaveChangesAsync();

            phoneGroup.Users!.Remove(phoneGroupUserTarget);

            foreach (var participant in phoneGroup.Users!)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == participant.Number);
                target?.UpdatePhoneGroups();
            }

            var onlineTarget = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == number);
            onlineTarget?.UpdatePhoneGroups();

            await player.WriteLog(LogType.Cellphone, $"Removeu membro do grupo {phoneGroup.Name} | {number}", null);
            player.SendMessage(MessageType.Success, $"Você removeu {player.GetCellphoneContactName(number)} do grupo.");
            player.EndLoading();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CellphoneGroupToggleMemberPermission))]
    public async Task CellphoneGroupToggleMemberPermission(Player playerParam, string groupId, uint number)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Character.Cellphone == 0)
            {
                player.SendMessage(MessageType.Error, Resources.YouDontHaveAnEquippedCellphone);
                return;
            }

            if (player.IsActionsBlocked())
            {
                player.SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
                return;
            }

            if (player.CellphoneItem.FlightMode)
            {
                player.SendMessage(MessageType.Error, "Seu celular está em modo avião.");
                return;
            }

            var phoneGroup = Global.PhonesGroups.FirstOrDefault(x => x.Id == groupId.ToGuid());
            if (phoneGroup is null)
            {
                player.EndLoading();
                player.SendMessage(MessageType.Error, Resources.RecordNotFound);
                return;
            }

            var phoneGroupUser = phoneGroup.Users!.FirstOrDefault(x => x.Number == player.Character.Cellphone);
            if (phoneGroupUser is null)
            {
                await player.UpdatePhoneGroups();
                player.Emit("PhonePage:CreateGroupServer");
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não está nesse grupo.");
                return;
            }

            if (phoneGroupUser.Permission != PhoneGroupUserPermission.Admin)
            {
                await player.UpdatePhoneGroups();
                player.EndLoading();
                player.SendMessage(MessageType.Error, "Você não é administrador do grupo.");
                return;
            }

            var phoneGroupUserTarget = phoneGroup.Users!.FirstOrDefault(x => x.Number == number);
            if (phoneGroupUserTarget is null)
            {
                await player.UpdatePhoneGroups();
                player.EndLoading();
                player.SendMessage(MessageType.Error, $"{player.GetCellphoneContactName(number)} não está no grupo.");
                return;
            }

            phoneGroupUserTarget.SetPermission(phoneGroupUserTarget.Permission == PhoneGroupUserPermission.User ?
                PhoneGroupUserPermission.Admin : PhoneGroupUserPermission.User);

            var context = Functions.GetDatabaseContext();
            context.PhonesGroupsUsers.Update(phoneGroupUserTarget);
            await context.SaveChangesAsync();

            foreach (var participant in phoneGroup.Users!)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == participant.Number);
                target?.UpdatePhoneGroups();
            }

            await player.WriteLog(LogType.Cellphone, $"Atualização membro do grupo {phoneGroup.Name} | {number}", null);
            player.SendMessage(MessageType.Success, $"Você atualização as informações de {player.GetCellphoneContactName(number)} no grupo.");
            player.EndLoading();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ReadMessages))]
    public async Task ReadMessages(Player playerParam, string messagesIdsJson)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var messagesIds = Functions.Deserialize<Guid[]>(messagesIdsJson);
            if (messagesIds.Length == 0)
                return;

            var context = Functions.GetDatabaseContext();
            var messagesToRead = await context.PhonesMessages
                .Where(x => messagesIds.Contains(x.Id) && x.Origin != player.Character.Cellphone && !x.Reads!.Any(y => y.Number == player.Character.Cellphone))
                .Select(x => x.Id)
                .ToListAsync();
            if (messagesToRead.Count == 0)
                return;

            var phonesMessagesReads = new List<PhoneMessageRead>();
            foreach (var messageToRead in messagesToRead)
            {
                var phoneMessageRead = new PhoneMessageRead();
                phoneMessageRead.Create(messageToRead, player.Character.Cellphone);
                phonesMessagesReads.Add(phoneMessageRead);
            }

            await context.PhonesMessagesReads.AddRangeAsync(phonesMessagesReads);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CellphoneGetChatMessages))]
    public async Task CellphoneGetChatMessages(Player playerParam, string numberOrGroupId, bool isGroup)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            player.CurrentPhoneChat = new(numberOrGroupId, isGroup);
            await player.UpdatePhoneChatMessages();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["vivavoz"], "Celular", "Ativa/desativa o viva-voz do celular")]
    public static void CMD_vivavoz(MyPlayer player)
    {
        if (player.PhoneCall.Type != PhoneCallType.Answered)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma ligação.");
            return;
        }

        player.CellphoneSpeakers = !player.CellphoneSpeakers;
        player.SendMessage(MessageType.Success, $"Viva voz {(!player.CellphoneSpeakers ? "des" : string.Empty)}ativado.");
    }
}