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

[Route("factions")]
public class FactionsController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("mine")]
    public async Task<IEnumerable<MyFactionResponse>> GetMyFactions()
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        return await context.Factions
            .Where(x => factionsIds.Contains(x.Id))
            .Select(x => new MyFactionResponse
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync();
    }

    [HttpGet("mine/{id}")]
    public async Task<MyFactionDetailResponse> GetMyFaction(Guid id)
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(id))
            throw new ArgumentException(Resources.RecordNotFound);

        var userFlags = characters.Where(x => x.FactionId == id).SelectMany(x => Deserialize<FactionFlag[]>(x.FactionFlagsJSON)!).Distinct();

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        const int DAYS_INTERVAL_ON_DUTY_SESSION_AVERAGE = 14;
        MyFactionDetailResponse.Character GetCharacterInfo(Character x)
        {
            return new()
            {
                Id = x.Id,
                RankName = x.FactionRank!.Name,
                RankId = x.FactionRankId!.Value,
                Name = x.Name,
                User = x.User!.Name,
                LastAccessDate = x.LastAccessDate,
                Position = x.FactionRank.Position,
                Badge = x.Badge,
                IsOnline = x.Connected,
                FlagsJson = x.FactionFlagsJSON,
                AverageMinutesOnDutyLastTwoWeeks = x.Sessions
                    !.Where(y => y.Type == SessionType.FactionDuty && y.FinalDate.HasValue
                        && y.RegisterDate >= DateTime.Now.AddDays(-DAYS_INTERVAL_ON_DUTY_SESSION_AVERAGE))
                    .Sum(y => (y.FinalDate!.Value - y.RegisterDate).TotalMinutes) / DAYS_INTERVAL_ON_DUTY_SESSION_AVERAGE,
                Flags = Deserialize<FactionFlag[]>(x.FactionFlagsJSON)!.Select(x => x.GetDescription()).Order(),
            };
        }

        return new MyFactionDetailResponse
        {
            Id = faction.Id,
            Name = faction.Name,
            HasDuty = faction.HasDuty,
            Color = faction.Color,
            ChatColor = faction.ChatColor,
            UserFlags = userFlags,
            IsLeader = characters.Any(x => x.Id == faction.CharacterId),
            FlagsOptions = faction.GetFlags()
            .Select(x => new SelectOptionResponse
            {
                Value = (byte)x,
                Label = x.GetDescription(),
            })
            .OrderBy(x => x.Label),
            Characters = (await context.Characters.WhereActive(false)
            .Where(x => x.FactionId == faction.Id)
            .Include(x => x.User)
            .Include(x => x.FactionRank)
            .Include(x => x.Sessions)
            .AsSplitQuery()
            .ToListAsync())
            .Select(GetCharacterInfo)
            .OrderByDescending(x => x.IsOnline)
                .ThenByDescending(x => x.Position)
                    .ThenBy(x => x.Name),
            Ranks = await context.FactionsRanks.Where(x => x.FactionId == faction.Id)
            .OrderBy(x => x.Position)
            .Select(x => new MyFactionDetailResponse.Rank
            {
                Id = x.Id,
                Name = x.Name,
                Salary = x.Salary
            })
            .ToListAsync(),
            Vehicles = await context.Vehicles.Where(x => x.FactionId == faction.Id && !x.Sold)
            .Select(x => new MyFactionDetailResponse.Vehicle
            {
                Id = x.Id,
                Model = x.Model.ToUpper(),
                Plate = x.Plate,
                Description = x.Description,
            })
            .OrderBy(x => x.Model)
            .ToListAsync(),
        };
    }

    [HttpPost]
    public async Task Save([FromBody] SaveFactionRequest request)
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(request.Id))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == request.Id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (!characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        faction.UpdateColors(request.Color.Replace("#", string.Empty), request.ChatColor.Replace("#", string.Empty));
        context.Factions.Update(faction);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactions, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Faction, $"Gravar Facção | {Serialize(faction)}");
    }

    [HttpPost("save-vehicle")]
    public async Task FactionVehicleSave([FromBody] SaveFactionVehicleRequest request)
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(request.FactionId))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == request.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var userFlags = characters.Where(x => x.FactionId == request.FactionId).SelectMany(x => Deserialize<FactionFlag[]>(x.FactionFlagsJSON)!).Distinct();
        if (!userFlags.Contains(FactionFlag.ManageVehicles) && !characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == request.Id && x.FactionId == request.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (request.Description.Length > 100)
            throw new ArgumentException(Resources.DescriptionMustNotExceed100Characters);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionVehicle, UserId,
            Serialize(new UCPActionReloadFactionVehicleRequest
            {
                Id = request.Id,
                Description = request.Description,
            }));

        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();
    }

    [HttpPost("save-rank")]
    public async Task FactionRankSave([FromBody] SaveFactionRankRequest request)
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(request.FactionId))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == request.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (!characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        var isNew = !request.Id.HasValue;
        var factionRank = new FactionRank();
        if (isNew)
        {
            var position = (await context.FactionsRanks
                .Where(x => x.FactionId == faction.Id)
                .Select(x => x.Position)
                .DefaultIfEmpty(0)
                .MaxAsync()) + 1;

            factionRank.Create(faction.Id, position, request.Name, 0);
        }
        else
        {
            factionRank = await context.FactionsRanks.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (factionRank is null)
                throw new ArgumentException(Resources.RecordNotFound);

            factionRank.Update(request.Name);
        }

        if (isNew)
            await context.FactionsRanks.AddAsync(factionRank);
        else
            context.FactionsRanks.Update(factionRank);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsRanks, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Faction, $"Gravar Rank | {Serialize(factionRank)}");
    }

    [HttpDelete("remove-rank/{id}")]
    public async Task FactionRankRemove(Guid id)
    {
        var factionRank = await context.FactionsRanks.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(factionRank.FactionId))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == factionRank.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (!characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        var factionCharactersToRemove = await context.Characters
            .Where(x => x.FactionRankId == factionRank.Id
                && (x.DeathDate.HasValue || x.NameChangeStatus == CharacterNameChangeStatus.Done || x.DeletedDate.HasValue))
            .ToListAsync();

        foreach (var character in factionCharactersToRemove)
        {
            character.ResetFaction();
            context.Characters.Update(character);
            await context.SaveChangesAsync();
        }

        if (await context.Characters.AnyAsync(x => x.FactionRankId == factionRank.Id))
            throw new ArgumentException($"Não é possível remover o rank {factionRank.Name} pois existem personagens nele.");

        context.FactionsRanks.Remove(factionRank);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsRanks, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Faction, $"Remover Rank | {Serialize(factionRank)}");
    }

    [HttpPost("order-ranks")]
    public async Task FactionRankOrder([FromBody] OrderFactionRanksRequest request)
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(request.FactionId))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == request.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (!characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        var factionRanks = await context.FactionsRanks.Where(x => x.FactionId == faction.Id).ToListAsync();
        foreach (var rank in request.Ranks)
        {
            var factionRank = factionRanks.FirstOrDefault(x => x.Id == rank.Id);
            factionRank?.SetPosition(rank.Position);
        }

        context.FactionsRanks.UpdateRange(factionRanks);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsRanks, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Faction, $"Ordenar Ranks | {Serialize(request)}");
    }

    [HttpPost("save-member")]
    public async Task FactionMemberSave([FromBody] SaveFactionMemberRequest request)
    {
        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(request.FactionId))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == request.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var userFlags = characters.Where(x => x.FactionId == request.FactionId).SelectMany(x => Deserialize<FactionFlag[]>(x.FactionFlagsJSON)!).Distinct();
        if (!userFlags.Contains(FactionFlag.EditMember) && !characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == request.Id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (character.FactionId != faction.Id)
            throw new ArgumentException(Resources.RecordNotFound);

        var factionRanks = await context.FactionsRanks.Where(x => x.FactionId == faction.Id).ToListAsync();

        var factionRank = factionRanks.FirstOrDefault(x => x.Id == character.FactionRankId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (request.Badge < 0)
            throw new ArgumentException("Distintivo deve ser maior ou igual a zero.");

        if (request.Badge > 0)
        {
            var characterTarget = await context.Characters.WhereActive().FirstOrDefaultAsync(x => x.FactionId == faction.Id && x.Badge == request.Badge);
            if (characterTarget is not null && characterTarget.Id != character.Id)
                throw new ArgumentException($"Distintivo {request.Badge} está sendo usado por {characterTarget.Name}.");
        }

        var maxPosition = characters.Where(x => x.FactionId == request.FactionId).Select(x => factionRanks.FirstOrDefault(y => y.Id == x.FactionRankId)?.Position ?? 0).Max();

        if (factionRank.Position >= maxPosition && !characters.Any(x => x.Id == character.Id))
            throw new ArgumentException("Jogador possui um rank igual ou maior que o seu.");

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.SaveFactionMember, UserId, Serialize(new UCPActionSaveFactionMemberRequest
        {
            Id = request.Id,
            Badge = request.Badge,
            FactionRankId = request.FactionRankId,
            Flags = request.Flags,
        }));
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }

    [HttpDelete("remove-member/{id}")]
    public async Task FactionMemberRemove(Guid id)
    {
        var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var characters = await GetUserCharacters();

        var factionsIds = characters.Select(x => x.FactionId);

        if (!factionsIds.Contains(character.FactionId))
            throw new ArgumentException(Resources.RecordNotFound);

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == character.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var userFlags = characters.Where(x => x.FactionId == character.FactionId).SelectMany(x => Deserialize<FactionFlag[]>(x.FactionFlagsJSON)!).Distinct();
        if (!userFlags.Contains(FactionFlag.RemoveMember) && !characters.Any(x => x.Id == faction.CharacterId))
            throw new ArgumentException(Resources.YouAreNotAuthorizedToUseThisCommand);

        var factionRanks = await context.FactionsRanks.Where(x => x.FactionId == faction.Id).ToListAsync();

        var factionRank = factionRanks.FirstOrDefault(x => x.Id == character.FactionRankId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        var maxPosition = characters.Where(x => x.FactionId == character.FactionId).Select(x => factionRanks.FirstOrDefault(y => y.Id == x.FactionRankId)?.Position ?? 0).Max();
        if (factionRank.Position >= maxPosition && !characters.Any(x => x.Id == character.Id))
            throw new ArgumentException("Jogador possui um rank igual ou maior que o seu.");

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.RemoveFactionMember, UserId, Serialize(id));
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }

    [HttpGet, Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<FactionResponse>> Get()
    {
        return context.Factions
            .OrderBy(x => x.Name)
            .Select(x => new FactionResponse
            {
                Id = x.Id,
                Name = x.Name,
                ShortName = x.ShortName,
                Slots = x.Slots,
                Type = x.Type,
                TypeDisplay = x.Type.GetDescription(),
                Leader = x.Character!.Name,
            })
            .ToListAsync();
    }

    [HttpGet("types"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public IEnumerable<SelectOptionResponse> GetTypes()
    {
        return Enum.GetValues<FactionType>()
           .Select(x => new SelectOptionResponse
           {
               Value = (byte)x,
               Label = x.GetDescription(),
           })
           .OrderBy(x => x.Label);
    }

    [HttpPost("staff-save"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionSave([FromBody] FactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Nome não preenchido.");

        if (string.IsNullOrWhiteSpace(request.ShortName))
            throw new ArgumentException("Acrônimo não preenchido.");

        if (request.Slots < 0)
            throw new ArgumentException("Slots deve ser maior ou igual a zero.");

        if (await context.Factions.AnyAsync(x => x.Id != request.Id && x.Name.ToLower() == request.Name.ToLower()))
            throw new ArgumentException("Já existe uma facção com esse nome.");

        if (await context.Factions.AnyAsync(x => x.Id != request.Id && x.ShortName.ToLower() == request.ShortName.ToLower()))
            throw new ArgumentException("Já existe uma facção com esse acrônimo.");

        Character? leader = null;
        if (!string.IsNullOrWhiteSpace(request.Leader))
        {
            leader = await context.Characters.FirstOrDefaultAsync(x => x.Name == request.Leader);
            if (leader is null)
                throw new ArgumentException($"Personagem {request.Leader} não encontrado.");

            if (leader.FactionId.HasValue && leader.FactionId != request.Id)
                throw new ArgumentException($"{leader.Name} já está em uma facção");
        }

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.StaffSaveFaction, UserId, Serialize(request));
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();
    }

    [HttpGet("ranks/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<MyFactionDetailResponse.Rank>> GetRanks(Guid id)
    {
        return context.FactionsRanks
            .Where(x => x.FactionId == id)
            .OrderBy(x => x.Position)
            .Select(x => new MyFactionDetailResponse.Rank
            {
                Id = x.Id,
                Name = x.Name,
                Salary = x.Salary,
            })
            .ToListAsync();
    }

    [HttpGet("members/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<FactionMemberResponse>> GetMembers(Guid id)
    {
        return context.Characters
            .WhereActive(false)
            .Where(x => x.FactionId == id)
            .Include(x => x.User)
            .Include(x => x.FactionRank)
            .OrderByDescending(x => x.Connected)
                .ThenByDescending(x => x.FactionRank!.Position)
                    .ThenBy(x => x.Name)
            .Select(x => new FactionMemberResponse
            {
                RankName = x.FactionRank!.Name,
                Name = x.Name,
                User = x.User!.Name,
                LastAccessDate = x.LastAccessDate,
                IsOnline = x.Connected,
            })
            .ToListAsync();
    }

    [HttpGet("equipments/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<FactionEquipmentResponse>> GetEquipments(Guid id)
    {
        return context.FactionsEquipments
            .Where(x => x.FactionId == id)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new FactionEquipmentResponse
            {
                Id = x.Id,
                Name = x.Name,
                PropertyOrVehicle = x.PropertyOrVehicle,
                SWAT = x.SWAT,
                UPR = x.UPR,
            })
            .ToListAsync();
    }

    [HttpGet("equipment-items/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task<IEnumerable<FactionEquipmentItemResponse>> GetEquipmentItems(Guid id)
    {
        return (await context.FactionsEquipmentsItems
            .Where(x => x.FactionEquipmentId == id)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync())
            .Select(x => new FactionEquipmentItemResponse
            {
                Id = x.Id,
                Weapon = x.Weapon,
                Ammo = x.Ammo,
                Components = Deserialize<IEnumerable<uint>>(x.ComponentsJson)!,
            });
    }

    [HttpGet("vehicles/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<FactionVehicleResponse>> GetVehicles(Guid id)
    {
        return context.Vehicles
            .Where(x => !x.Sold && x.FactionId == id)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new FactionVehicleResponse
            {
                Id = x.Id,
                Model = x.Model,
                Plate = x.Plate,
                Description = x.Description,
            })
            .ToListAsync();
    }

    [HttpGet("frequencies/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public Task<List<FactionFrequencyResponse>> GetFrequencies(Guid id)
    {
        return context.FactionsFrequencies
            .Where(x => x.FactionId == id)
            .OrderBy(x => x.Frequency)
            .Select(x => new FactionFrequencyResponse
            {
                Id = x.Id,
                Name = x.Name,
                Frequency = x.Frequency,
            })
            .ToListAsync();
    }

    [HttpPost("staff-save-rank"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionRankSave([FromBody] FactionRankRequest request)
    {
        if (request.Salary < 0)
            throw new ArgumentException("Salário deve ser maior ou igual a 0.");

        var factionRank = await context.FactionsRanks.FirstOrDefaultAsync(x => x.Id == request.Id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        factionRank.Update(request.Salary);

        context.FactionsRanks.Update(factionRank);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsRanks, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Rank | {Serialize(factionRank)}");
    }

    [HttpDelete("staff-remove-equipment/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionEquipmentRemove(Guid id)
    {
        var factionEquipment = await context.FactionsEquipments.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (factionEquipment.Items!.Count > 0)
            context.FactionsEquipmentsItems.RemoveRange(factionEquipment.Items);

        context.FactionsEquipments.Remove(factionEquipment);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsEquipments, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Remover Equipamento | {Serialize(factionEquipment)}");
    }

    [HttpPost("staff-save-equipment"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionEquipmentSave([FromBody] FactionEquipmentRequest request)
    {
        request.Name ??= string.Empty;
        if (request.Name.Length < 1 || request.Name.Length > 25)
            throw new ArgumentException("Nome deve ter entre 1 e 25 caracteres.");

        if (await context.FactionsEquipments.AnyAsync(x => x.Id != request.Id && x.FactionId == request.FactionId && x.Name.ToLower() == request.Name.ToLower()))
            throw new ArgumentException("Já existe um registro com esse nome para essa facção.");

        var isNew = !request.Id.HasValue;
        var factionEquipment = new FactionEquipment();
        if (isNew)
        {
            factionEquipment.Create(request.FactionId, request.Name, request.PropertyOrVehicle, request.SWAT, request.UPR);
        }
        else
        {
            factionEquipment = await context.FactionsEquipments.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (factionEquipment is null)
                throw new ArgumentException(Resources.RecordNotFound);

            factionEquipment.Update(request.Name, request.PropertyOrVehicle, request.SWAT, request.UPR);
        }

        if (isNew)
            await context.FactionsEquipments.AddAsync(factionEquipment);
        else
            context.FactionsEquipments.Update(factionEquipment);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsEquipments, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Equipamento | {Serialize(factionEquipment)}");
    }

    [HttpPost("staff-save-equipment-item"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionEquipmentItemSave([FromBody] FactionEquipmentItemRequest request)
    {
        if (request.Ammo <= 0)
            throw new ArgumentException("Munição deve ser maior que 0.");

        if (!GlobalFunctions.CheckIfWeaponExists(request.Weapon))
            throw new ArgumentException($"Arma {request.Weapon} não encontrada.");

        var factionEquipmentItem = new FactionEquipmentItem(request.FactionEquipmentId, request.Weapon, request.Ammo, Serialize(request.Components));
        var isNew = !request.Id.HasValue;
        if (!isNew)
        {
            factionEquipmentItem = await context.FactionsEquipmentsItems.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (factionEquipmentItem is null)
                throw new ArgumentException(Resources.RecordNotFound);
        }

        factionEquipmentItem.Update(request.Weapon, request.Ammo, Serialize(request.Components));

        if (isNew)
            await context.FactionsEquipmentsItems.AddAsync(factionEquipmentItem);
        else
            context.FactionsEquipmentsItems.Update(factionEquipmentItem);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsEquipments, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Item Equipamento Facção | {Serialize(factionEquipmentItem)}");
    }

    [HttpDelete("staff-remove-equipment-item/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionEquipmentItemRemove(Guid id)
    {
        var factionEquipmentItem = await context.FactionsEquipmentsItems.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        context.FactionsEquipmentsItems.Remove(factionEquipmentItem);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsEquipments, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Remover Item Equipamento Facção | {Serialize(factionEquipmentItem)}");
    }

    [HttpPost("staff-save-vehicle"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffVehicleSave([FromBody] FactionVehicleRequest request)
    {
        if (!GlobalFunctions.CheckIfVehicleExists(request.Model))
            throw new ArgumentException($"Modelo {request.Model} não existe.");

        var faction = await context.Factions.FirstOrDefaultAsync(x => x.Id == request.FactionId)
            ?? throw new ArgumentException(Resources.RecordNotFound);

        if (!faction.HasVehicles)
            throw new ArgumentException("Facção não possui flag de veículos.");

        var vehicle = new Vehicle();
        vehicle.Create(request.Model, await GenerateVehiclePlate(faction.HasDuty), 0, 0, 0, 0, 0, 0);
        vehicle.SetFuel(vehicle.GetMaxFuel());
        vehicle.SetFaction(faction!.Id);
        await context.Vehicles.AddAsync(vehicle);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Criar Veículo | {Serialize(vehicle)}");
    }

    private async Task<string> GenerateVehiclePlate(bool government)
    {
        var plate = string.Empty;
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        do
        {
            if (government)
                plate = $"1{random.Next(0, 9_999_999).ToString().PadLeft(7, '0')}";
            else
                plate = $"{chars[random.Next(25)]}{chars[random.Next(25)]}{chars[random.Next(25)]}{random.Next(0, 999).ToString().PadLeft(3, '0')}";
        } while (await context.Vehicles.AnyAsync(x => x.Plate == plate));
        return plate;
    }

    [HttpDelete("staff-remove-vehicle/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffVehicleRemove(Guid id)
    {
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == id);
        if (vehicle is not null)
        {
            if (vehicle.Spawned)
                throw new ArgumentException("Veículo está spawnado.");

            vehicle.SetSold();
            context.Vehicles.Update(vehicle);
            await context.SaveChangesAsync();
            await WriteLog(LogType.Staff, $"Remover Veículo {vehicle.Plate}");
        }
    }

    [HttpPost("staff-save-frequency"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionFrequencySave([FromBody] FactionFrequencyRequest request)
    {
        if (request.Frequency <= 0)
            throw new ArgumentException("Frequência deve ser maior que 0.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Nome não preenchido.");

        var factionFrequency = new FactionFrequency();

        if (await context.FactionsFrequencies.AnyAsync(x => x.Frequency == request.Frequency && x.Id != request.Id))
            throw new ArgumentException($"A frequência {request.Frequency} já está sendo utilizada.");

        var isNew = !request.Id.HasValue;
        if (isNew)
        {
            factionFrequency.Create(request.FactionId, request.Frequency, request.Name);
        }
        else
        {
            factionFrequency = await context.FactionsFrequencies.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (factionFrequency is null)
                throw new ArgumentException(Resources.RecordNotFound);

            factionFrequency.Update(request.Frequency, request.Name);
        }

        if (isNew)
            await context.FactionsFrequencies.AddAsync(factionFrequency);
        else
            context.FactionsFrequencies.Update(factionFrequency);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadFactionsFrequencies, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);
        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Gravar Frequência | {Serialize(factionFrequency)}");
    }

    [HttpDelete("staff-remove-frequency/{id}"), Authorize(Policy = PolicySettings.POLICY_STAFF_FLAG_FACTIONS)]
    public async Task StaffFactionFrequencyRemove(Guid id)
    {
        var factionFrequency = await context.FactionsFrequencies.FirstOrDefaultAsync(x => x.Id == id);
        if (factionFrequency is not null)
        {
            context.FactionsFrequencies.Remove(factionFrequency);

            var ucpAction = new UCPAction();
            ucpAction.Create(UCPActionType.ReloadFactionsFrequencies, UserId, string.Empty);
            await context.UCPActions.AddAsync(ucpAction);
            await context.SaveChangesAsync();

            await WriteLog(LogType.Staff, $"Remover Frequência | {Serialize(factionFrequency)}");
        }
    }
}