using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("administrative-punishments")]
public class AdministrativePunishmentsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet]
    public async Task<IEnumerable<AdministrativePunishmentResponse>> Get()
    {
        return (await context.Punishments
            .Include(x => x.Character)
            .Include(x => x.StaffUser)
            .Where(x => x.Character!.UserId == UserId)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync())
            .Select(x => new AdministrativePunishmentResponse
            {
                Character = x.Character!.Name,
                Date = x.RegisterDate,
                Type = x.Type.ToString(),
                Duration = x.Type == PunishmentType.Ban ? (x.Duration > 0 ? $"{x.Duration} dia{(x.Duration != 1 ? "s" : string.Empty)}" : "Permanente") : string.Empty,
                Staffer = x.StaffUser!.Name,
                Reason = x.Reason,
            });
    }
}