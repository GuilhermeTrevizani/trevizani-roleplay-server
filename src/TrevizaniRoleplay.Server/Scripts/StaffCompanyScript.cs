using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extesions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Scripts;

public class StaffCompanyScript : Script
{
    [Command("empresas")]
    public async Task CMD_empresas(MyPlayer player)
    {
        if (!player.StaffFlags.Contains(StaffFlag.Companies))
        {
            player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
            return;
        }

        var typesJson = Functions.Serialize(
            Enum.GetValues<CompanyType>()
            .Select(x => new
            {
                Value = x,
                Label = x.GetDisplay(),
            })
            .OrderBy(x => x.Label)
        );

        player.Emit("StaffCompany:Show", await GetCompaniesJson(), typesJson);
    }

    [RemoteEvent(nameof(StaffCompanyGoto))]
    public static void StaffCompanyGoto(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var company = Global.Companies.FirstOrDefault(x => x.Id == id);
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            player.SetPosition(new(company.PosX, company.PosY, company.PosZ), 0, false);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCompanySave))]
    public async Task StaffCompanySave(Player playerParam, string idString, string name, Vector3 pos, int weekRentValue, int type,
        int blipType, int blipColor)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Companies))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            if (name.Length < 1 || name.Length > 100)
            {
                player.SendNotification(NotificationType.Error, "Nome deve ter entre 1 e 100 caracteres.");
                return;
            }

            if (weekRentValue < 0)
            {
                player.SendNotification(NotificationType.Error, "Aluguel Semanal deve ser maior ou igual a 0.");
                return;
            }

            if (!Enum.IsDefined(typeof(CompanyType), Convert.ToByte(type)))
            {
                player.SendNotification(NotificationType.Error, $"Tipo {type} não existe.");
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

            var companyType = (CompanyType)type;
            if (Functions.IsOwnedByState(companyType) && weekRentValue > 0)
            {
                player.SendNotification(NotificationType.Error, "Aluguel Semanal deve ser 0 pois o tipo de empresa é estatal.");
                return;
            }

            var company = new Company();
            var id = idString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(idString);
            if (isNew)
            {
                company.Create(name, pos.X, pos.Y, pos.Z, weekRentValue, companyType,
                    Convert.ToUInt16(blipType), Convert.ToByte(blipColor));
            }
            else
            {
                company = Global.Companies.FirstOrDefault(x => x.Id == id);
                if (company is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                company.Update(name, pos.X, pos.Y, pos.Z, weekRentValue, companyType,
                    Convert.ToUInt16(blipType), Convert.ToByte(blipColor));
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.Companies.AddAsync(company);
            else
                context.Companies.Update(company);

            await context.SaveChangesAsync();

            company.CreateIdentifier();

            if (isNew)
                Global.Companies.Add(company);

            await player.WriteLog(LogType.Staff, $"Gravar Empresa | {Functions.Serialize(company)}", null);
            player.SendNotification(NotificationType.Success, $"Empresa {(isNew ? "criada" : "editada")}.");
            await UpdateCompanies();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCompanyRemove))]
    public async Task StaffCompanyRemove(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Companies))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var company = Global.Companies.FirstOrDefault(x => x.Id == idString.ToGuid());
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (company.CharacterId.HasValue)
            {
                player.SendNotification(NotificationType.Error, "Propriedade possui um dono.");
                return;
            }

            if (company.Characters!.Count > 0)
            {
                player.SendNotification(NotificationType.Error, "Empresa possui funcionários.");
                return;
            }

            if (Global.Properties.Any(x => x.CompanyId == company.Id))
            {
                player.SendNotification(NotificationType.Error, "Empresa possui propriedade vinculada.");
                return;
            }

            if (Global.Doors.Any(x => x.CompanyId == company.Id))
            {
                player.SendNotification(NotificationType.Error, "Empresa possui porta vinculada.");
                return;
            }

            var context = Functions.GetDatabaseContext();
            if (await context.CompaniesSells.AnyAsync(x => x.CompanyId == company.Id))
            {
                player.SendNotification(NotificationType.Error, "Empresa possui histórico de vendas.");
                return;
            }

            if (company.Items!.Count > 0)
                context.CompaniesItems.RemoveRange(company.Items);

            if (company.TuningPrices!.Count > 0)
                context.CompaniesTuningPrices.RemoveRange(company.TuningPrices);

            context.Companies.Remove(company);

            await context.SaveChangesAsync();
            Global.Companies.Remove(company);
            company.RemoveIdentifier();
            await player.WriteLog(LogType.Staff, $"Remover Empresa | {Functions.Serialize(company)}", null);
            player.SendNotification(NotificationType.Success, "Empresa excluída.");
            await UpdateCompanies();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCompanyRemoveOwner))]
    public async Task StaffCompanyRemoveOwner(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Companies))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var id = idString.ToGuid();
            var company = Global.Companies.FirstOrDefault(x => x.Id == id);
            if (company is not null)
            {
                var context = Functions.GetDatabaseContext();
                await company.RemoveOwner(context);
                await player.WriteLog(LogType.Staff, $"Remover Dono Empresa {id}", null);
            }

            player.SendNotification(NotificationType.Success, "Dono da empresa removido.");
            await UpdateCompanies();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task UpdateCompanies()
    {
        var json = await GetCompaniesJson();
        foreach (var target in Global.SpawnedPlayers.Where(x => x.StaffFlags.Contains(StaffFlag.Companies)))
            target.Emit("StaffCompany:Update", json);
    }

    private async Task<string> GetCompaniesJson()
    {
        var context = Functions.GetDatabaseContext();
        var characters = await context.Characters
            .Where(x => Global.Companies.Select(y => y.CharacterId).Contains(x.Id))
            .ToListAsync();

        string GetOwner(Guid? characterId)
        {
            return characters.FirstOrDefault(x => x.Id == characterId)?.Name ?? string.Empty;
        }

        return Functions.Serialize(Global.Companies.OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.PosX,
                x.PosY,
                x.PosZ,
                x.WeekRentValue,
                x.RentPaymentDate,
                Owner = GetOwner(x.CharacterId),
                TypeDisplay = x.Type.GetDisplay(),
                x.Type,
                x.Safe,
                x.BlipType,
                x.BlipColor,
                EmployeeOnDuty = x.EmployeeOnDuty.HasValue ?
                    Global.SpawnedPlayers.FirstOrDefault(y => y.Character.Id == x.EmployeeOnDuty.Value)?.Character?.Name ?? string.Empty
                    :
                    string.Empty,
            }));
    }

    [RemoteEvent(nameof(StaffCompanyShowItems))]
    public async Task StaffCompanyShowItems(Player playerParam, string idString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            var id = idString.ToGuid();
            var company = Global.Companies.FirstOrDefault(x => x.Id == id);
            if (company is null)
                return;

            if (company.Type == CompanyType.MechanicWorkshop)
            {
                await StaffCompanyShowTuningPrices(player, company);
                return;
            }

            player.Emit("StaffCompanyItem:Show", GetCompaniesItemsJson(company), Functions.GetItemsTemplatesResponse(), idString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCompanyItemSave))]
    public async Task StaffCompanyItemSave(Player playerParam, string companyIdString, string companyItemIdString,
        string itemTemplateIdString, int costPrice)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Companies))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var itemTemplate = Global.ItemsTemplates.FirstOrDefault(x => x.Id == itemTemplateIdString.ToGuid());
            if (itemTemplate is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (costPrice <= 0)
            {
                player.SendNotification(NotificationType.Error, "Preço de Custo deve ser maior que 0.");
                return;
            }

            var companyItem = new CompanyItem();
            var id = companyItemIdString.ToGuid();
            var isNew = string.IsNullOrWhiteSpace(companyItemIdString);
            if (isNew)
            {
                companyItem.Create(company.Id, itemTemplate.Id, costPrice);
            }
            else
            {
                companyItem = company.Items!.FirstOrDefault(x => x.Id == id);
                if (companyItem is null)
                {
                    player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                    return;
                }

                companyItem.Update(itemTemplate.Id, costPrice);
            }

            var context = Functions.GetDatabaseContext();
            if (isNew)
                await context.CompaniesItems.AddAsync(companyItem);
            else
                context.CompaniesItems.Update(companyItem);

            await context.SaveChangesAsync();

            if (isNew && !company.Items!.Contains(companyItem))
                company.Items!.Add(companyItem);

            await player.WriteLog(LogType.Staff, $"Gravar Item Empresa | {Functions.Serialize(companyItem)}", null);
            player.SendNotification(NotificationType.Success, $"Item {(isNew ? "criado" : "editado")}.");
            await StaffCompanyShowItems(player, companyIdString);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    [RemoteEvent(nameof(StaffCompanyItemRemove))]
    public async Task StaffCompanyItemRemove(Player playerParam, string companyIdString, string companyItemIdString)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Companies))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            var id = companyItemIdString.ToGuid();
            var companyItem = company.Items!.FirstOrDefault(x => x.Id == id);
            if (companyItem is not null)
            {
                var context = Functions.GetDatabaseContext();
                context.CompaniesItems.Remove(companyItem);
                await context.SaveChangesAsync();
                company.Items!.Remove(companyItem);
                await player.WriteLog(LogType.Staff, $"Remover Item Empresa | {Functions.Serialize(companyItem)}", null);
            }

            player.SendNotification(NotificationType.Success, "Item excluído.");
            await StaffCompanyShowItems(player, companyIdString);
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
                x.ItemTemplateId,
                ItemTemplateName = Global.ItemsTemplates.FirstOrDefault(y => y.Id == x.ItemTemplateId)!.Name,
                x.CostPrice,
                x.SellPrice﻿,
            }));
    }

    private async Task StaffCompanyShowTuningPrices(MyPlayer player, Company company)
    {
        var types = Enum.GetValues<CompanyTuningPriceType>();
        foreach (var type in types)
        {
            var tuningPrice = company.TuningPrices!.FirstOrDefault(x => x.Type == type);
            if (tuningPrice is null)
            {
                tuningPrice = new();
                tuningPrice.Create(company.Id, type, 1);
                var context = Functions.GetDatabaseContext();
                await context.CompaniesTuningPrices.AddAsync(tuningPrice);
                await context.SaveChangesAsync();
                if (!company.TuningPrices!.Contains(tuningPrice))
                    company.TuningPrices.Add(tuningPrice);
            }
        }

        player.Emit("StaffCompanyTuningPrice:Show", GetCompaniesTuningPricesJson(company), company.Id.ToString());
    }

    private static string GetCompaniesTuningPricesJson(Company company)
    {
        return Functions.Serialize(company.TuningPrices
            !.Select(x => new
            {
                x.Id,
                Type = x.Type.GetDisplay(),
                x.CostPercentagePrice,
                x.SellPercentagePrice,
            })
            .OrderBy(x => x.Type));
    }

    [RemoteEvent(nameof(StaffCompanyTuningPriceSave))]
    public async Task StaffCompanyTuningPriceSave(Player playerParam, string companyIdString, string companyTuningPriceIdString,
        float costPercentagePrice)
    {
        try
        {
            var player = Functions.CastPlayer(playerParam);
            if (!player.StaffFlags.Contains(StaffFlag.Companies))
            {
                player.SendNotification(NotificationType.Error, Globalization.YOU_ARE_NOT_AUTHORIZED);
                return;
            }

            var company = Global.Companies.FirstOrDefault(x => x.Id == companyIdString.ToGuid());
            if (company is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            if (costPercentagePrice <= 0)
            {
                player.SendNotification(NotificationType.Error, "Preço de Custo em Porcentagem deve ser maior que 0.");
                return;
            }

            var companyTuningPrice = company.TuningPrices!.FirstOrDefault(x => x.Id == companyTuningPriceIdString.ToGuid());
            if (companyTuningPrice is null)
            {
                player.SendNotification(NotificationType.Error, Globalization.RECORD_NOT_FOUND);
                return;
            }

            companyTuningPrice.SetCostPercentagePrice(costPercentagePrice);
            var context = Functions.GetDatabaseContext();
            context.CompaniesTuningPrices.Update(companyTuningPrice);
            await context.SaveChangesAsync();

            await player.WriteLog(LogType.Staff, $"Gravar Preço Tuning | {Functions.Serialize(companyTuningPrice)}", null);
            player.SendNotification(NotificationType.Success, "Preço editado.");
            await StaffCompanyShowTuningPrices(player, company);
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}