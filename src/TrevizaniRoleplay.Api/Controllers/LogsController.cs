using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("logs")]
public class LogsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("types"), Authorize(Policy = PolicySettings.POLICY_LEAD_ADMIN)]
    public async Task<IEnumerable<SelectOptionResponse>> GetTypes()
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);

        var types = Enum.GetValues<LogType>().ToList();
        if (user!.Staff < UserStaff.Management)
            types.RemoveAll(x => x == LogType.ICChat || x == LogType.ViewLogs);

        return types
           .Select(x => new SelectOptionResponse
           {
               Value = (byte)x,
               Label = x.GetDescription(),
           })
           .OrderBy(x => x.Label);
    }

    [HttpPost("search"), Authorize(Policy = PolicySettings.POLICY_LEAD_ADMIN)]
    public async Task<IEnumerable<LogResponse>> Search([FromBody] LogRequest request)
    {
        var query = context.Logs.AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(x => x.Type == request.Type);

        if (request.StartDate.HasValue)
            query = query.Where(x => x.RegisterDate.Date >= request.StartDate.Value.Date);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.RegisterDate.Date <= request.EndDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(request.OriginCharacter))
            query = query.Where(x => x.OriginCharacterId.HasValue && x.OriginCharacter!.Name.ToLower() == request.OriginCharacter.ToLower());

        if (!string.IsNullOrWhiteSpace(request.OriginIp))
            query = query.Where(x => x.OriginIp == request.OriginIp);

        if (!string.IsNullOrWhiteSpace(request.OriginUser))
            query = query.Where(x => x.OriginUserId.HasValue && x.OriginUser!.DiscordUsername.ToLower() == request.OriginUser.ToLower());

        if (!string.IsNullOrWhiteSpace(request.OriginSocialClubName))
            query = query.Where(x => x.OriginSocialClubName == request.OriginSocialClubName);

        if (!string.IsNullOrWhiteSpace(request.TargetCharacter))
            query = query.Where(x => x.TargetCharacterId.HasValue && x.TargetCharacter!.Name.ToLower() == request.TargetCharacter.ToLower());

        if (!string.IsNullOrWhiteSpace(request.TargetIp))
            query = query.Where(x => x.TargetIp == request.TargetIp);

        if (!string.IsNullOrWhiteSpace(request.TargetUser))
            query = query.Where(x => x.TargetCharacterId.HasValue && x.TargetCharacter!.User!.DiscordUsername.ToLower() == request.TargetUser.ToLower());

        if (!string.IsNullOrWhiteSpace(request.TargetSocialClubName))
            query = query.Where(x => x.TargetSocialClubName == request.TargetSocialClubName);

        if (!string.IsNullOrWhiteSpace(request.Description))
            query = query.Where(x => x.Description.ToLower().Contains(request.Description.ToLower()));

        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
        if (user!.Staff < UserStaff.Management)
        {
            var blockedTypes = new List<LogType> { LogType.ICChat, LogType.ViewLogs };
            query = query.Where(x => !blockedTypes.Contains(x.Type));
        }

        var logs = await query
            .Include(x => x.OriginCharacter)
            .Include(x => x.OriginUser)
            .Include(x => x.TargetCharacter)
                .ThenInclude(x => x!.User)
            .OrderByDescending(x => x.RegisterDate)
            .Take(100)
            .ToListAsync();

        await WriteLog(LogType.ViewLogs, $"Tipo: {request.Type?.GetDescription()} | {Serialize(request)}");

        return logs.Select(x => new LogResponse
        {
            Id = x.Id,
            Type = x.Type.GetDescription(),
            Date = x.RegisterDate,
            Description = x.Description,
            OriginCharacter = x.OriginCharacterId.HasValue ? x.OriginCharacter!.Name : string.Empty,
            OriginUser = x.OriginUserId.HasValue ? $"{x.OriginUser!.Name} ({x.OriginUser.DiscordUsername})" : string.Empty,
            OriginIp = x.OriginIp,
            OriginSocialClubName = x.OriginSocialClubName,
            TargetCharacter = x.TargetCharacterId.HasValue ? $"{x.TargetCharacter!.Name} ()" : string.Empty,
            TargetUser = x.TargetCharacterId.HasValue ? $"{x.TargetCharacter!.User!.Name} ({x.TargetCharacter.User.DiscordUsername})" : string.Empty,
            TargetIp = x.TargetIp,
            TargetSocialClubName = x.TargetSocialClubName,
        });
    }
}