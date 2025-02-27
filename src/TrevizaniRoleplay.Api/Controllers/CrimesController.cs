using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("crimes")]
public class CrimesController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<CrimeResponse>> GetAll()
    {
        return context.Crimes
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new CrimeResponse
            {
                Id = x.Id,
                Name = x.Name,
                PrisonMinutes = x.PrisonMinutes,
                FineValue = x.FineValue,
                DriverLicensePoints = x.DriverLicensePoints,
            })
            .ToListAsync();
    }

    [HttpPost, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task CreateOrUpdate([FromBody] CrimeResponse response)
    {
        if (response.Name.Length < 1 || response.Name.Length > 100)
            throw new ArgumentException("Nome deve ter entre 1 e 100 caracteres.");

        if (response.PrisonMinutes < 0)
            throw new ArgumentException("Tempo de Prisão em Minutos deve ser maior ou igual a 0.");

        if (response.FineValue < 0)
            throw new ArgumentException("Valor da Multa deve ser maior ou igual a 0.");

        if (response.DriverLicensePoints < 0)
            throw new ArgumentException("Pontos na Licença de Motorista deve ser maior ou igual a 0.");

        if (await context.Crimes.AnyAsync(x => x.Name.ToLower() == response.Name.ToLower() && x.Id != response.Id))
            throw new ArgumentException($"{response.Name} já existe.");

        var isNew = !response.Id.HasValue;
        var crime = new Crime();
        if (isNew)
        {
            crime.Create(response.Name, response.PrisonMinutes, response.FineValue, response.DriverLicensePoints);
        }
        else
        {
            crime = await context.Crimes.FirstOrDefaultAsync(x => x.Id == response.Id);
            if (crime is null)
                throw new ArgumentException(Globalization.RECORD_NOT_FOUND);

            crime.Update(response.Name, response.PrisonMinutes, response.FineValue, response.DriverLicensePoints);
        }

        if (isNew)
            await context.Crimes.AddAsync(crime);
        else
            context.Crimes.Update(crime);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadCrimes, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Crime | {Serialize(crime)}");
    }

    [HttpDelete("{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task Delete(Guid id)
    {
        var crime = await context.Crimes.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Globalization.RECORD_NOT_FOUND);

        context.Crimes.Remove(crime);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadCrimes, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
        await WriteLog(LogType.Staff, $"Remover Crime | {Serialize(crime)}");
    }
}