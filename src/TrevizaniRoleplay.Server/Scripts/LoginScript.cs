using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class LoginScript : Script
{
    public static string GetName(ulong id, string? globalName)
    {
        globalName ??= string.Empty;
        if (Global.DiscordClient is null)
            return globalName;

        var guild = Global.DiscordClient.GetGuild(Global.MainDiscordGuild);
        if (guild is null)
            return globalName;

        var user = guild.GetUser(id);
        return user is null ? globalName : user.DisplayName;
    }

    [RemoteEvent(nameof(ValidateDiscordToken))]
    public async Task ValidateDiscordToken(Player playerParam, string discordToken, float resolutionX, float resolutionY)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);

            var url = "https://discord.com/api/oauth2/token";

            var httpClient = new HttpClient();
            var formData = new Dictionary<string, string>
            {
                { "client_id", Global.DiscordClientId },
                { "client_secret", Global.DiscordClientSecret },
                { "grant_type", "authorization_code" },
                { "code", discordToken },
                { "redirect_uri",  "https://ucp.ls-chronicles.com.br/login"}
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = Functions.Deserialize<DiscordTokenResponse>(responseString);
            var token = tokenResponse!.Access_Token;

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var res = await httpClient.GetFromJsonAsync<DiscordResponse>("https://discordapp.com/api/users/@me");
            if (res is null)
            {
                player.SendNotification(NotificationType.Error, "Usuário do Discord não encontrado.");
                return;
            }
            res.Username = res.Discriminator == "0" ? res.Username : $"{res.Username}#{res.Discriminator}";
            res.Global_Name = GetName(Convert.ToUInt64(res.Id), res.Global_Name);

            var displayResolution = $"{resolutionX}x{resolutionY}";

            var context = Functions.GetDatabaseContext();
            var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordId == res.Id);
            if (user is null)
            {
                var hasUsers = await context.Users.AnyAsync();
                user = new();
                user.Create(res.Id, res.Username, res.Global_Name, player.RealIp,
                    hasUsers ? UserStaff.None : UserStaff.Founder,
                    hasUsers ? "[]" : Functions.Serialize(Enum.GetValues<StaffFlag>()));
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
            else
            {
                var banishment = await context.Banishments
                    .Include(x => x.StaffUser)
                    .FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (banishment is not null)
                {
                    if (!banishment.ExpirationDate.HasValue || DateTime.Now <= banishment.ExpirationDate)
                    {
                        player.Emit("ViewBanishmentInfo", Functions.Serialize(new
                        {
                            banishment.RegisterDate,
                            banishment.Reason,
                            banishment.ExpirationDate,
                            Staffer = banishment.StaffUser!.Name,
                        }));
                        return;
                    }

                    context.Banishments.Remove(banishment);
                    await context.SaveChangesAsync();
                }

                if (Global.AllPlayers.Any(x => x.User?.Id == user.Id))
                {
                    await player.WriteLog(LogType.DoubleIdentity, $"Usuário Já Logado | {user.Id} | {user.Name} | {player.RealIp}", null);
                    player.SendNotification(NotificationType.Error, "Usuário já está logado.");
                    return;
                }
            }

            player.User = user;
            player.User.UpdateLastAccess(player.RealIp, res.Username, res.Global_Name, displayResolution);
            await player.CheckDiscordBooster();
            context.Users.Update(player.User);
            await context.SaveChangesAsync();
            player.StaffFlags = Functions.Deserialize<List<StaffFlag>>(player.User.StaffFlagsJSON);

            await ListCharacters(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ListCharacters))]
    public async Task ListCharacters(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            await player.ListCharacters(string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(SelectCharacter))]
    public async Task SelectCharacter(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);

            if (Global.DiscordClient is not null)
            {
                var guild = Global.DiscordClient.GetGuild(Global.MainDiscordGuild);
                if (guild is null || guild.GetUser(Convert.ToUInt64(player.User.DiscordId)) is null)
                {
                    player.SendNotification(NotificationType.Error, "Você não está em nosso Discord principal. Você pode encontrar o convite no UCP.");
                    return;
                }
            }

            var context = Functions.GetDatabaseContext();
            var id = idString.ToGuid();
            var character = await context.Characters
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == player.User.Id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, "Personagem não encontrado.");
                return;
            }

            if (character.DeletedDate.HasValue)
            {
                player.SendNotification(NotificationType.Error, "Personagem foi deletado.");
                return;
            }

            if (character.NameChangeStatus == CharacterNameChangeStatus.Done)
            {
                player.SendNotification(NotificationType.Error, "Personagem sofreu mudança de nome.");
                return;
            }

            if (character.DeathDate.HasValue)
            {
                player.SendNotification(NotificationType.Error, "Personagem está morto.");
                return;
            }

            if (character.CKAvaliation)
            {
                player.SendNotification(NotificationType.Error, "Personagem está sob avaliação de CK.");
                return;
            }

            if (character.EvaluatorStaffUserId is null)
            {
                player.SendNotification(NotificationType.Error, "Personagem está aguardando avaliação.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(character.RejectionReason))
            {
                player.SendNotification(NotificationType.Error, "Personagem foi rejeitado. Reenvie a aplicação pelo UCP.");
                return;
            }

            var banishment = await context.Banishments
                .Include(x => x.StaffUser)
                .FirstOrDefaultAsync(x => x.CharacterId == character.Id);
            if (banishment is not null)
            {
                if (banishment.ExpirationDate.HasValue && DateTime.Now > banishment.ExpirationDate)
                {
                    context.Banishments.Remove(banishment);
                    await context.SaveChangesAsync();
                }
                else
                {
                    var strBan = !banishment.ExpirationDate.HasValue ? " permanentemente." : $". Seu banimento expira em: {banishment.ExpirationDate?.ToString()}";
                    player.SendNotification(NotificationType.Error, $"Você está banido{strBan}<br/>Data: <strong>{banishment.RegisterDate}</strong><br/>Motivo: <strong>{banishment.Reason}</strong><br/>Staffer: <strong>{banishment.StaffUser!.Name}</strong>");
                    return;
                }
            }

            player.Character = character;
            player.Character.UpdateLastAccess(player.RealIp);
            player.Character.SetConnected(true);
            context.Characters.Update(player.Character);
            await context.SaveChangesAsync();

            player.SetModel(player.Character.Model);
            player.Wounds = Functions.Deserialize<List<Wound>>(player.Character.WoundsJSON);
            player.Items = await context.CharactersItems.Where(x => x.CharacterId == player.Character.Id).ToListAsync();
            player.FactionFlags = Functions.Deserialize<List<FactionFlag>>(player.Character.FactionFlagsJSON);
            player.PropertiesAccess = await context.CharactersProperties.Where(x => x.CharacterId == player.Character.Id).Select(x => x.PropertyId).ToListAsync();
            player.VehiclesAccess = await context.CharactersVehicles.Where(x => x.CharacterId == player.Character.Id).Select(x => x.VehicleId).ToListAsync();
            player.SetOutfit();

            if (!string.IsNullOrWhiteSpace(player.Character.PersonalizationJSON))
                player.Personalization = Functions.Deserialize<Personalization>(player.Character.PersonalizationJSON);

            if (player.Personalization.Structure.Count == 0)
                player.Personalization.Structure = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

            if (player.Personalization.OpacityOverlays.Count == 0)
                player.Personalization.OpacityOverlays =
                [
                    new(0),
                    new(3),
                    new(6),
                    new(7),
                    new(9),
                    new(11),
                ];

            if (player.Personalization.ColorOverlays.Count == 0)
                player.Personalization.ColorOverlays =
                [
                    new(1),
                    new(2),
                    new(4),
                    new(5),
                    new(8),
                    new(10),
                ];

            player.ClearBloodDamage();
            player.SetPersonalization();

            await player.WriteLog(LogType.Entrance, string.Empty, null);

            if (player.Character.PersonalizationStep == CharacterPersonalizationStep.Ready)
            {
                await player.SpawnEx();
            }
            else
            {
                player.SetPosition(new(-158.47913f, -297.7055f, 40f), player.ExclusiveDimension, true);
                player.SelectCharacter();
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}