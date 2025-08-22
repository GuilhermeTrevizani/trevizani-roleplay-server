using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("drugs")]
public class DrugsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_DRUGS)]
    public async Task<List<DrugResponse>> Get()
    {
        var hasNewDrugs = false;
        foreach (var itemTemplate in await context.ItemsTemplates.Where(x => x.Category == ItemCategory.Drug).ToListAsync())
        {
            var drug = await context.Drugs.FirstOrDefaultAsync(x => x.ItemTemplateId == itemTemplate.Id);
            if (drug is null)
            {
                drug ??= new Drug();
                drug.Create(itemTemplate.Id);
                await context.Drugs.AddAsync(drug);
                await context.SaveChangesAsync();
            }
        }

        if (hasNewDrugs)
        {
            var ucpAction = new UCPAction();
            ucpAction.Create(UCPActionType.ReloadDrugs, UserId, string.Empty);
            await context.UCPActions.AddAsync(ucpAction);
            await context.SaveChangesAsync();
        }

        return await context.Drugs
            .Select(x => new DrugResponse
            {
                Id = x.Id,
                Name = x.ItemTemplate!.Name,
                ThresoldDeath = x.ThresoldDeath,
                Health = x.Health,
                GarbageCollectorMultiplier = x.GarbageCollectorMultiplier,
                TruckerMultiplier = x.TruckerMultiplier,
                MinutesDuration = x.MinutesDuration,
                Warn = x.Warn,
                ShakeGameplayCamName = x.ShakeGameplayCamName,
                ShakeGameplayCamIntensity = x.ShakeGameplayCamIntensity,
                TimecycModifier = x.TimecycModifier,
                AnimpostFXName = x.AnimpostFXName,
            })
            .OrderByDescending(x => x.Name)
            .ToListAsync();
    }

    [HttpPost, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_DRUGS)]
    public async Task Save([FromBody] DrugRequest request)
    {
        var drug = await context.Drugs.FirstOrDefaultAsync(x => x.Id == request.Id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        drug.Update(request.ThresoldDeath, request.Health, request.GarbageCollectorMultiplier, request.TruckerMultiplier,
        request.MinutesDuration, request.Warn, request.ShakeGameplayCamName, request.ShakeGameplayCamIntensity,
        request.TimecycModifier, request.AnimpostFXName);
        context.Drugs.Update(drug);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadDrugs, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Droga | {Serialize(drug)}");
    }
}