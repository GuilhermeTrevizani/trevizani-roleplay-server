using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("vehicles")]
public class VehiclesController(DatabaseContext context) : BaseController(context)
{
    [HttpPost("change-access")]
    public async Task ChangeAccess([FromBody] CharacterResponse.Vehicle request)
    {
        var vehicle = await context.Vehicles
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.Sold)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var characters = await GetUserCharacters();

        if (!characters.Any(x => x.Id == vehicle.CharacterId))
            throw new ArgumentException(Resources.YouAreNotTheOwnerOfTheVehicle);

        var charactersVehicles = new List<CharacterVehicle>();

        foreach (var characterName in request.CharactersWithAccess)
        {
            var selectedCharacter = await context.Characters
                .WhereActive()
                .FirstOrDefaultAsync(x => x.Name.ToLower() == characterName.ToLower())
                    ?? throw new ArgumentException(string.Format(Resources.CharacterNotFound, characterName));

            var characterVehicle = new CharacterVehicle();
            characterVehicle.Create(selectedCharacter.Id, vehicle.Id);

            charactersVehicles.Add(characterVehicle);
        }

        await context.CharactersVehicles.Where(x => x.VehicleId == vehicle.Id).ExecuteDeleteAsync();

        if (charactersVehicles.Count > 0)
            await context.CharactersVehicles.AddRangeAsync(charactersVehicles);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadCharactersAcess, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }

    [HttpGet("models")]
    public IEnumerable<string> GetVehiclesMods()
    {
        return Enum.GetValues<VehicleModelMods>()
          .Select(x => x.ToString())
          .Order();
    }
}