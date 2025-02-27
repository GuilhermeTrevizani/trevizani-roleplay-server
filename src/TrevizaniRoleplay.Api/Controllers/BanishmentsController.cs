using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("banishments")]
public class BanishmentsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_SERVER_SUPPORT)]
    public async Task<IEnumerable<BanishmentResponse>> Get()
    {
        var banishments = await context.Banishments
            .Include(x => x.Character)
            .Include(x => x.User)
            .Include(x => x.StaffUser)
            .OrderByDescending(x => x.ExpirationDate)
            .ThenByDescending(x => x.RegisterDate)
            .Select(x => new BanishmentResponse
            {
                Id = x.Id,
                RegisterDate = x.RegisterDate,
                ExpirationDate = x.ExpirationDate,
                Reason = x.Reason,
                Character = x.Character!.Name,
                User = $"{x.User!.Name} ({x.User.DiscordUsername})",
                UserStaff = $"{x.StaffUser!.Name} ({x.StaffUser.DiscordUsername})",
            })
            .ToListAsync();

        return banishments;
    }

    [HttpPost("unban"), Authorize(Policy = PolicySettings.POLICY_JUNIOR_SERVER_ADMIN)]
    public async Task Unban([FromBody] UnbanRequest request)
    {
        var banishment = await context.Banishments.FirstOrDefaultAsync(x => x.Id == request.Id)
            ?? throw new ArgumentException("O banimento não foi encontrado.");

        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);

        if (request.Total)
        {
            context.Banishments.Remove(banishment);
        }
        else
        {
            banishment.ClearUserId();
            context.Banishments.Update(banishment);
        }

        await context.SaveChangesAsync();
        await WriteLog(LogType.Staff, $"{user!.Name} desbaniu {Serialize(banishment)}. Total: {request.Total}");
    }
}