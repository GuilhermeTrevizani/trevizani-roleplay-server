using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("properties")]
public class PropertiesController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_PROPERTIES)]
    public async Task<IEnumerable<PropertyResponse>> Get()
    {
        var properties = await context.Properties
            .Include(x => x.Character)
            .Include(x => x.Faction)
            .Include(x => x.Company)
            .Include(x => x.ParentProperty)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new PropertyResponse
            {
                Id = x.Id,
                Number = x.Number,
                InteriorDisplay = x.Interior.ToString(),
                Address = x.Address,
                Value = x.Value,
                EntranceDimension = x.EntranceDimension,
                EntrancePosX = x.EntrancePosX,
                EntrancePosY = x.EntrancePosY,
                EntrancePosZ = x.EntrancePosZ,
                FactionName = x.Faction!.Name,
                CompanyName = x.Company!.Name,
                Name = x.Name ?? string.Empty,
                ExitPosX = x.ExitPosX,
                ExitPosY = x.ExitPosY,
                ExitPosZ = x.ExitPosZ,
                Owner = x.Character!.Name,
                ParentPropertyNumber = x.ParentProperty!.Number,
            })
            .ToListAsync();
        return properties;
    }
}