using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class CompanyScript : Script
{
    [Command("empresa")]
    public static void CMD_empresa(MyPlayer player)
    {
        if (player.Companies.Count == 0)
        {
            player.SendMessage(MessageType.Error, "Você não está em nenhuma empresa.");
            return;
        }

        player.Emit("Company:Show", GetCompaniesJsonByCharacter(player));
    }

    [RemoteEvent(nameof(CompanySave))]
    public async Task CompanySave(Player playerParam, string idString, string color, int blipType, int blipColor)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (company?.CharacterId != player.Character.Id)
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            color = color?.Replace("#", string.Empty) ?? string.Empty;
            if (color.Length != 6)
            {
                player.SendNotification(NotificationType.Error, "Cor deve possuir 6 caracteres.");
                return;
            }

            if (blipType < 1 || blipType > Constants.MAX_BLIP_TYPE)
            {
                player.SendNotification(NotificationType.Error, string.Format("Tipo do Blip deve ser entre 1 e {0}.", Constants.MAX_BLIP_TYPE));
                return;
            }

            if (blipColor < 1 || blipColor > 85)
            {
                player.SendNotification(NotificationType.Error, "Cor do Blip deve ser entre 1 e 85.");
                return;
            }

            company.Update(color, Convert.ToUInt16(blipType), Convert.ToByte(blipColor));

            var context = Functions.GetDatabaseContext();
            context.Companies.Update(company);
            await context.SaveChangesAsync();

            player.SendNotification(NotificationType.Success, $"Empresa editada.");

            await player.WriteLog(LogType.Company, $"Gravar | {Functions.Serialize(company)}", null);
            player.Emit("Company:Show", GetCompaniesJsonByCharacter(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyEmployees))]
    public async Task CompanyEmployees(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var company = Global.Companies.FirstOrDefault(x => x.Id == id);
            if (company is null || !player.CheckCompanyPermission(company, null))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var companyCharacter = company.Characters!.FirstOrDefault(x => x.CharacterId == player.Character.Id);

            var companyFlagsJSON = Functions.Serialize(
                Enum.GetValues<CompanyFlag>()
                .Select(x => new
                {
                    Value = x,
                    Label = x.GetDescription(),
                })
                .OrderBy(x => x.Label)
            );

            var context = Functions.GetDatabaseContext();
            var characters = (await context.CompaniesCharacters
                .Where(x => x.CompanyId == company.Id)
                .Include(x => x.Character)
                    .ThenInclude(x => x!.User)
                .ToListAsync())
                .Select(x => new
                {
                    x.Character!.Id,
                    x.Character.Name,
                    x.Character.LastAccessDate,
                    x.FlagsJSON,
                    User = x.Character.User!.Name,
                    IsOnline = Global.SpawnedPlayers.Any(y => y.Character.Id == x.CharacterId),
                })
                .OrderByDescending(x => x.IsOnline)
                .ThenBy(x => x.Name);

            player.Emit("CompanyCharacters",
                Functions.Serialize(characters),
                company.CharacterId == player.Character.Id ? Functions.Serialize(Enum.GetValues<CompanyFlag>()) : companyCharacter?.FlagsJSON ?? "[]",
                company.Id.ToString(), company.Name, companyFlagsJSON);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyOpenClose))]
    public async Task CompanyOpenClose(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.Open))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (company.Color.Length != 6)
            {
                player.SendNotification(NotificationType.Error, "Cor deve possuir 6 caracteres.");
                return;
            }

            if (company.BlipType < 1 || company.BlipType > Constants.MAX_BLIP_TYPE)
            {
                player.SendNotification(NotificationType.Error, string.Format("Tipo do Blip deve ser entre 1 e {0}.", Constants.MAX_BLIP_TYPE));
                return;
            }

            if (company.BlipColor < 1 || company.BlipColor > 85)
            {
                player.SendNotification(NotificationType.Error, "Cor do Blip deve ser entre 1 e 85.");
                return;
            }

            company.ToggleOpen();

            var context = Functions.GetDatabaseContext();
            await player.WriteLog(LogType.Company, $"{(company.GetIsOpen() ? "Abrir" : "fechou")} Empresa {company.Name} ({company.Id})", null);
            player.SendNotification(NotificationType.Success, $"Você {(company.GetIsOpen() ? "abriu" : "fechou")} a empresa {company.Name}.");
            player.Emit("Company:Show", GetCompaniesJsonByCharacter(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyAnnounce))]
    public async Task CompanyAnnounce(Player playerParam, string idString, string message)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.Open))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (!company.GetIsOpen())
            {
                player.SendNotification(NotificationType.Error, "A empresa está fechada.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            await company.Announce(player, message);
            player.Emit("Company:Show", GetCompaniesJsonByCharacter(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyCharacterInvite))]
    public async Task CompanyCharacterInvite(Player playerParam, string companyIdString, int characterSessionId)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var companyId = new Guid(companyIdString);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyId);
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.InviteEmployee))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == characterSessionId);
            if (target is null)
            {
                player.SendNotification(NotificationType.Error, $"Nenhum personagem online com o ID {characterSessionId}.");
                return;
            }

            if (company.CharacterId == target.Character.Id || company.Characters!.Any(x => x.CharacterId == target.Character.Id))
            {
                player.SendNotification(NotificationType.Error, "Personagem já está nessa empresa.");
                return;
            }

            var invite = new Invite
            {
                Type = InviteType.Company,
                SenderCharacterId = player.Character.Id,
                Value = [company.Id.ToString()],
            };
            target.Invites.RemoveAll(x => x.Type == InviteType.Company);
            target.Invites.Add(invite);

            player.SendNotification(NotificationType.Success, $"Você convidou {target.Character.Name} para {company.Name}.");
            target.SendMessage(MessageType.Success, $"{player.User.Name} convidou você para a empresa {company.Name}. (/ac {(int)invite.Type} para aceitar ou /rc {(int)invite.Type} para recusar)");

            var context = Functions.GetDatabaseContext();
            await player.WriteLog(LogType.Company, $"Convidar Empresa {companyId}", target);
            await CompanyEmployees(player, companyIdString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyCharacterSave))]
    public async Task CompanyCharacterSave(Player playerParam, string companyIdString, string characterIdString, string flagsJSON)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.EditEmployee))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var companyCharacterTarget = company.Characters!.FirstOrDefault(x => x.CharacterId == characterIdString.ToGuid());
            if (companyCharacterTarget is null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem não está na empresa {company.Id}.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            companyCharacterTarget.SetFlagsJSON(flagsJSON);
            context.CompaniesCharacters.Update(companyCharacterTarget);
            await context.SaveChangesAsync();

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == companyCharacterTarget.CharacterId);
            target?.SendMessage(MessageType.Success, $"{player.User.Name} alterou suas informações na empresa {company.Name}.");

            player.SendNotification(NotificationType.Success, "Você alterou as informações do personagem na empresa.");
            await player.WriteLog(LogType.Company, $"Salvar Funcionário Empresa {company.Name} {companyCharacterTarget.CharacterId} {flagsJSON}", target);

            await CompanyEmployees(player, companyIdString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyCharacterRemove))]
    public async Task CompanyCharacterRemove(Player playerParam, string companyIdString, string characterIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var companyId = new Guid(companyIdString);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyId);
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.RemoveEmployee))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var characterId = new Guid(characterIdString);
            var companyCharacterTarget = company.Characters!.FirstOrDefault(x => x.CharacterId == characterId);
            if (companyCharacterTarget is null)
            {
                player.SendNotification(NotificationType.Error, $"Personagem {characterId} não está na empresa {companyId}.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            context.CompaniesCharacters.Remove(companyCharacterTarget);
            await context.SaveChangesAsync();
            company.Characters!.Remove(companyCharacterTarget);

            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == characterId);
            target?.SendMessage(MessageType.Success, $"{player.User.Name} expulsou você da empresa {company.Name}.");

            player.SendNotification(NotificationType.Success, $"Você expulsou o personagem {characterId} da empresa.");
            await player.WriteLog(LogType.Company, $"Expulsar Empresa {companyId} {characterId}", target);

            await CompanyEmployees(player, companyIdString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetCompaniesJsonByCharacter(MyPlayer player)
    {
        return Functions.Serialize(player.Companies
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.WeekRentValue,
                x.RentPaymentDate,
                x.Color,
                x.BlipType,
                x.BlipColor,
                x.Safe,
                IsOpen = x.GetIsOpen(),
                IsOwner = x.CharacterId == player.Character.Id,
                CanOpen = player.CheckCompanyPermission(x, CompanyFlag.Open),
                CanUseSafe = player.CheckCompanyPermission(x, CompanyFlag.Safe),
            }));
    }

    [Command("alugarempresa")]
    public async Task CMD_alugarempresa(MyPlayer player)
    {
        var company = Global.Companies.Where(x => !x.CharacterId.HasValue && x.WeekRentValue > 0
            && player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE)
        .MinBy(x => player.GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE);
        if (company is null)
        {
            player.SendNotification(NotificationType.Error, "Você não está perto de uma empresa disponível para alugar.");
            return;
        }

        if (player.Money < company.WeekRentValue)
        {
            player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, company.WeekRentValue));
            return;
        }

        var context = Functions.GetDatabaseContext();
        await player.RemoveMoney(company.WeekRentValue);

        company.Rent(player.Character.Id);
        company.RemoveIdentifier();

        context.Companies.Update(company);
        await context.SaveChangesAsync();

        await player.WriteLog(LogType.Company, $"/alugarempresa {company.Name} {company.Id} {company.WeekRentValue}", null);

        player.SendMessage(MessageType.Success, $"Você alugou {company.Name} por 7 dias por ${company.WeekRentValue:N0}.");
        player.SendMessage(MessageType.Success, $"O próximo pagamento será em {company.RentPaymentDate} e será debitado da sua conta bancária. Se você não possuir este valor, a empresa será retirada do seu nome.");
    }

    [RemoteEvent(nameof(CompanyShowItems))]
    public static void CompanyShowItems(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var company = Global.Companies.FirstOrDefault(x => x.Id == id);
            if (company is null || !player.CheckCompanyPermission(company, null))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            var companyCharacter = company.Characters!.FirstOrDefault(x => x.CharacterId == player.Character.Id);
            var flagsJson = company.CharacterId == player.Character.Id ? Functions.Serialize(Enum.GetValues<CompanyFlag>()) : companyCharacter?.FlagsJSON ?? "[]";

            if (company.Type == CompanyType.MechanicWorkshop)
            {
                player.Emit("CompanyTuningPrice:Show", idString, company.Name, flagsJson, GetCompaniesTuningPricesJson(company));
                return;
            }

            player.Emit("CompanyItem:Show", idString, company.Name, flagsJson, GetCompaniesItemsJson(company));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanyItemSave))]
    public async Task CompanyItemSave(Player playerParam, string companyIdString, string companyItemIdString, int sellPrice)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.ManageItems))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (sellPrice <= 0)
            {
                player.SendNotification(NotificationType.Error, "Preço de Venda deve ser maior que 0.");
                return;
            }

            var companyItemId = companyItemIdString.ToGuid();
            var companyItem = company.Items!.FirstOrDefault(x => x.Id == companyItemId);
            if (companyItem is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var context = Functions.GetDatabaseContext();
            companyItem.Update(sellPrice);
            context.CompaniesItems.Update(companyItem);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Company, $"Gravar Item Empresa | {Functions.Serialize(companyItem)}", null);
            player.SendNotification(NotificationType.Success, "Item editado.");
            CompanyShowItems(player, companyIdString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetCompaniesItemsJson(Company company)
    {
        return Functions.Serialize(company.Items
            !.OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                ItemTemplateName = Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)!.Name,
                x.CostPrice,
                x.SellPrice﻿,
            }));
    }

    [RemoteEvent(nameof(CompanyBuyItem))]
    public async Task CompanyBuyItem(Player playerParam, string companyIdString, string companyItemIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var companyItem = company.Items!.FirstOrDefault(x => x.Id == companyItemIdString.ToGuid());
            if (companyItem is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == companyItem.ItemTemplateId)!;

            var value = Functions.IsOwnedByState(company.Type) ? companyItem.CostPrice : companyItem.SellPrice;
            if (value < companyItem.CostPrice)
            {
                player.SendNotification(NotificationType.Error, $"Preço de Venda de {itemTemplate.Name} é menor que o Preço de Custo. Por favor, reporte o bug.");
                return;
            }

            var quantity = 1;
            value *= 1;

            var costPrice = companyItem.CostPrice * quantity;

            if (player.Money < value)
            {
                player.SendNotification(NotificationType.Error, string.Format(Resources.YouDontHaveEnoughMoney, value));
                return;
            }

            var context = Functions.GetDatabaseContext();
            var isAmmo = Functions.CheckIfIsAmmo(itemTemplate.Category);
            if ((itemTemplate.Category == ItemCategory.Weapon && Functions.IsWeaponWithAmmo(itemTemplate.Type))
                || itemTemplate.Category == ItemCategory.WeaponComponent
                || isAmmo)
            {
                if ((player.Character.WeaponLicenseValidDate ?? DateTime.MinValue).Date < DateTime.Now.Date)
                {
                    player.SendNotification(NotificationType.Error, "Você não possui uma licença de armas válida.");
                    return;
                }

                var itemCount = 0;
                var date = DateTime.Now.AddDays(-Global.Parameter.WeaponLicensePurchaseDaysInterval);
                if (isAmmo)
                {
                    itemCount = await context.CompaniesSells.Where(x => x.CharacterId == player.Character.Id
                        && (x.ItemTemplate!.Category == ItemCategory.PistolAmmo || x.ItemTemplate!.Category == ItemCategory.ShotgunAmmo
                            || x.ItemTemplate!.Category == ItemCategory.AssaultRifleAmmo || x.ItemTemplate!.Category == ItemCategory.LightMachineGunAmmo
                            || x.ItemTemplate!.Category == ItemCategory.SniperRifleAmmo || x.ItemTemplate!.Category == ItemCategory.SubMachineGunAmmo)).SumAsync(x => x.Quantity);
                }
                else if (itemTemplate.Category == ItemCategory.Weapon)
                {
                    var weaponsToCheck = Global.WeaponsInfos
                        .Where(x => x.AmmoItemTemplateId.HasValue)
                        .Select(x => Functions.GetWeaponType(x.Name))
                        .ToList();

                    itemCount = await context.CompaniesSells.Where(x => x.CharacterId == player.Character.Id
                        && x.ItemTemplate!.Category == ItemCategory.Weapon
                        && weaponsToCheck.Contains(x.ItemTemplate!.Type)).SumAsync(x => x.Quantity);
                }
                else
                {
                    itemCount = await context.CompaniesSells.Where(x => x.CharacterId == player.Character.Id
                        && x.ItemTemplate!.Category == itemTemplate.Category).SumAsync(x => x.Quantity);
                }

                itemCount += quantity;

                if (isAmmo)
                {
                    if (itemCount > Global.Parameter.WeaponLicenseMaxAmmo)
                    {
                        player.SendNotification(NotificationType.Error, $"Compra irá ultrapassar o máximo ({Global.Parameter.WeaponLicenseMaxAmmo}) de munições permitidas em {Global.Parameter.WeaponLicensePurchaseDaysInterval} dias.");
                        return;
                    }
                }
                else if (itemTemplate.Category == ItemCategory.Weapon)
                {
                    if (itemCount > Global.Parameter.WeaponLicenseMaxWeapon)
                    {
                        player.SendNotification(NotificationType.Error, $"Compra irá ultrapassar o máximo ({Global.Parameter.WeaponLicenseMaxWeapon}) de armas permitidas em {Global.Parameter.WeaponLicensePurchaseDaysInterval} dias.");
                        return;
                    }
                }
                else if (itemTemplate.Category == ItemCategory.WeaponComponent)
                {
                    if (itemCount > Global.Parameter.WeaponLicenseMaxAttachment)
                    {
                        player.SendNotification(NotificationType.Error, $"Compra irá ultrapassar o máximo ({Global.Parameter.WeaponLicenseMaxAttachment}) de componentes de armas permitidos em {Global.Parameter.WeaponLicensePurchaseDaysInterval} dias.");
                        return;
                    }
                }
            }

            var characterItem = new CharacterItem();
            characterItem.Create(companyItem.ItemTemplateId, 0, quantity, null);
            var res = await player.GiveItem(characterItem);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            await player.RemoveMoney(value);

            Guid? serialNumber = null;
            if (itemTemplate.Category == ItemCategory.Weapon)
            {
                var extra = Functions.Deserialize<WeaponItem>(characterItem.Extra!);
                serialNumber = extra.SerialNumber = extra.Id;
                characterItem.SetExtra(Functions.Serialize(extra));
                context.CharactersItems.Update(characterItem);
                await context.SaveChangesAsync();
            }
            else if (itemTemplate.Category == ItemCategory.WeaponComponent)
            {
                var extra = Functions.Deserialize<WeaponComponentItem>(characterItem.Extra!);
                serialNumber = extra.SerialNumber = Guid.NewGuid();
                characterItem.SetExtra(Functions.Serialize(extra));
                context.CharactersItems.Update(characterItem);
                await context.SaveChangesAsync();
            }

            var companySell = new CompanySell();
            companySell.Create(company.Id, player.Character.Id, itemTemplate.Id, quantity, companyItem.CostPrice, companyItem.SellPrice, serialNumber);

            await context.CompaniesSells.AddAsync(companySell);
            await context.SaveChangesAsync();

            var safeValue = value - costPrice;
            if (safeValue > 0)
                company.DepositSafe(safeValue);

            await player.WriteLog(LogType.Company, $"Comprar Item Empresa {company.Id} {company.Name} {itemTemplate.Id} {itemTemplate.Name} {value} {costPrice} {safeValue}", null);
            player.SendNotification(NotificationType.Success, $"Você comprou {quantity}x {itemTemplate.Name} por ${value:N0}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanySafeWithdraw))]
    public async Task CompanySafeWithdraw(Player playerParam, string idString, int value)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var company = Global.Companies.FirstOrDefault(x => x.Id == id);
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.Safe))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (player.GetPosition().DistanceTo(new(company.PosX, company.PosY, company.PosZ)) > Constants.RP_DISTANCE)
            {
                var companyProperty = Global.Properties.FirstOrDefault(x => x.Number == player.GetDimension() && x.CompanyId == company.Id);
                if (companyProperty is null)
                {
                    player.SendNotification(NotificationType.Error, "Você não está na empresa.");
                    return;
                }
            }

            if (value <= 0 || value > company.Safe)
            {
                player.SendNotification(NotificationType.Error, "Cofre não possui esse valor.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var res = await player.GiveMoney(value);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            company.WithdrawSafe(value);
            context.Companies.Update(company);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Company, $"Sacar Cofre {company.Id} {company.Name} {value}", null);
            player.SendNotification(NotificationType.Success, $"Você sacou ${value:N0} do cofre de {company.Name}.");
            player.Emit("Company:Show", GetCompaniesJsonByCharacter(player));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private static string GetCompaniesTuningPricesJson(Company company)
    {
        return Functions.Serialize(company.TuningPrices
            !.Select(x => new
            {
                x.Id,
                Type = x.Type.GetDescription(),
                x.CostPercentagePrice,
                x.SellPercentagePrice,
            })
            .OrderBy(x => x.Type));
    }

    [RemoteEvent(nameof(CompanyTuningPriceSave))]
    public async Task CompanyTuningPriceSave(Player playerParam, string companyIdString, string companyTuningPriceIdString, float sellPercentagePrice)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null || !player.CheckCompanyPermission(company, CompanyFlag.ManageItems))
            {
                player.SendNotification(NotificationType.Error, Resources.YouAreNotAuthorizedToUseThisCommand);
                return;
            }

            if (sellPercentagePrice <= 0)
            {
                player.SendNotification(NotificationType.Error, "Preço de Venda em Porcentagem deve ser maior que 0.");
                return;
            }

            var companyTuningPrice = company.TuningPrices!.FirstOrDefault(x => x.Id == companyTuningPriceIdString.ToGuid());
            if (companyTuningPrice is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var context = Functions.GetDatabaseContext();
            companyTuningPrice.SetSellPercentagePrice(sellPercentagePrice);
            context.CompaniesTuningPrices.Update(companyTuningPrice);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Company, $"Gravar Preço Tuning | {Functions.Serialize(companyTuningPrice)}", null);
            player.SendNotification(NotificationType.Success, "Preço editado.");
            CompanyShowItems(player, companyIdString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(CompanySellItem))]
    public async Task CompanySellItem(Player playerParam, string companyIdString, string companyItemIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var companyItem = company.Items!.FirstOrDefault(x => x.Id == companyItemIdString.ToGuid());
            if (companyItem is null)
            {
                player.SendNotification(NotificationType.Error, Resources.RecordNotFound);
                return;
            }

            var characterItem = player.Items.FirstOrDefault(x => x.ItemTemplateId == companyItem.ItemTemplateId);
            if (characterItem is null)
            {
                var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == companyItem.ItemTemplateId)!;
                player.SendNotification(NotificationType.Error, $"Você não possui {itemTemplate.Name}.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            var res = await player.GiveMoney(companyItem.CostPrice);
            if (!string.IsNullOrWhiteSpace(res))
            {
                player.SendNotification(NotificationType.Error, res);
                return;
            }

            if (characterItem.GetIsStack())
                await player.RemoveStackedItem(characterItem.ItemTemplateId, 1);
            else
                await player.RemoveItem(characterItem);

            await player.WriteLog(LogType.Company, $"Vender Item Empresa {company.Id} {company.Name} {characterItem.ItemTemplateId} {characterItem.GetName()} {companyItem.CostPrice}", null);
            player.SendNotification(NotificationType.Success, $"Você vendeu {characterItem.GetName()} por ${companyItem.CostPrice:N0}.");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}