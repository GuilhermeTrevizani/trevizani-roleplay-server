using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("furnitures")]
public class FurnituresController(DatabaseContext context) : BaseController(context)
{
    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FURNITURES)]
    public Task<List<FurnitureResponse>> GetAll()
    {
        return context.Furnitures
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new FurnitureResponse
            {
                Id = x.Id,
                AudioOutput = x.AudioOutput,
                Category = x.Category,
                Door = x.Door,
                Model = x.Model,
                Name = x.Name,
                Subcategory = x.Subcategory,
                TVTexture = x.TVTexture,
                Value = x.Value,
                UseSlot = x.UseSlot,
            })
            .ToListAsync();
    }

    [HttpPost, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FURNITURES)]
    public async Task CreateOrUpdate([FromBody] FurnitureResponse response)
    {
        if (response.Category.ToLower() != Resources.Barriers && response.Value <= 0)
            throw new ArgumentException("Valor inválido.");

        var isNew = !response.Id.HasValue;
        var furniture = new Furniture();
        if (isNew)
        {
            furniture.Create(response.Category, response.Name, response.Model, response.Value, response.Door,
                response.AudioOutput, response.TVTexture, response.Subcategory, response.UseSlot);
        }
        else
        {
            furniture = await context.Furnitures.FirstOrDefaultAsync(x => x.Id == response.Id);
            if (furniture is null)
                throw new ArgumentException(Resources.RecordNotFound);

            furniture.Update(response.Category, response.Name, response.Model, response.Value, response.Door,
                response.AudioOutput, response.TVTexture, response.Subcategory, response.UseSlot);
        }

        if (isNew)
            await context.Furnitures.AddAsync(furniture);
        else
            context.Furnitures.Update(furniture);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFurnitures, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Mobília | {Serialize(furniture)}");
    }

    [HttpDelete("{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FURNITURES)]
    public async Task Delete(Guid id)
    {
        var furniture = await context.Furnitures.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        context.Furnitures.Remove(furniture);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFurnitures, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
        await WriteLog(LogType.Staff, $"Remover Mobília | {Serialize(furniture)}");
    }
}