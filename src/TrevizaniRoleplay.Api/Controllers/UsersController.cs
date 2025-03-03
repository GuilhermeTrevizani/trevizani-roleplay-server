using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("users")]
public class UsersController(
    DatabaseContext context,
    IOptions<JwtSettings> jwtSettings,
    IOptions<DiscordSettings> discordSettings) : BaseController(context)
{
    [HttpPost("login/{discordToken}"), AllowAnonymous]
    public async Task<LoginResponse> Login(string discordToken)
    {
        var httpClient = new HttpClient();

        var url = "https://discord.com/api/oauth2/token";

        var formData = new Dictionary<string, string>
            {
                { "client_id", discordSettings.Value.ClientId },
                { "client_secret", discordSettings.Value.ClientSecret },
                { "grant_type", "authorization_code" },
                { "code", discordToken },
                { "redirect_uri", discordSettings.Value.RedirectUri}
            };

        var content = new FormUrlEncodedContent(formData);
        var response = await httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();
        var tokenResponse = Deserialize<DiscordTokenResponse>(responseString);
        discordToken = tokenResponse!.Access_Token;

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {discordToken}");

        var res = await httpClient.GetFromJsonAsync<DiscordResponse>("https://discordapp.com/api/users/@me")
            ?? throw new ArgumentException("Usuário do Discord não encontrado.");

        res.Username = res.Discriminator == "0" ? res.Username : $"{res.Username}#{res.Discriminator}";
        res.Global_Name ??= string.Empty;

        var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordId == res.Id);
        if (user is null)
        {
            var hasUsers = await context.Users.AnyAsync();
            user = new();
            user.Create(res.Id, res.Username, res.Global_Name, Ip,
                hasUsers ? UserStaff.None : UserStaff.HeadServerDeveloper,
                hasUsers ? "[]" : Serialize(Enum.GetValues<StaffFlag>()));
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
                    var banMessage = "Você está banido";
                    if (banishment.ExpirationDate.HasValue)
                        banMessage += $". Seu banimento expira em: {banishment.ExpirationDate}.";
                    else
                        banMessage += " permanentemente.";
                    banMessage += $" Banido em {banishment.RegisterDate} por {banishment.StaffUser!.Name}. Motivo: {banishment.Reason}";
                    throw new ArgumentException(banMessage);
                }

                context.Banishments.Remove(banishment);
                await context.SaveChangesAsync();
            }
        }

        return new()
        {
            Name = user.Name,
            Token = GenerateToken(user.Id),
            Avatar = $"https://cdn.discordapp.com/avatars/{user.DiscordId}/{res.Avatar}.png?size=128",
            Staff = user.Staff,
            DiscordUsername = user.DiscordUsername,
            StaffFlags = Deserialize<StaffFlag[]>(user.StaffFlagsJSON)!,
        };
    }

    private string GenerateToken(Guid id)
    {
        var claims = new List<Claim>
        {
            new("Id", id.ToString()),
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Identity");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddDays(jwtSettings.Value.DaysToExpire),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var generatedToken = tokenHandler.WriteToken(securityToken);

        return generatedToken;
    }

    [HttpGet("staffers"), Authorize(Policy = PolicySettings.POLICY_SERVER_SUPPORT)]
    public async Task<IEnumerable<StafferResponse>> GetStaffers()
    {
        return (await context.Users
            .Where(x => x.Staff != UserStaff.None)
            .Include(x => x.Characters)
            .ToListAsync())
            .OrderByDescending(x => x.Staff)
                .ThenBy(x => x.Name)
            .Select(x => new StafferResponse
            {
                Staff = x.Staff.GetDescription(),
                Name = $"{x.Name} ({x.DiscordUsername})",
                HelpRequestsAnswersQuantity = x.HelpRequestsAnswersQuantity,
                CharacterApplicationsQuantity = x.CharacterApplicationsQuantity,
                StaffDutyTime = x.StaffDutyTime,
                ConnectedTime = x.Characters!.Sum(x => x.ConnectedTime),
                Flags = Deserialize<List<StaffFlag>>(x.StaffFlagsJSON)!.Select(x => x.GetDescription()).Order(),
                LastAccessDate = x.LastAccessDate,
            });
    }

    [HttpGet("dashboard")]
    public async Task<DashboardResponse> GetDashboard()
    {
        return new()
        {
            Users = await context.Users.CountAsync(),
            Characters = await context.Characters.CountAsync(),
            Vehicles = await context.Vehicles.CountAsync(),
            Properties = await context.Properties.CountAsync(),
        };
    }

    [HttpGet("me")]
    public async Task<UserMyInfoResponse> GetMyInfo()
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
        return new()
        {
            PremiumPoints = user!.PremiumPoints,
            Staff = user.Staff,
            StaffFlags = Deserialize<StaffFlag[]>(user.StaffFlagsJSON)!,
        };
    }

    [HttpGet("potential-fakes"), Authorize(Policy = PolicySettings.POLICY_JUNIOR_SERVER_ADMIN)]
    public async Task<IEnumerable<PotentialFakeResponse>> GetPotentialFakes()
    {
        var users = await context.Database.SqlQueryRaw<PotentialFakeResponse>(@"SELECT DISTINCT GROUP_CONCAT(DiscordUserName ORDER BY DiscordUsername SEPARATOR ', ') AS Users
        FROM (
            SELECT DISTINCT s.Ip, u.DiscordUserName FROM Sessions s
            inner join characters c on c.Id = s.CharacterId
            inner join users u on u.Id = c.UserId
            WHERE s.Ip != ''
        ) as AllUsers
        GROUP BY Ip
        HAVING COUNT(*) > 1;").ToListAsync();

        var usersLogs = await context.Database.SqlQueryRaw<PotentialFakeResponse>(@"SELECT DISTINCT GROUP_CONCAT(DiscordUserName ORDER BY DiscordUsername SEPARATOR ', ') AS Users
        FROM (
            SELECT DISTINCT s.SocialCLubName, u.DiscordUserName FROM Sessions s
            inner join characters c on c.Id = s.CharacterId
            inner join users u on u.Id = c.UserId
            WHERE s.SocialCLubName != ''
        ) as AllUsers
        GROUP BY SocialCLubName
        HAVING COUNT(*) > 1;").ToListAsync();

        users.AddRange(usersLogs);
        users = [.. users.DistinctBy(x => x.Users)];
        return users;
    }
}