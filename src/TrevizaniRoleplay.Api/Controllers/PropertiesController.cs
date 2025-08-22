using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
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
                FactionName = x.Faction!.Name,
                CompanyName = x.Company!.Name,
                Name = x.Name ?? string.Empty,
                Owner = x.Character!.Name,
                ParentPropertyNumber = x.ParentProperty!.Number,
            })
            .ToListAsync();
        return properties;
    }

    [HttpPost("change-access")]
    public async Task ChangeAccess([FromBody] CharacterResponse.Property request)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(x => x.Id == request.Id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var characters = await GetUserCharacters();

        if (!characters.Any(x => x.Id == property.CharacterId))
            throw new ArgumentException(Resources.YouAreNotTheOwnerOfTheProperty);

        var charactersProperties = new List<CharacterProperty>();

        foreach (var characterName in request.CharactersWithAccess)
        {
            var selectedCharacter = await context.Characters
                .WhereActive()
                .FirstOrDefaultAsync(x => x.Name.ToLower() == characterName.ToLower())
                    ?? throw new ArgumentException(string.Format(Resources.CharacterNotFound, characterName));

            var characterProperty = new CharacterProperty();
            characterProperty.Create(selectedCharacter.Id, property.Id);

            charactersProperties.Add(characterProperty);
        }

        await context.CharactersProperties.Where(x => x.PropertyId == property.Id).ExecuteDeleteAsync();

        if (charactersProperties.Count > 0)
            await context.CharactersProperties.AddRangeAsync(charactersProperties);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadCharactersAcess, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }
}