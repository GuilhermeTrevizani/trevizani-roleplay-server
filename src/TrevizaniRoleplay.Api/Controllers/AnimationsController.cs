using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("animations")]
public class AnimationsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_ANIMATIONS)]
    public Task<List<AnimationResponse>> GetAll()
    {
        return context.Animations
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new AnimationResponse
            {
                Id = x.Id,
                Display = x.Display,
                Dictionary = x.Dictionary,
                Name = x.Name,
                Flag = x.Flag,
                OnlyInVehicle = x.OnlyInVehicle,
                Category = x.Category,
                Scenario = x.Scenario,
            })
            .ToListAsync();
    }

    [HttpPost, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_ANIMATIONS)]
    public async Task CreateOrUpdate([FromBody] AnimationResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Display))
            throw new ArgumentException("Opção é obrigatória.");

        if (string.IsNullOrWhiteSpace(response.Scenario))
        {
            if (string.IsNullOrWhiteSpace(response.Dictionary))
                throw new ArgumentException("Dicionário é obrigatório.");

            if (string.IsNullOrWhiteSpace(response.Name))
                throw new ArgumentException("Nome é obrigatório.");
        }
        else
        {
            response.Dictionary = response.Name = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(response.Category))
            throw new ArgumentException("Categoria é obrigatória.");

        if (await context.Animations.AnyAsync(x => x.Name.ToLower() == response.Display.ToLower() && x.Id != response.Id))
            throw new ArgumentException($"{response.Display} já existe.");

        var isNew = !response.Id.HasValue;
        var animation = new Animation();
        if (isNew)
        {
            animation.Create(response.Display, response.Dictionary, response.Name, response.Flag,
                response.OnlyInVehicle, response.Category, response.Scenario);
        }
        else
        {
            animation = await context.Animations.FirstOrDefaultAsync(x => x.Id == response.Id);
            if (animation is null)
                throw new ArgumentException(Globalization.RECORD_NOT_FOUND);

            animation.Update(response.Display, response.Dictionary, response.Name, response.Flag,
                response.OnlyInVehicle, response.Category, response.Scenario);
        }

        if (isNew)
            await context.Animations.AddAsync(animation);
        else
            context.Animations.Update(animation);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadAnimations, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Animação | {Serialize(animation)}");
    }

    [HttpDelete("{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_ANIMATIONS)]
    public async Task Delete(Guid id)
    {
        var animation = await context.Animations.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Globalization.RECORD_NOT_FOUND);

        context.Animations.Remove(animation);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadAnimations, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
        await WriteLog(LogType.Staff, $"Remover Animação | {Serialize(animation)}");
    }
}