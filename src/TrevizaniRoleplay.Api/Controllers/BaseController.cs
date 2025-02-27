using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[ApiController, Authorize]
public abstract class BaseController(DatabaseContext context) : ControllerBase
{
    protected Guid UserId => new(HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == "Id")?.Value!);
    protected string Ip => HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

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
        var log = new Log();
        log.Create(type, description,
            null, Ip, string.Empty,
            null, string.Empty, string.Empty);
        await context.Logs.AddAsync(log);
        await context.SaveChangesAsync();
    }
}