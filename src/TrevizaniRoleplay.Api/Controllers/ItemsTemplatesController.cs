using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Core.Services;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("items-templates")]
public class ItemsTemplatesController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("options")]
    public Task<List<ItemTemplateSelectOptionResponse>> GetOptions()
    {
        return context.ItemsTemplates.OrderBy(x => x.Name)
            .Select(x => new ItemTemplateSelectOptionResponse
            {
                Value = x.Id,
                Label = x.Name,
                IsStack = GlobalFunctions.CheckIfIsStack(x.Category),
            })
            .ToListAsync();
    }

    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_ITEMS)]
    public async Task<IEnumerable<ItemTemplateResponse>> Get()
    {
        static string GetType(ItemTemplate itemTemplate)
        {
            if (itemTemplate.Category == ItemCategory.Weapon)
                return GlobalFunctions.GetWeaponName(itemTemplate.Type);

            if (itemTemplate.Category == ItemCategory.Boombox
                || itemTemplate.Category == ItemCategory.WeaponComponent)
                return itemTemplate.Type.ToString();

            return string.Empty;
        }

        return (await context.ItemsTemplates
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync())
            .Select(x => new ItemTemplateResponse
            {
                Id = x.Id,
                Category = x.Category,
                CategoryDisplay = x.Category.GetDescription(),
                Type = GetType(x),
                Name = x.Name,
                Weight = x.Weight,
                Image = x.Image,
                ObjectModel = x.ObjectModel,
            });
    }

    [HttpGet("categories"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_ITEMS)]
    public IEnumerable<ItemCategoryResponse> GetCategories()
    {
        return Enum.GetValues<ItemCategory>()
            .Select(category => new ItemCategoryResponse
            {
                Value = category,
                Label = category.GetDescription(),
                HasType = category == ItemCategory.Weapon || category == ItemCategory.Boombox || category == ItemCategory.WeaponComponent,
            })
            .OrderBy(x => x.Label);
    }

    [HttpPost, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_ITEMS)]
    public async Task Save([FromBody] ItemTemplateRequest request)
    {
        if (request.Name.Length < 1 || request.Name.Length > 50)
            throw new ArgumentException("Nome deve ter entre 1 e 50 caracteres.");

        if (request.Image.Length < 1 || request.Image.Length > 50)
            throw new ArgumentException("Imagem deve ter entre 1 e 50 caracteres.");

        request.ObjectModel ??= string.Empty;
        if (request.ObjectModel.Length > 50)
            throw new ArgumentException("Objeto deve ter entre até 50 caracteres.");

        if (!GlobalFunctions.IsValidImageUrl(request.Image))
            throw new ArgumentException("Imagem inválida.");

        if (request.Weight <= 0)
            throw new ArgumentException("Peso deve ser maior que 0.");

        _ = uint.TryParse(request.Type, out uint type);

        if (request.Category == ItemCategory.Weapon)
        {
            type = GlobalFunctions.GetWeaponType(request.Type);
            if (type == 0)
                throw new ArgumentException($"Arma {request.Type} não existe.");
        }
        else if (request.Category == ItemCategory.Boombox)
        {
            if (type <= 0)
                throw new ArgumentException("Tipo deve ser maior que 0.");
        }
        else if (request.Category == ItemCategory.WeaponComponent)
        {
            if (type <= 0)
                throw new ArgumentException("Tipo deve ser maior que 0.");
        }
        else
        {
            if (type != 0)
                throw new ArgumentException("Tipo deve ser 0.");
        }

        if (await context.ItemsTemplates.AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower()))
            throw new ArgumentException($"Já existe o item {request.Name}.");

        var isNew = !request.Id.HasValue;
        var itemTemplate = new ItemTemplate();
        if (isNew)
        {
            if (request.Category == ItemCategory.Money || request.Category == ItemCategory.VehiclePart
                || request.Category == ItemCategory.BloodSample || GlobalFunctions.CheckIfIsAmmo(request.Category))
                throw new ArgumentException($"{request.Category.GetDescription()} só pode ter um item.");

            itemTemplate.Create(request.Category, type, request.Name, request.Weight, request.Image, request.ObjectModel);
        }
        else
        {
            itemTemplate = await context.ItemsTemplates.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (itemTemplate is null)
                throw new ArgumentException(Resources.RecordNotFound);

            itemTemplate.Update(type, request.Name, request.Weight, request.Image, request.ObjectModel);
        }

        if (isNew)
            await context.ItemsTemplates.AddAsync(itemTemplate);
        else
            context.ItemsTemplates.Update(itemTemplate);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadItemsTemplates, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Item | {Serialize(itemTemplate)}");
    }
}