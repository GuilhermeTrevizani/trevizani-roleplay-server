using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class MDCScript : Script
{
    [Command(["mdc"], "Facção", "Abre o MDC")]
    public async Task CMD_mdc(MyPlayer player)
    {
        if (player.Faction?.HasMDC != true)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção habilitada.");
            return;
        }

        var apbs = "[]";
        var bolos = "[]";
        var pendingReports = "[]";
        var crimes = "[]";
        var weaponLicenseTypes = "[]";
        var emergencyCalls = "[]";

        if (player.Faction.Type == FactionType.Police || player.Faction.Type == FactionType.Firefighter)
        {
            emergencyCalls = Functions.Serialize(Global.EmergencyCalls.Where(x => ((int)x.Type == (int)player.Faction.Type || x.Type == EmergencyCallType.PoliceAndFirefighter)
                && (DateTime.Now - x.RegisterDate).TotalHours < 24)
                .OrderByDescending(x => x.RegisterDate)
                .Select(x => new
                {
                    x.Id,
                    x.RegisterDate,
                    x.Number,
                    x.Location,
                    x.Message
                })
                .ToList());
        }

        if (player.Faction.Type == FactionType.Police || player.Faction.Type == FactionType.Government)
        {
            pendingReports = await GetPendingReports(player);
        }

        if (player.Faction.Type == FactionType.Police)
        {
            var wanteds = await GetWantedsJson();
            apbs = wanteds.Item1;
            bolos = wanteds.Item2;
            crimes = Functions.Serialize(Global.Crimes.OrderBy(x => x.Name).Select(x => new
            {
                x.Id,
                x.Name,
                x.PrisonMinutes,
                x.FineValue,
                x.DriverLicensePoints,
            }));
            weaponLicenseTypes = Functions.Serialize(
                Enum.GetValues<CharacterWeaponLicenseType>()
                .Select(x => new
                {
                    Value = x,
                    Label = x.ToString(),
                })
                .OrderBy(x => x.Label)
            );
        }

        player.Emit("Server:AbrirMDC", (int)player.Faction.Type, player.Faction.Name,
            emergencyCalls,
            GetFactionsUnitsJson(player),
            apbs, bolos,
            pendingReports,
            crimes,
            weaponLicenseTypes);
        player.SendMessageToNearbyPlayers("abre o MDC.", MessageCategory.Ame);
    }

    private async Task<string> GetPendingReports(MyPlayer player)
    {
        var context = Functions.GetDatabaseContext();
        var seizedVehicles = (await context.SeizedVehicles
            .Where(x => x.FactionId == player.Faction!.Id && string.IsNullOrWhiteSpace(x.Description))
                .Include(x => x.Vehicle)
                .Include(x => x.PoliceOfficerCharacter)
            .ToListAsync())
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                Description = $"Apreensão de {x.Vehicle!.Model.ToUpper()} {x.Vehicle.Plate} por ${x.Value:N0} até {x.EndDate}. Motivo: {x.Reason}",
                IsOwner = x.PoliceOfficerCharacterId == player.Character.Id,
                Type = 1,
            })
            .ToList();

        var jails = (await context.Jails
            .Where(x => x.FactionId == player.Faction!.Id && string.IsNullOrWhiteSpace(x.Description))
                .Include(x => x.Character)
                .Include(x => x.PoliceOfficerCharacter)
            .ToListAsync())
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                Description = $"Prisão de {x.Character!.Name} por {(x.EndDate - x.RegisterDate).TotalMinutes:N0} minutos. Motivo: {x.Reason}",
                IsOwner = x.PoliceOfficerCharacterId == player.Character.Id,
                Type = 2,
            })
            .ToList();

        var fines = (await context.Fines
            .Where(x => x.FactionId == player.Faction!.Id && string.IsNullOrWhiteSpace(x.Description))
                .Include(x => x.Character)
                .Include(x => x.PoliceOfficerCharacter)
            .ToListAsync())
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                Description = $"Multa de {x.Character!.Name} por ${x.Value:N0}. Motivo: {x.Reason}",
                IsOwner = x.PoliceOfficerCharacterId == player.Character.Id,
                Type = 3,
            })
            .ToList();

        var confiscations = await context.Confiscations
            .Where(x => x.FactionId == player.Faction!.Id && string.IsNullOrWhiteSpace(x.Description))
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                Description = $"Confisco ({x.Identifier})",
                IsOwner = x.PoliceOfficerCharacterId == player.Character.Id,
                Type = 4,
            })
            .ToListAsync();

        seizedVehicles.AddRange(jails);
        seizedVehicles.AddRange(fines);
        seizedVehicles.AddRange(confiscations);

        return Functions.Serialize(seizedVehicles.OrderByDescending(x => x.IsOwner).ThenByDescending(x => x.RegisterDate));
    }

    private async Task<(string, string)> GetWantedsJson()
    {
        var context = Functions.GetDatabaseContext();
        var wanted = await context.Wanted
            .Where(x => !x.DeletedDate.HasValue)
            .Include(x => x.PoliceOfficerCharacter)
            .Include(x => x.WantedCharacter)
            .Include(x => x.WantedVehicle)
            .OrderByDescending(x => x.RegisterDate)
            .ToListAsync();

        var apbs = wanted.Where(x => x.WantedCharacterId.HasValue)
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                x.WantedCharacter!.Name,
                x.Reason,
            });

        var bolos = wanted.Where(x => x.WantedVehicleId.HasValue)
            .Select(x => new
            {
                x.Id,
                x.RegisterDate,
                PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                Name = $"{x.WantedVehicle!.Model} {x.WantedVehicle.Plate}",
                x.Reason,
            });

        return new(Functions.Serialize(apbs), Functions.Serialize(bolos));
    }

    private static string GetFactionsUnitsJson(MyPlayer player)
    {
        var factionUnits = Global.FactionsUnits.Where(x => x.FactionId == player.Character.FactionId)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.RegisterDate,
                CanClose = player.FactionFlags.Contains(FactionFlag.CloseUnit) || x.CharacterId == player.Character.Id || x.Characters!.Any(y => y.CharacterId == player.Character.Id),
                Characters = x.GetCharacters(),
                x.PosX,
                x.PosY,
                x.Status,
            }).ToList();
        return Functions.Serialize(factionUnits);
    }

    [RemoteEvent(nameof(MDCSearchPerson))]
    public async Task MDCSearchPerson(Player playerParam, string search)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            if (int.TryParse(search, out int id))
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == id);
                if (target is null)
                {
                    player.SendNotification(NotificationType.Error, $"Nenhum jogador online com o ID {id}.");
                    return;
                }

                search = target.Character.Name;
            }

            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.Where(x => x.Name.ToLower() == search.ToLower())
                .Include(x => x.PoliceOfficerBlockedDriverLicenseCharacter)
                .Include(x => x.PoliceOfficerWeaponLicenseCharacter)
                .FirstOrDefaultAsync();
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhuma pessoa foi encontrada com o nome {search}.");
                return;
            }

            var job = character.Job.GetDescription();
            if (character.FactionId.HasValue)
            {
                var faction = Global.Factions.FirstOrDefault(x => x.Id == character.FactionId);
                if (faction?.Type != FactionType.Criminal)
                    job = $"{Global.FactionsRanks.FirstOrDefault(x => x.Id == character.FactionRankId)?.Name ?? string.Empty} - {faction?.Name ?? string.Empty}";
            }

            var properties = Global.Properties
                .Where(x => x.CharacterId == character.Id)
                .Select(x => new
                {
                    x.FormatedAddress,
                })
                .ToList();

            var vehicles = (await context.Vehicles
                .Where(x => x.CharacterId == character.Id && !x.Sold)
                .OrderByDescending(x => x.RegisterDate)
                .ToListAsync())
                .Select(x => new
                {
                    Model = x.Model.ToUpper(),
                    x.Plate,
                    Insurance = x.GetInsuranceInfo(),
                });

            var fines = (await context.Fines
                .Where(x => x.CharacterId == character.Id)
                .Include(x => x.PoliceOfficerCharacter)
                .OrderByDescending(x => x.RegisterDate)
                .ToListAsync())
                .Select(x => new
                {
                    x.RegisterDate,
                    x.Value,
                    PoliceOfficerCharacter = x.PoliceOfficerCharacter!.Name,
                    x.Reason,
                    x.Description,
                    Payed = x.PaymentDate.HasValue,
                    x.DriverLicensePoints,
                });

            var jails = (await context.Jails
                .Where(x => x.CharacterId == character.Id)
                .Include(x => x.PoliceOfficerCharacter)
                .OrderByDescending(x => x.RegisterDate)
                .ToListAsync())
                .Select(x => new
                {
                    x.RegisterDate,
                    Minutes = Convert.ToInt32((x.EndDate - x.RegisterDate).TotalMinutes),
                    PoliceOfficerCharacter = x.PoliceOfficerCharacter!.Name,
                    x.Description,
                    x.Reason,
                });

            var confiscations = (await context.Confiscations
                .Where(x => x.CharacterId == character.Id)
                .Include(x => x.PoliceOfficerCharacter)
                .Include(x => x.Items)
                .OrderByDescending(x => x.RegisterDate)
                .Select(x => new
                {
                    x.RegisterDate,
                    PoliceOfficerCharacter = x.PoliceOfficerCharacter!.Name,
                    x.Description,
                    x.Identifier,
                    Items = x.Items!.Select(y => new
                    {
                        Name = y.GetName(),
                        y.Quantity,
                        Extra = y.GetExtra().Replace("<br/>", ", "),
                        y.Identifier,
                    })
                })
                .ToListAsync());

            var apb = await context.Wanted
                .Include(x => x.PoliceOfficerCharacter)
                .FirstOrDefaultAsync(x => x.WantedCharacterId == character.Id && !x.DeletedDate.HasValue);

            string GetDriverLicenseColor()
            {
                return character.PoliceOfficerBlockedDriverLicenseCharacterId.HasValue
                    || character.DriverLicenseBlockedDate?.Date >= DateTime.Now.Date
                    || !character.DriverLicenseValidDate.HasValue
                    || character.DriverLicenseValidDate.Value.Date < DateTime.Now.Date
                    ? "red" : "green";
            }

            string GetDriverLicenseText()
            {
                if (character.PoliceOfficerBlockedDriverLicenseCharacterId.HasValue)
                    return string.Format(Resources.RevokedBy, character.PoliceOfficerBlockedDriverLicenseCharacter!.Name!.ToUpper());

                if (character.DriverLicenseBlockedDate?.Date >= DateTime.Now.Date)
                    return Resources.Suspended;

                if (!character.DriverLicenseValidDate.HasValue)
                    return Resources.DoesNotHave;

                if (character.DriverLicenseValidDate.Value.Date < DateTime.Now.Date)
                    return Resources.Expired;

                return Resources.Valid;
            }

            string GetWeaponLicenseColor()
            {
                return (character.WeaponLicenseValidDate ?? DateTime.MinValue).Date >= DateTime.Now.Date
                    ? "green" : "red";
            }

            string GetWeaponLicenseText()
            {
                var text = string.Empty;

                if (!character.WeaponLicenseValidDate.HasValue)
                    text = Resources.DoesNotHave;
                else if (character.WeaponLicenseValidDate.Value.Date < DateTime.Now.Date)
                    text = Resources.Expired;
                else
                    text = Resources.Valid;

                if (character.PoliceOfficerWeaponLicenseCharacterId.HasValue)
                    text += $" ({character.WeaponLicenseType}) (POR {character.PoliceOfficerWeaponLicenseCharacter!.Name})";

                return text.ToUpper();
            }

            var person = new
            {
                character.Id,
                character.Name,
                character.Age,
                Sex = character.Sex.GetDescription(),
                Job = job,
                DriverLicenseColor = GetDriverLicenseColor(),
                DriverLicenseText = GetDriverLicenseText().ToUpper(),
                APB = apb is null ? null : new
                {
                    apb.Id,
                    PoliceOfficerCharacter = apb.PoliceOfficerCharacter!.Name,
                    apb.RegisterDate,
                    apb.Reason,
                },
                Properties = properties,
                Vehicles = vehicles,
                Fines = fines,
                Jails = jails,
                Confiscations = confiscations,
                WeaponLicenseColor = GetWeaponLicenseColor(),
                WeaponLicenseText = GetWeaponLicenseText().ToUpper(),
                CanManageWeaponLicense = player.FactionFlags.Contains(FactionFlag.WeaponLicense),
            };

            player.Emit("MDC:UpdateSearchPerson", Functions.Serialize(person));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCSearchVehicle))]
    public async Task MDCSearchVehicle(Player playerParam, string search)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            if (int.TryParse(search, out int id))
            {
                var target = Global.Vehicles.FirstOrDefault(x => x.Id == id);
                if (target is null)
                {
                    player.SendNotification(NotificationType.Error, $"Nenhum veículo spawnado com o ID {id}.");
                    return;
                }

                search = target.VehicleDB.Plate;
            }

            var context = Functions.GetDatabaseContext();
            var vehicle = await context.Vehicles
                .Include(x => x.Character)
                .Include(x => x.Faction)
                .FirstOrDefaultAsync(x => x.Plate.ToLower() == search.ToLower());
            if (vehicle is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhum veículo foi encontrado com a placa {search}.");
                return;
            }

            var bolo = await context.Wanted
                .Include(x => x.PoliceOfficerCharacter)
                .FirstOrDefaultAsync(x => x.WantedVehicleId == vehicle.Id && !x.DeletedDate.HasValue);

            var owner = "N/A";
            if (vehicle.CharacterId.HasValue)
                owner = vehicle.Sold ? Resources.Dealership : vehicle.Character!.Name;
            if (vehicle.FactionId.HasValue)
                owner = vehicle.Faction!.Name;

            var vehicleResponse = new
            {
                vehicle.Id,
                vehicle.Plate,
                Model = vehicle.Model.ToUpper(),
                vehicle.SeizedValue,
                Owner = owner,
                CanTrack = vehicle.FactionId.HasValue,
                BOLO = bolo is null ? null : new
                {
                    bolo.Id,
                    bolo.RegisterDate,
                    PoliceOfficerCharacter = bolo.PoliceOfficerCharacter!.Name,
                    bolo.Reason,
                },
                Insurance = vehicle.GetInsuranceInfo(),
            };

            player.Emit("MDC:UpdateSearchVehicle", Functions.Serialize(vehicleResponse));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCSearchProperty))]
    public async Task MDCSearchProperty(Player playerParam, string search)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.FormatedAddress == search);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhuma propriedade foi encontrada com o endereço {search}.");
                return;
            }

            var owner = "N/A";
            if (property.CharacterId.HasValue)
            {
                var context = Functions.GetDatabaseContext();
                var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == property.CharacterId);
                owner = character?.Name ?? string.Empty;
            }

            var propertyResponse = new
            {
                property.FormatedAddress,
                Owner = owner,
                property.Value,
            };

            player.Emit("MDC:UpdateSearchProperty", Functions.Serialize(propertyResponse));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCRastrearVeiculo))]
    public void MDCRastrearVeiculo(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var id = idString.ToGuid();
            var vehicle = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == id && x.VehicleDB.FactionId.HasValue);
            if (vehicle is null)
            {
                player.SendNotification(NotificationType.Error, "O veículo não foi encontrado.");
                return;
            }

            player.SetWaypoint(vehicle.ICPosition.X, vehicle.ICPosition.Y);
            player.SendNotification(NotificationType.Success, "A posição do veículo foi marcada no seu GPS.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCAdicionarBOLO))]
    public async Task MDCAdicionarBOLO(Player playerParam, int type, string idString, string reason, string pesquisa)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                player.SendNotification(NotificationType.Error, "Motivo não informado.");
                return;
            }

            var wanted = new Wanted();

            var id = idString.ToGuid()!;
            if (type == 1)
                wanted.CreateByCharacter(player.Character.Id, id.Value, reason);
            else
                wanted.CreateByVehicle(player.Character.Id, id.Value, reason);

            var context = Functions.GetDatabaseContext();
            await context.Wanted.AddAsync(wanted);
            await context.SaveChangesAsync();

            var wanteds = await GetWantedsJson();

            if (wanted.WantedCharacterId.HasValue)
            {
                player.Emit("MDC:UpdateAPBS", wanteds.Item1);
                await MDCSearchPerson(player, pesquisa);
            }
            else
            {
                player.Emit("MDC:UpdateBOLOS", wanteds.Item2);
                await MDCSearchVehicle(player, pesquisa);
            }

            player.SendNotification(NotificationType.Success, $"{(type == 1 ? "APB" : "BOLO")} adicionado.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCRemoverBOLO))]
    public async Task MDCRemoverBOLO(Player playerParam, string idString, string pesquisa)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var id = idString.ToGuid();
            var wanted = await context.Wanted.FirstOrDefaultAsync(x => x.Id == id);
            if (wanted is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            wanted.Delete(player.Character.Id);

            context.Wanted.Update(wanted);
            await context.SaveChangesAsync();

            var wanteds = await GetWantedsJson();

            if (wanted.WantedCharacterId.HasValue)
            {
                player.Emit("MDC:UpdateAPBS", wanteds.Item1);
                await MDCSearchPerson(player, pesquisa);
            }
            else
            {
                player.Emit("MDC:UpdateBOLOS", wanteds.Item2);
                await MDCSearchVehicle(player, pesquisa);
            }

            player.SendNotification(NotificationType.Success, $"{(wanted.WantedCharacterId.HasValue ? "APB" : "BOLO")} removido.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCTrackEmergencyCall))]
    public void MDCTrackEmergencyCall(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Government != true)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção habilitada.");
                return;
            }

            var id = idString.ToGuid();
            var emergencyCall = Global.EmergencyCalls.FirstOrDefault(x => x.Id == id);
            if (emergencyCall is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            player.SetWaypoint(emergencyCall.PosX, emergencyCall.PosY);
            player.SendNotification(NotificationType.Success, $"A localização da ligação de emergência foi marcada no seu GPS.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCFinePerson))]
    public async Task MDCFinePerson(Player playerParam, string idString, string nome, string crimesJson)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var id = idString.ToGuid();
            var character = await context.Characters
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var crimesIds = Functions.Deserialize<Guid[]>(crimesJson);
            var crimes = Global.Crimes.Where(x => crimesIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();
            var value = crimes.Sum(x => x.FineValue);
            if (value <= 0)
            {
                player.SendNotification(NotificationType.Error, "Valor inválido.");
                return;
            }

            var reason = string.Join(", ", crimes.Select(x => x.Name));
            var driverLicensePoints = crimes.Sum(x => x.DriverLicensePoints);

            var fine = new Fine();
            fine.Create(character.Id, player.Character.Id, player.Faction!.Id, reason, value, driverLicensePoints);
            await context.Fines.AddAsync(fine);
            await context.SaveChangesAsync();

            DateTime? driverLicenseBlockedDate = null;
            var driverLicenseRevoked = false;

            driverLicensePoints = await context.Fines
                .Where(x => x.CharacterId == fine.CharacterId && x.RegisterDate >= DateTime.Now.AddDays(-14))
                .SumAsync(x => x.DriverLicensePoints);
            if (driverLicensePoints >= 5)
            {
                driverLicenseRevoked = true;
                driverLicenseBlockedDate = DateTime.Now.AddDays(2);
            }
            else if (driverLicensePoints >= 3)
            {
                driverLicenseBlockedDate = DateTime.Now.AddDays(2);
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is null)
            {
                if (driverLicenseRevoked)
                    character.SetPoliceOfficerBlockedDriverLicenseCharacterId(player.Character.Id);
                if (driverLicenseBlockedDate.HasValue)
                    character.SetDriverLicenseBlockedDate(driverLicenseBlockedDate);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }
            else
            {
                if (driverLicenseRevoked)
                    target.Character.SetPoliceOfficerBlockedDriverLicenseCharacterId(player.Character.Id);
                if (driverLicenseBlockedDate.HasValue)
                    character.SetDriverLicenseBlockedDate(driverLicenseBlockedDate);
            }

            await Functions.SendNotification(character.User!, $"Você foi multado em ${value:N0}.");

            player.SendNotification(NotificationType.Success, $"Você multou {nome} por ${value:N0}.");

            await MDCSearchPerson(player, nome);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCRevogarLicencaMotorista))]
    public async Task MDCRevogarLicencaMotorista(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == idString.ToGuid());
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is null)
            {
                character.SetPoliceOfficerBlockedDriverLicenseCharacterId(player.Character.Id);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }
            else
            {
                target.Character.SetPoliceOfficerBlockedDriverLicenseCharacterId(player.Character.Id);
            }

            await MDCSearchPerson(player, character.Name);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCAddUnit))]
    public async Task MDCAddUnit(Player playerParam, string name, string partners)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (string.IsNullOrWhiteSpace(name) || name.Length > 25)
            {
                player.SendNotification(NotificationType.Error, "Nome deve ter entre 1 e 25 caracteres.");
                return;
            }

            if (Global.FactionsUnits.Any(x => x.CharacterId == player.Character.Id)
                || Global.FactionsUnits.SelectMany(x => x.Characters!).Any(x => x.CharacterId == player.Character.Id))
            {
                player.SendNotification(NotificationType.Error, "Você já está em uma unidade.");
                return;
            }

            if (Global.FactionsUnits.Any(x => x.FactionId == player.Character.FactionId && x.Name == name))
            {
                player.SendNotification(NotificationType.Error, $"Unidade {name} já está em serviço.");
                return;
            }

            var factionUnitCharacters = new List<FactionUnitCharacter>();
            if (!string.IsNullOrWhiteSpace(partners))
            {
                var characters = partners.Split(',').ToList();
                foreach (var x in characters)
                {
                    if (!int.TryParse(x.Trim(), out int personagem))
                    {
                        player.SendNotification(NotificationType.Error, $"ID {x} informado em parceiros é inválido.");
                        return;
                    }

                    if (personagem == player.SessionId)
                    {
                        player.SendNotification(NotificationType.Error, $"ID {personagem} informado em parceiros é o seu ID.");
                        return;
                    }

                    var target = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == personagem);
                    if (target is null || target.Character.FactionId != player.Character.FactionId)
                    {
                        player.SendNotification(NotificationType.Error, $"ID {personagem} não é da sua facção ou está offline.");
                        return;
                    }

                    if (Global.FactionsUnits.Any(x => x.CharacterId == target.Character.Id)
                        || Global.FactionsUnits.SelectMany(x => x.Characters!).Any(x => x.CharacterId == target.Character.Id))
                    {
                        player.SendNotification(NotificationType.Error, $"ID {personagem} já está em uma unidade.");
                        return;
                    }

                    var factionUnitCharacter = new FactionUnitCharacter();
                    factionUnitCharacter.Create(target.Character.Id);

                    factionUnitCharacters.Add(factionUnitCharacter);
                }
            }

            var factionUnit = new FactionUnit();
            factionUnit.Create(name, player.Character.FactionId!.Value, player.Character.Id, factionUnitCharacters);

            var context = Functions.GetDatabaseContext();
            await context.FactionsUnits.AddAsync(factionUnit);
            await context.SaveChangesAsync();

            Global.FactionsUnits = await context.FactionsUnits
                .Where(x => !x.FinalDate.HasValue)
                .Include(x => x.Character!)
                .Include(x => x.Characters!)
                    .ThenInclude(x => x.Character)
                .ToListAsync();

            factionUnit = Global.FactionsUnits.FirstOrDefault(x => x.Name == name)!;
            player.SendFactionMessage($"{factionUnit.GetCharacters()} em serviço com a unidade {factionUnit.Name}.");

            player.Emit("MDC:UpdateFactionUnits", GetFactionsUnitsJson(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCCloseUnit))]
    public async Task MDCCloseUnit(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var factionUnit = Global.FactionsUnits.FirstOrDefault(x => x.Id == id);
            if (factionUnit is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            if (!player.FactionFlags.Contains(FactionFlag.CloseUnit)
                && factionUnit.CharacterId != player.Character.Id
                && !factionUnit.Characters!.Any(y => y.CharacterId == player.Character.Id))
            {
                player.SendNotification(NotificationType.Error, "Você não possui permissão para encerrar essa unidade.");
                return;
            }

            factionUnit.End();

            var context = Functions.GetDatabaseContext();
            context.FactionsUnits.Update(factionUnit);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Faction, $"Encerrar Unidade {factionUnit.Name} ({factionUnit.Id})", null);

            Global.FactionsUnits.Remove(factionUnit);
            player.SendFactionMessage($"{factionUnit.Name} fora de serviço.");

            player.Emit("MDC:UpdateFactionUnits", GetFactionsUnitsJson(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task UpdatePendingReports(MyPlayer player)
    {
        player.Emit("MDC:UpdatePendingReports", await GetPendingReports(player));
    }

    [RemoteEvent(nameof(MDCFillReportSeizedVehicle))]
    public async Task MDCFillReportSeizedVehicle(Player playerParam, string idString, string description)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.CanSeizeVehicles != true)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção habilitada.");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                player.SendNotification(NotificationType.Error, "Descrição não foi informada.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var seizedVehicle = await context.SeizedVehicles.FirstOrDefaultAsync(x => x.Id == id);
            if (seizedVehicle is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            seizedVehicle.SetDescription(description);
            context.SeizedVehicles.Update(seizedVehicle);
            await context.SaveChangesAsync();

            await UpdatePendingReports(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCFillReportFine))]
    public async Task MDCFillReportFine(Player playerParam, string idString, string description)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                player.SendNotification(NotificationType.Error, "Descrição não foi informada.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var fine = await context.Fines.FirstOrDefaultAsync(x => x.Id == id);
            if (fine is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            fine.SetDescription(description);
            context.Fines.Update(fine);
            await context.SaveChangesAsync();

            await UpdatePendingReports(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCFillReportJail))]
    public async Task MDCFillReportJail(Player playerParam, string idString, string description)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                player.SendNotification(NotificationType.Error, "Descrição não foi informada.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var jail = await context.Jails.FirstOrDefaultAsync(x => x.Id == id);
            if (jail is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            jail.SetDescription(description);
            context.Jails.Update(jail);
            await context.SaveChangesAsync();

            await UpdatePendingReports(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCFillReportConfiscation))]
    public async Task MDCFillReportConfiscation(Player playerParam, string idString, string description)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                player.SendNotification(NotificationType.Error, "Descrição não foi informada.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var confiscation = await context.Confiscations.FirstOrDefaultAsync(x => x.Id == id);
            if (confiscation is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            confiscation.SetDescription(description);
            context.Confiscations.Update(confiscation);
            await context.SaveChangesAsync();

            await UpdatePendingReports(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCGetConfiscationsWithoutAttribuition))]
    public async Task MDCGetConfiscationsWithoutAttribuition(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var context = Functions.GetDatabaseContext();
            var confiscations = await context.Confiscations
                .Where(x => !x.CharacterId.HasValue && x.FactionId == player.Character.FactionId)
                .Select(x => new
                {
                    x.Id,
                    x.RegisterDate,
                    x.Identifier,
                    PoliceOfficer = x.PoliceOfficerCharacter!.Name,
                })
                .OrderByDescending(x => x.RegisterDate)
                .ToListAsync();

            player.Emit("MDC:UpdateConfiscationsWithoutAttribuition", Functions.Serialize(confiscations));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCSaveConfiscation))]
    public async Task MDCSaveConfiscation(Player playerParam, string idString, string identifier, string characterName)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            identifier ??= string.Empty;
            if (identifier.Length < 1 || identifier.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Identificador deve ter entre 1 e 50 caracteres.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Name == characterName
                && !x.DeathDate.HasValue && !x.DeletedDate.HasValue);

            var id = idString.ToGuid();
            var confiscation = await context.Confiscations.FirstOrDefaultAsync(x => x.Id == id && x.FactionId == player.Character.FactionId);
            if (confiscation is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            confiscation.Update(character?.Id, player.Character.Id, identifier);

            context.Confiscations.Update(confiscation);
            await context.SaveChangesAsync();

            player.SendNotification(NotificationType.Success, "Você modificou o confisco.");
            await MDCGetConfiscationsWithoutAttribuition(player);
            await UpdatePendingReports(player);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCArrestPerson))]
    public async Task MDCArrestPerson(Player playerParam, string idString, string crimesJson)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension() && x.FactionId == player.Character.FactionId);
            if (property is null)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma propriedade da sua facção.");
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == idString.ToGuid());
            if (target is null || !player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotCloseToThePlayer);
                return;
            }

            var ilegalItems = new List<ItemCategory> { ItemCategory.Weapon, ItemCategory.WalkieTalkie, ItemCategory.Drug, ItemCategory.WeaponComponent };
            if (target.Items.Any(x => ilegalItems.Contains(x.GetCategory()) || GlobalFunctions.CheckIfIsAmmo(x.GetCategory())))
            {
                player.SendNotification(NotificationType.Error, "Jogador está com itens ilegais.");
                return;
            }

            var crimesIds = Functions.Deserialize<Guid[]>(crimesJson);
            var crimes = Global.Crimes.Where(x => crimesIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();
            var minutes = crimes.Sum(x => x.PrisonMinutes);
            if (minutes <= 0)
            {
                player.SendNotification(NotificationType.Error, "Minutos inválidos.");
                return;
            }

            var reason = string.Join(", ", crimes.Select(x => x.Name));

            var jail = new Jail();
            jail.Create(target.Character.Id, player.Character.Id, player.Character.FactionId!.Value, minutes, reason);
            var context = Functions.GetDatabaseContext();
            await context.Jails.AddAsync(jail);
            await context.SaveChangesAsync();

            target.Character.SetJailFinalDate(jail.EndDate);

            player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} prendeu {target.Character.Name} por {minutes} minuto(s).");
            target.SetPosition(new(Global.Parameter.PrisonInsidePosX, Global.Parameter.PrisonInsidePosY, Global.Parameter.PrisonInsidePosZ), Global.Parameter.PrisonInsideDimension, true);
            target.SendMessage(MessageType.Error, $"{player.Character.Name} prendeu você por {minutes} minuto(s).");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCUpdateForensicLaboratory))]
    public async Task MDCUpdateForensicLaboratory(Player playerParam)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var context = Functions.GetDatabaseContext();
            var forensicTests = await context.ForensicTests
                .Include(x => x.Character)
                .Include(x => x.Items!)
                    .ThenInclude(x => x.OriginConfiscationItem)
                .Include(x => x.Items!)
                    .ThenInclude(x => x.TargetConfiscationItem)
                .Where(x => x.FactionId == player.Character.FactionId)
                .OrderByDescending(x => x.RegisterDate)
                .Take(25)
                .ToListAsync();

            static bool HasResult(ForensicTest forensicTest)
                => (DateTime.Now - forensicTest.RegisterDate).TotalHours >= 6;

            foreach (var forensicTest in forensicTests)
            {
                if (!HasResult(forensicTest))
                    continue;

                var items = forensicTest.Items!.Where(x => string.IsNullOrWhiteSpace(x.Result)).ToList();
                foreach (var item in items)
                {
                    var result = string.Empty;
                    if (!string.IsNullOrWhiteSpace(item.Identifier))
                        result = $"{item.Identifier}: ";

                    result += $"{item.OriginConfiscationItem!.GetName()}{(!string.IsNullOrWhiteSpace(item.OriginConfiscationItem!.Identifier) ? $" ({item.OriginConfiscationItem.Identifier})" : string.Empty)}";
                    if (item.Type == ForensicTestItemType.Blood)
                    {
                        var originExtra = Functions.Deserialize<BloodSampleItem>(item.OriginConfiscationItem!.Extra!);
                        result += $" de tipo sanguíneo {originExtra.BloodType.GetDescription()}";

                        if (item.TargetConfiscationItemId.HasValue)
                        {
                            var targetExtra = Functions.Deserialize<BloodSampleItem>(item.OriginConfiscationItem!.Extra!);
                            if (originExtra.CharacterId == targetExtra.CharacterId)
                                result += $" possui correspondência com ";
                            else
                                result += $" não possui correspondência com ";

                            result += $"{item.TargetConfiscationItem!.GetName()}{(!string.IsNullOrWhiteSpace(item.TargetConfiscationItem!.Identifier) ? $" ({item.TargetConfiscationItem.Identifier})" : string.Empty)}.";
                        }
                        else
                        {
                            var jail = await context.Jails
                                .Include(x => x.Character)
                                .FirstOrDefaultAsync(x => x.CharacterId == originExtra.CharacterId);
                            if (jail is null)
                                result += " não possui correspondência no banco de dados criminal.";
                            else
                                result += $" possui correspondência no banco de dados criminal ({jail.Character!.Name}).";
                        }
                    }
                    else if (item.Type == ForensicTestItemType.BulletShell)
                    {
                        var originExtra = Functions.Deserialize<BulletShellItem>(item.OriginConfiscationItem!.Extra!);

                        var targetExtraId = Guid.Empty;
                        if (item.TargetConfiscationItem!.GetCategory() == ItemCategory.Weapon)
                        {
                            var targetExtra = Functions.Deserialize<WeaponItem>(item.TargetConfiscationItem!.Extra!);
                            targetExtraId = targetExtra.Id;
                        }
                        else
                        {
                            var targetExtra = Functions.Deserialize<BulletShellItem>(item.TargetConfiscationItem!.Extra!);
                            targetExtraId = targetExtra.WeaponItemId;
                        }

                        if (originExtra.WeaponItemId == targetExtraId)
                            result += $" possui correspondência com ";
                        else
                            result += $" não possui correspondência com ";

                        result += $"{item.TargetConfiscationItem!.GetName()}{(!string.IsNullOrWhiteSpace(item.TargetConfiscationItem!.Identifier) ? $" ({item.TargetConfiscationItem.Identifier})" : string.Empty)}.";
                    }
                    else
                    {
                        continue;
                    }
                    item.SetResult(result);

                    await context.ForensicTestsItems
                        .Where(x => x.Id == item.Id)
                        .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Result, item.Result));
                }
            }

            player.Emit("MDC:UpdateForensicLaboratory", Functions.Serialize(forensicTests.Select(x => new
            {
                x.RegisterDate,
                x.Identifier,
                PoliceOfficer = x.Character!.Name,
                HasResult = HasResult(x),
                Items = x.Items!.OrderBy(x => x.Identifier).Select(x => x.Result),
            })));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCGiveWeaponLicense))]
    public async Task MDCGiveWeaponLicense(Player playerParam, string idString, int type)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var weaponLicenseType = (CharacterWeaponLicenseType)type;

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is null)
            {
                character.SetWeaponLicense(weaponLicenseType, Global.Parameter.WeaponLicenseMonths, player.Character.Id);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }
            else
            {
                target.Character.SetWeaponLicense(weaponLicenseType, Global.Parameter.WeaponLicenseMonths, player.Character.Id);
                await target.Save();
            }

            await MDCSearchPerson(player, character.Name);
            player.SendNotification(NotificationType.Success, "Licença de armas atribuída.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(MDCRemoveWeaponLicense))]
    public async Task MDCRemoveWeaponLicense(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (player.Faction?.Type != FactionType.Police)
            {
                player.SendNotification(NotificationType.Error, "Você não está em uma facção policial.");
                return;
            }

            var id = idString.ToGuid();
            var context = Functions.GetDatabaseContext();
            var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == id);
            if (character is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character.Id);
            if (target is null)
            {
                character.RemoveWeaponLicense(player.Character.Id);
                context.Characters.Update(character);
                await context.SaveChangesAsync();
            }
            else
            {
                target.Character.RemoveWeaponLicense(player.Character.Id);
                await target.Save();
            }

            await MDCSearchPerson(player, character.Name);
            player.SendNotification(NotificationType.Success, "Licença de armas removida.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [Command(["ploc"], "Facção", "Atualiza a localização da sua unidade")]
    public async Task CMD_ploc(MyPlayer player)
    {
        if (player.Faction?.Government != true)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção habilitada.");
            return;
        }

        var factionUnit = Global.FactionsUnits.FirstOrDefault(x => x.CharacterId == player.Character.Id
            || x.Characters!.Any(y => y.CharacterId == player.Character.Id));
        if (factionUnit is null)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma unidade.");
            return;
        }

        factionUnit.UpdatePosition(player.ICPosition.X, player.ICPosition.Y);

        var context = Functions.GetDatabaseContext();
        context.FactionsUnits.Update(factionUnit);
        await context.SaveChangesAsync();

        player.SendMessage(MessageType.Success, "Você atualizou a localização da sua unidade.");
    }

    [Command(["ps"], "Facção", "Atualiza o status da sua unidade", "(status)", GreedyArg = true)]
    public async Task CMD_ps(MyPlayer player, string status)
    {
        if (player.Faction?.Government != true)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção habilitada.");
            return;
        }

        var factionUnit = Global.FactionsUnits.FirstOrDefault(x => x.CharacterId == player.Character.Id
            || x.Characters!.Any(y => y.CharacterId == player.Character.Id));
        if (factionUnit is null)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma unidade.");
            return;
        }

        if (status.Length > 25)
        {
            player.SendMessage(MessageType.Error, "Status deve possuir no máximo 25 caracteres.");
            return;
        }

        factionUnit.UpdateStatus(status);

        var context = Functions.GetDatabaseContext();
        context.FactionsUnits.Update(factionUnit);
        await context.SaveChangesAsync();

        player.SendMessage(MessageType.Success, "Você atualizou o status da sua unidade.");
    }
}