using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[ApiController, Authorize]
public abstract class BaseController(DatabaseContext context) : ControllerBase
{
    protected Guid UserId => new(User?.Claims?.FirstOrDefault(x => x.Type == "Id")?.Value!);

    protected string Ip => HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    protected Guid? CharacterId
    {
        get
        {
            var characterId = HttpContext.Request.Headers[nameof(CharacterId)].FirstOrDefault();
            return string.IsNullOrWhiteSpace(characterId) ? null : new Guid(characterId);
        }
    }

    private readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    protected string Serialize(object obj)
            => JsonSerializer.Serialize(obj, JsonSerializerOptions);

    protected T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);

    protected async Task WriteLog(LogType type, string description)
    {
        var userId = !string.IsNullOrWhiteSpace(User?.FindFirst("Id")?.Value) ? UserId : (Guid?)null;

        var log = new Log();
        log.Create(type, description,
            CharacterId, Ip, string.Empty, userId,
            null, string.Empty, string.Empty);
        await context.Logs.AddAsync(log);
        await context.SaveChangesAsync();
    }

    protected bool HasCompanySafeAccess(Company company, Guid characterId)
    {
        var companyCharacter = company.Characters!.FirstOrDefault(x => x.CharacterId == characterId);

        return company.CharacterId == characterId
            || Deserialize<CompanyFlag[]>(companyCharacter?.FlagsJSON ?? "[]")!.Contains(CompanyFlag.Safe);
    }

    protected Task<List<Character>> GetUserCharacters()
    {
        return context.Characters
            .WhereActive()
            .Where(x => x.UserId == UserId)
            .ToListAsync();
    }
}