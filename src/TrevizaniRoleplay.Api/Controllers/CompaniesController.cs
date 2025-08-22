using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("companies")]
public class CompaniesController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("safe-movements/{id}")]
    public async Task<IEnumerable<SafeMovementResponse>> GetSafeMovementsByCompanyId(Guid id)
    {
        var company = await context.Companies
            .Include(x => x.Characters)
            .Include(x => x.SafeMovements!)
                .ThenInclude(x => x.Character)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (company is null)
            return [];

        var characters = await GetUserCharacters();

        if (!characters.Any(x => HasCompanySafeAccess(company!, x.Id)))
            return [];

        return company.SafeMovements!.OrderByDescending(x => x.RegisterDate).Select(x => new SafeMovementResponse
        {
            Date = x.RegisterDate,
            Type = x.Type.GetDescription(),
            Value = x.Value,
            Description = x.Description,
            Character = x.Character!.Name,
        });
    }
}