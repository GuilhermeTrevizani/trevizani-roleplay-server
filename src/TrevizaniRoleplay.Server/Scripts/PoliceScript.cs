using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class PoliceScript : Script
{
    [Command("algemar", "/algemar (ID ou nome)")]
    public static void CMD_algemar(MyPlayer player, string idOrName)
    {
        if (player.Faction?.Type != FactionType.Police)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial.");
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendNotification(NotificationType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return;
        }

        var target = player.GetCharacterByIdOrName(idOrName, false);
        if (target is null)
            return;

        if (!player.CheckIfTargetIsCloseIC(target, Constants.RP_DISTANCE))
        {
            player.SendMessage(MessageType.Error, Resources.YouAreNotCloseToThePlayer);
            return;
        }

        if (player.Speed > 0 || target.Speed > 0)
        {
            player.SendMessage(MessageType.Error, "Você ou o jogador está se movendo.");
            return;
        }

        target.Cuffed = !target.Cuffed;

        if (target.Cuffed)
        {
            target.AttachObject(Constants.HANDCUFF_OBJECT_MODEL, 60309, new(-0.04f, 0.06f, 0.03f), new(-68.18, 105.42, 0));
            target.PlayAnimation("mp_arresting", "idle", (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), freeze: true);
            player.SendMessage(MessageType.Success, $"Você algemou {target.ICName}.");
            target.SendMessage(MessageType.Success, $"{player.ICName} algemou você.");
        }
        else
        {
            target.DetachObject(Constants.HANDCUFF_OBJECT_MODEL);
            target.StopAnimationEx();
            player.SendMessage(MessageType.Success, $"Você desalgemou {target.ICName}.");
            target.SendMessage(MessageType.Success, $"{player.ICName} desalgemou você.");
        }
    }

    [Command("apreender", "/apreender (id) (valor) (dias) (motivo)", GreedyArg = true)]
    public async Task CMD_apreender(MyPlayer player, int id, int value, int days, string reason)
    {
        if (player.Faction?.CanSeizeVehicles != true || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção habilitada ou não está em serviço.");
            return;
        }

        var spot = Global.Spots.FirstOrDefault(x => x.Type == SpotType.VehicleSeizure
            && player.GetPosition().DistanceTo(x.GetPosition()) <= 5);
        if (spot is null)
        {
            player.SendMessage(MessageType.Error, "Você não está em ponto de apreensão de veículos.");
            return;
        }

        if (value <= 0)
        {
            player.SendMessage(MessageType.Error, "Valor da apreensão deve ser maior que 0.");
            return;
        }

        if (days <= 0)
        {
            player.SendMessage(MessageType.Error, "Dias deve ser maior que 0.");
            return;
        }

        if (reason.Length > 255)
        {
            player.SendMessage(MessageType.Error, "Motivo deve ter menos que 255 caracteres.");
            return;
        }

        var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == id);
        if (vehicle is null)
        {
            player.SendMessage(MessageType.Error, $"Veículo {id} não encontrado.");
            return;
        }

        if (player.GetPosition().DistanceTo(vehicle.GetPosition()) > 5 || player.GetDimension() != vehicle.GetDimension())
        {
            player.SendMessage(MessageType.Error, "Você não está próximo do veículo.");
            return;
        }

        if (vehicle.VehicleDB.FactionId.HasValue || vehicle.SpawnType != MyVehicleSpawnType.Normal)
        {
            player.SendMessage(MessageType.Error, "Veículo não pode ser apreendido.");
            return;
        }

        var endDate = DateTime.Now.AddDays(days);

        var seizedVehicle = new SeizedVehicle();
        seizedVehicle.Create(vehicle.VehicleDB.Id, player.Character.Id, value, reason, player.Character.FactionId!.Value, endDate);
        var context = Functions.GetDatabaseContext();
        await context.SeizedVehicles.AddAsync(seizedVehicle);
        await context.SaveChangesAsync();

        vehicle.VehicleDB.ChangePosition(spot.PosX, spot.PosY, spot.PosZ, 0, 0, 0, spot.Dimension);
        vehicle.VehicleDB.SetSeized(value, false, endDate);

        await vehicle.Park(player);

        player.SendFactionMessage($"{player.FactionRank!.Name} {player.Character.Name} apreendeu {vehicle.Identifier} por ${value:N0} até {endDate}. Motivo: {reason}");
    }

    [Command("radar", "/radar (velocidade)")]
    public static void CMD_radar(MyPlayer player, int speed)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        if (player.RadarSpot is not null)
        {
            player.SendMessage(MessageType.Error, "Você já possui um radar ativo.");
            return;
        }

        var pos = player.GetPosition();
        pos.Z -= player.IsInVehicle ? 0.45f : 0.95f;

        var newSpot = new Spot();
        newSpot.Create(SpotType.GarbageCollector, pos.X, pos.Y, pos.Z, 0);
        newSpot.CreateBlipAndMarkerForClient(player, pos,
            225, 59, 0.5f, "Radar",
            Constants.MARKER_TYPE_HALO, 10, Global.MainRgba);

        var colShape = Functions.CreateColShapeCylinder(pos, 10, 3, player.GetDimension());
        colShape.PoliceOfficerCharacterId = player.Character.Id;
        colShape.MaxSpeed = speed;
        colShape.SpotId = newSpot.Id;

        player.RadarSpot = newSpot;

        player.SendMessage(MessageType.Success, $"Você criou um radar com a velocidade {speed}.");
    }

    [Command("radaroff")]
    public static void CMD_radaroff(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendMessage(MessageType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        if (player.RadarSpot == null)
        {
            player.SendMessage(MessageType.Error, "Você não possui um radar ativo.");
            return;
        }

        player.RadarSpot?.RemoveIdentifier();
        player.RadarSpot = null;

        player.SendMessage(MessageType.Success, "Você removeu o radar.");
    }

    [Command("confisco")]
    public static void CMD_confisco(MyPlayer player)
    {
        if (player.Faction?.Type != FactionType.Police || !player.OnDuty)
        {
            player.SendNotification(NotificationType.Error, "Você não está em uma facção policial ou não está em serviço.");
            return;
        }

        var property = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension() && x.FactionId == player.Character.FactionId);
        if (property is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está em uma propriedade da sua facção.");
            return;
        }

        var itemsJSON = Functions.Serialize(
            player.Items.Select(x => new
            {
                x.Id,
                Name = $"{x.GetName()} {(!string.IsNullOrWhiteSpace(x.GetExtra()) ? $"[{x.GetExtra().Replace("<br/>", ", ")}]" : string.Empty)}",
                x.Quantity
            })
        );

        player.Emit("Confiscation:Show", itemsJSON);
    }

    [RemoteEvent(nameof(ConfiscationSave))]
    public async Task ConfiscationSave(Player playerParam, string identifier, string characterName, string itemsJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            identifier ??= string.Empty;
            if (identifier.Length < 1 || identifier.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Identificador deve ter entre 1 e 50 caracteres.");
                return;
            }

            var items = Functions.Deserialize<IEnumerable<AddConfiscationItemRequest>>(itemsJSON);
            if (!items.Any())
            {
                player.SendNotification(NotificationType.Error, "Nenhum item informado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            if (await context.Confiscations.AnyAsync(x => x.Identifier == identifier))
            {
                player.SendNotification(NotificationType.Error, "Identificador já existe.");
                return;
            }

            var confiscationItems = new List<ConfiscationItem>();

            foreach (var item in items)
            {
                var it = player.Items.FirstOrDefault(x => x.Id == item.Id);
                if (it is null || item.Quantity > it.Quantity)
                {
                    player.SendNotification(NotificationType.Error, $"Quantidade do item {it?.GetName()} é superior a que você possui no inventário.");
                    return;
                }

                item.Identifier ??= string.Empty;
                if (item.Identifier.Length > 50)
                {
                    player.SendNotification(NotificationType.Error, "Identificador deve ter até 50 caracteres.");
                    return;
                }

                var confiscationItem = new ConfiscationItem();
                confiscationItem.Create(it.ItemTemplateId, it.Subtype, item.Quantity, it.Extra);
                confiscationItem.SetIdentifier(item.Identifier);

                confiscationItems.Add(confiscationItem);
            }

            var character = await context.Characters.FirstOrDefaultAsync(x => x.Name == characterName
                && !x.DeathDate.HasValue && !x.DeletedDate.HasValue);

            var confiscation = new Confiscation();
            confiscation.Create(character?.Id, player.Character.Id, player.Faction!.Id, confiscationItems, identifier);

            await context.Confiscations.AddAsync(confiscation);
            await context.SaveChangesAsync();

            foreach (var item in items)
            {
                var it = player.Items.FirstOrDefault(x => x.Id == item.Id);
                if (it is not null)
                {
                    if (it.GetIsStack())
                        await player.RemoveStackedItem(it.ItemTemplateId, item.Quantity);
                    else
                        await player.RemoveItem(it);
                }
            }

            player.SendNotification(NotificationType.Success, "Confisco criado.");
            player.Emit(Constants.CONFISCATION_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(ForensicSearchConfiscation))]
    public async Task ForensicSearchConfiscation(Player playerParam, bool first, int typeValue, string identifier)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var context = Functions.GetDatabaseContext();
            var confiscation = await context.Confiscations
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Identifier == identifier);
            if (confiscation is null)
            {
                ForensicSearchConfiscationEnd(player, first, "[]");
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var json = "[]";
            var type = (ForensicTestItemType)typeValue;
            if (type == ForensicTestItemType.Blood)
            {
                json = Functions.Serialize(confiscation.Items!
                    .Where(x => x.GetCategory() == ItemCategory.BloodSample)
                    .Select(x => new
                    {
                        Value = x.Id,
                        Label = $"{x.GetName()}{(!string.IsNullOrWhiteSpace(x.Identifier) ? $" ({x.Identifier})" : string.Empty)}",
                    })
                    .OrderBy(x => x.Label));
            }
            else if (type == ForensicTestItemType.BulletShell)
            {
                var items = confiscation.Items!.ToList();

                if (first)
                    items = [.. items.Where(x => GlobalFunctions.CheckIfIsBulletShell(x.GetCategory()))];
                else
                    items = [.. items.Where(x => x.GetCategory() == ItemCategory.Weapon || GlobalFunctions.CheckIfIsBulletShell(x.GetCategory()))];

                json = Functions.Serialize(items!
                    .Select(x => new
                    {
                        Value = x.Id,
                        Label = $"{x.GetName()}{(!string.IsNullOrWhiteSpace(x.Identifier) ? $" ({x.Identifier})" : string.Empty)}",
                    })
                    .OrderBy(x => x.Label));
            }
            else
            {
                player.SendNotification(NotificationType.Error, "Tipo inválido.");
            }

            ForensicSearchConfiscationEnd(player, first, json);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static void ForensicSearchConfiscationEnd(MyPlayer player, bool first, string json) =>
        player.Emit("ForensicLaboratoryPage:SearchConfiscationServer", first, json);

    [RemoteEvent(nameof(ForensicTestSave))]
    public async Task ForensicTestSave(Player playerParam, string identifier, string itemsJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            identifier ??= string.Empty;
            if (identifier.Length < 1 || identifier.Length > 50)
            {
                player.SendNotification(NotificationType.Error, "Identificador deve ter entre 1 e 50 caracteres.");
                return;
            }

            var items = Functions.Deserialize<IEnumerable<ForensicTestItemRequest>>(itemsJSON);
            if (!items.Any())
            {
                player.SendNotification(NotificationType.Error, "Nenhum item informado.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            if (await context.ForensicTests.AnyAsync(x => x.Identifier == identifier))
            {
                player.SendNotification(NotificationType.Error, "Identificador já existe.");
                return;
            }

            var forensicTestItems = new List<ForensicTestItem>();

            foreach (var item in items)
            {
                var originConfiscationItem = await context.ConfiscationsItems
                    .FirstOrDefaultAsync(x => x.Id == item.OriginConfiscationItemId);
                if (originConfiscationItem is null)
                {
                    player.SendNotification(NotificationType.Error, $"Primeiro Item de {item.Identifier} é inválido.");
                    return;
                }

                ConfiscationItem? targetConfiscationItem = null;
                if (item.TargetConfiscationItemId.HasValue)
                    targetConfiscationItem = await context.ConfiscationsItems
                        .FirstOrDefaultAsync(x => x.Id == item.TargetConfiscationItemId);

                if (item.Type == ForensicTestItemType.Blood)
                {
                    if (originConfiscationItem.GetCategory() != ItemCategory.BloodSample)
                    {
                        player.SendNotification(NotificationType.Error, $"Primeiro Item de {item.Identifier} não é uma amostra de sangue.");
                        return;
                    }

                    if (targetConfiscationItem is not null && targetConfiscationItem.GetCategory() != ItemCategory.BloodSample)
                    {
                        player.SendNotification(NotificationType.Error, $"Segundo Item de {item.Identifier} não é uma amostra de sangue.");
                        return;
                    }
                }
                else if (item.Type == ForensicTestItemType.BulletShell)
                {
                    if (!GlobalFunctions.CheckIfIsBulletShell(originConfiscationItem.GetCategory()))
                    {
                        player.SendNotification(NotificationType.Error, $"Primeiro Item de {item.Identifier} não é uma cápsula.");
                        return;
                    }

                    if (targetConfiscationItem is null)
                    {
                        player.SendNotification(NotificationType.Error, $"Segundo Item de {item.Identifier} é inválido.");
                        return;
                    }

                    if (targetConfiscationItem.GetCategory() != ItemCategory.Weapon
                        && !GlobalFunctions.CheckIfIsBulletShell(targetConfiscationItem.GetCategory()))
                    {
                        player.SendNotification(NotificationType.Error, $"Segundo Item de {item.Identifier} não é uma arma ou uma cápsula.");
                        return;
                    }
                }

                item.Identifier ??= string.Empty;
                if (item.Identifier.Length > 50)
                {
                    player.SendNotification(NotificationType.Error, $"Identificador {item.Identifier} deve ter até 50 caracteres.");
                    return;
                }

                var forensicTestItem = new ForensicTestItem();
                forensicTestItem.Create(item.Type, item.OriginConfiscationItemId, item.TargetConfiscationItemId, item.Identifier);
                forensicTestItems.Add(forensicTestItem);
            }

            var forensicTest = new ForensicTest();
            forensicTest.Create(player.Character.Id, player.Faction!.Id, identifier, forensicTestItems);

            await context.ForensicTests.AddAsync(forensicTest);
            await context.SaveChangesAsync();

            player.SendNotification(NotificationType.Success, "Exame solicitado.");
            player.Emit(Constants.FORENSIC_LABORATORY_PAGE_CLOSE);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}