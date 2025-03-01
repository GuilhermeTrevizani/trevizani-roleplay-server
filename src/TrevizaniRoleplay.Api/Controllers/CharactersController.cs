using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Server;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("characters")]
public class CharactersController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("patrimony"), Authorize(Policy = PolicySettings.POLICY_SERVER_MANAGER)]
    public async Task<IEnumerable<CharacterPatrimonyResponse>> GetPatrimony()
    {
        var characters = await context.Characters
            .Include(x => x.Faction)
            .Include(x => x.FactionRank)
            .Include(x => x.User)
            .Include(x => x.Items!)
                .ThenInclude(x => x.ItemTemplate)
            .Include(x => x.Properties!)
                .ThenInclude(x => x.Items!)
                    .ThenInclude(x => x.ItemTemplate)
            .Include(x => x.Vehicles!.Where(x => !x.Sold))
                .ThenInclude(x => x.Items!)
                    .ThenInclude(x => x.ItemTemplate)
            .Include(x => x.Companies)
            .Where(x => x.NameChangeStatus != CharacterNameChangeStatus.Done)
            .AsSplitQuery()
            .ToListAsync();

        var dealershipsVehicles = await context.DealershipsVehicles.ToListAsync();

        var response = new List<CharacterPatrimonyResponse>();
        foreach (var character in characters)
        {
            var patrimony = new CharacterPatrimonyResponse
            {
                User = $"{character.User!.Name} ({character.User.DiscordUsername})",
                Name = character.Name,
                Value = character.Bank,
                ConnectedTime = character.ConnectedTime,
                Properties = string.Join(", ", character.Properties!.Select(x => $"{x.FormatedAddress} (ID: {x.Number}) (${x.Value:N0})")),
                Vehicles = string.Join(", ", character.Vehicles!.Select(x => $"{x.Model} {x.Plate} (${dealershipsVehicles.FirstOrDefault(y => y.Model.ToLower() == x.Model.ToLower())?.Value ?? 0:N0})")),
                Companies = string.Join(", ", character.Companies!.Select(x => $"{x.Name} (Cofre: ${x.Safe:N0})")),
                Job = "-",
                Description = $"Dinheiro no Banco: ${character.Bank:N0}<br/>",
            };

            if (character.Faction is not null)
                patrimony.Job = $"{character.Faction.Name} {character.FactionRank!.Name} (${character.FactionRank.Salary:N0})";
            else if (character.Job != CharacterJob.Unemployed)
                patrimony.Job = character.Job.GetDescription();

            var money = character.Items!
                .Where(x => x.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID))
                .Sum(x => x.Quantity);
            patrimony.Description += $"Dinheiro no Inventário: ${money:N0}<br/>";
            patrimony.Value += money;

            var propertiesValue = character.Properties!.Sum(x => x.Value);
            patrimony.Description += $"Valor das Propriedades: ${propertiesValue:N0}<br/>";
            patrimony.Value += propertiesValue;

            var vehiclesValue = character.Vehicles!
                .Sum(x => dealershipsVehicles.FirstOrDefault(y => y.Model.ToLower() == x.Model.ToLower())?.Value ?? 0);
            patrimony.Description += $"Valor dos Veículos: ${vehiclesValue:N0}<br/>";
            patrimony.Value += vehiclesValue;

            var companiesSafeValue = character.Companies!.Sum(x => x.Safe);
            patrimony.Description += $"Valor nos Cofres das Empresas: ${companiesSafeValue:N0}<br/>";
            patrimony.Value += companiesSafeValue;

            if (character.DeletedDate.HasValue)
                patrimony.Description += "Excluído<br/>";

            if (character.DeathDate.HasValue)
                patrimony.Description += "Morto<br/>";

            foreach (var property in character.Properties!)
            {
                if (property.Items!.Count == 0)
                    continue;

                patrimony.Description += $"<br/><br/>Armazenamento da Propriedade {property.FormatedAddress} (ID: {property.Number})<br/>";
                patrimony.Description += string.Join("<br/>", property.Items.Select(x => $"{x.Quantity}x {x.ItemTemplate!.Name}"));
                patrimony.Value += property.Items.Where(x => x.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID)).Sum(x => x.Quantity);
            }

            foreach (var vehicle in character.Vehicles!)
            {
                if (vehicle.Items!.Count == 0)
                    continue;

                patrimony.Description += $"<br/><br/>Armazenamento do Veículo {vehicle.Plate}<br/>";
                patrimony.Description += string.Join("<br/>", vehicle.Items.Select(x => $"{x.Quantity}x {x.ItemTemplate!.Name}"));
                patrimony.Value += vehicle.Items.Where(x => x.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID)).Sum(x => x.Quantity);
            }

            response.Add(patrimony);
        }

        response = [.. response.OrderByDescending(x => x.Value)];
        response.ForEach(x => x.Position = response.IndexOf(x) + 1);
        return response;
    }

    [HttpGet("mine")]
    public async Task<MyCharactersResponse> GetMine()
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);

        var response = new MyCharactersResponse
        {
            Characters = await context.Characters
            .Where(x => x.UserId == UserId
                && x.NameChangeStatus != CharacterNameChangeStatus.Done
                && !x.DeletedDate.HasValue)
            .OrderBy(x => x.Name)
            .Select(x => new MyCharactersResponse.Character
            {
                Id = x.Id,
                Name = x.Name,
                LastAccessDate = x.LastAccessDate,
                CanApplyNamechange = user!.NameChanges > 0
                    && x.NameChangeStatus == CharacterNameChangeStatus.Allowed
                    && string.IsNullOrWhiteSpace(x.RejectionReason)
                    && x.EvaluatorStaffUserId.HasValue,
                CanResendApplication = !string.IsNullOrWhiteSpace(x.RejectionReason),
                Status = x.GetStatus(),
                DeathReason = x.DeathReason,
            })
            .ToListAsync()
        };

        var parameter = await context.Parameters.FirstOrDefaultAsync();
        if (parameter!.WhoCanLogin == WhoCanLogin.OnlyStaff && user!.Staff < UserStaff.ServerSupport)
            response.CreateCharacterWarning = "Apenas staff pode criar personagens no momento.";
        else if (response.Characters.Count() >= user!.CharacterSlots)
            response.CreateCharacterWarning = "Você não possui slot de personagens.";

        return response;
    }

    [HttpGet("create-character-info/{id}")]
    public async Task<CreateCharacterInfoResponse> GetCreateCharacterInfo(Guid id)
    {
        var character = await context.Characters
            .Include(x => x.EvaluatorStaffUser)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId && !x.DeletedDate.HasValue)
            ??
            throw new ArgumentException("Personagem não encontrado.");

        if (character.Connected)
            throw new ArgumentException("Personagem está conectado no servidor.");

        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId)!;
        if (string.IsNullOrWhiteSpace(character.RejectionReason)
            && user!.NameChanges == 0)
            throw new ArgumentException("Personagem não foi rejeitado e você não possui mudanças de nome.");

        return new()
        {
            Name = character.Name,
            Age = character.Age,
            History = character.History,
            RejectionReason = character.RejectionReason,
            Sex = character.Sex,
            Staffer = character.EvaluatorStaffUser?.Name,
        };
    }

    [HttpPost]
    public async Task Create([FromBody] CreateCharacterRequest request)
    {
        request.History = (request.History ?? string.Empty).Trim();
        if (request.History.Length < 500 || request.History.Length > 4096)
            throw new ArgumentException($"História deve possuir entre 500 e 4096 caracteres. Quantidade atual de caracteres: {request.History.Length}.");

        request.Name = (request.Name ?? string.Empty).Trim();
        if (request.Name.Length < 3 || request.Name.Length > 25)
            throw new ArgumentException("Nome do personagem deve possuir entre 3 e 25 caracteres.");

        if (request.Age < Constants.MIN_AGE || request.Age > Constants.MAX_AGE)
            throw new ArgumentException($"Idade deve ser entre {Constants.MIN_AGE} e {Constants.MAX_AGE}.");

        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId)!;

        var parameter = await context.Parameters.FirstOrDefaultAsync();
        if (parameter!.WhoCanLogin == WhoCanLogin.OnlyStaff && user!.Staff < UserStaff.ServerSupport)
            throw new ArgumentException("Apenas staff pode criar personagens no momento.");

        var isNew = true;
        Character? oldCharacter = null;
        if (request.Id.HasValue)
        {
            oldCharacter = await context.Characters.FirstOrDefaultAsync(x => x.Id == request.Id && !x.DeletedDate.HasValue);
            if (oldCharacter is null)
                throw new ArgumentException("Personagem antigo não encontrado.");

            if (oldCharacter.Connected)
                throw new ArgumentException("Personagem está conectado no servidor.");

            if (string.IsNullOrWhiteSpace(oldCharacter.RejectionReason))
            {
                if (oldCharacter.NameChangeStatus != CharacterNameChangeStatus.Allowed)
                    throw new ArgumentException("Você não pode alterar o nome desse personagem.");
            }
            else
            {
                isNew = false;
            }
        }

        if (await context.Characters.AnyAsync(x => x.Name == request.Name && x.Id != request.Id))
            throw new ArgumentException($"Personagem {request.Name} já existe.");

        var model = request.Sex == CharacterSex.Man ? 1885233650u : 2627665880u;
        var character = new Character();
        if (isNew)
        {
            var charactersCount = await context.Characters.CountAsync(x => x.UserId == user!.Id
                && !x.DeletedDate.HasValue
                && x.NameChangeStatus != CharacterNameChangeStatus.Done);
            if (oldCharacter is not null)
                charactersCount--;

            if (charactersCount >= user!.CharacterSlots)
                throw new ArgumentException("Você não possui slot de personagens.");

            var realCharactersCount = await context.Characters.CountAsync(x => x.UserId == user.Id
                && x.NameChangeStatus != CharacterNameChangeStatus.Done);
            if (oldCharacter is not null)
                realCharactersCount--;

            var initialHelpHours = realCharactersCount < 3 ? (oldCharacter?.InitialHelpHours ?? 20) : 0;

            var bloodType = Enum.GetValues<CharacterBloodType>().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            var bankAccount = await context.Characters.CountAsync();
            bankAccount++;
            character.Create(request.Name, request.Age, request.History, request.Sex,
                UserId, Ip, model, Constants.MAX_HEALTH,
                user.Staff >= UserStaff.JuniorServerAdmin ? user.Id : null,
                bankAccount, bloodType, initialHelpHours,
                Constants.INITIAL_SPAWN_POSITION_X, Constants.INITIAL_SPAWN_POSITION_Y, Constants.INITIAL_SPAWN_POSITION_Z);
            if (oldCharacter is not null)
            {
                character.SetBank(oldCharacter!.Bank);
                character.SetPremiumDate(oldCharacter.Premium, oldCharacter.PremiumValidDate);
            }
            await context.Characters.AddAsync(character);

            if (oldCharacter is null)
            {
                var characterItem = new CharacterItem();
                characterItem.Create(new Guid(Constants.MONEY_ITEM_TEMPLATE_ID), 0, 5_000, string.Empty);
                characterItem.SetSlot(1);
                characterItem.SetCharacterId(character.Id);
                await context.CharactersItems.AddAsync(characterItem);
            }
        }
        else
        {
            character = oldCharacter!;
            character.UpdateApplication(request.Name, request.Age, request.History, request.Sex, model);
            context.Characters.Update(character);
        }

        var namechange = oldCharacter is not null && isNew;

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.CreateCharacter, user!.Id,
            Serialize(new UCPActionCreateCharacterRequest
            {
                SendStaffNotification = character.EvaluatorStaffUserId is null,
                Namechange = namechange,
                OldCharacterId = oldCharacter?.Id,
                NewCharacterId = character.Id,
            }));

        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        if (namechange)
        {
            await WriteLog(LogType.NameChange, $"{oldCharacter!.Name} ({oldCharacter.Id}) > {character.Name} ({character.Id})");

            await context.CharactersItems.Where(x => x.CharacterId == oldCharacter.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.CharacterId, character.Id));
            await context.Companies.Where(x => x.CharacterId == oldCharacter.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.CharacterId, character.Id));
            await context.CompaniesCharacters.Where(x => x.CharacterId == oldCharacter.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.CharacterId, character.Id));

            oldCharacter.SetNameChangeStatus(CharacterNameChangeStatus.Done);
            context.Characters.Update(oldCharacter);
            await context.SaveChangesAsync();
        }
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id)
    {
        var date = DateTime.Now.AddDays(-15);
        if (await context.Characters.AnyAsync(x => x.UserId == UserId && x.DeletedDate.HasValue && x.DeletedDate >= date))
            throw new ArgumentException("Você já deletou um personagem no período de 15 dias.");

        var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId && !x.DeletedDate.HasValue)
            ??
            throw new ArgumentException("Personagem não encontrado.");

        if (character.Connected)
            throw new ArgumentException("Personagem está conectado no servidor.");

        character.Delete();
        context.Characters.Update(character);
        await context.SaveChangesAsync();
        await WriteLog(LogType.CharacterDelete, $"{character.Name} ({character.Id})");
    }
}