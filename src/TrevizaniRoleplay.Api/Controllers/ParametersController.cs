using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrevizaniRoleplay.Core.Extensions;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Core.Models.Responses;
using TrevizaniRoleplay.Core.Models.Server;
using TrevizaniRoleplay.Core.Models.Settings;
using TrevizaniRoleplay.Core.Services;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Controllers;

[Route("parameters")]
public class ParametersController(DatabaseContext context) : BaseController(context)
{
    [HttpGet("who-can-login"), Authorize(Policy = PolicySettings.POLICY_MANAGEMENT)]
    public IEnumerable<SelectOptionResponse> GetWhoCanLogin()
    {
        return Enum.GetValues<WhoCanLogin>()
           .Select(x => new SelectOptionResponse
           {
               Value = (byte)x,
               Label = x.GetDescription(),
           })
           .OrderBy(x => x.Label);
    }

    [HttpGet, Authorize(Policy = PolicySettings.POLICY_MANAGEMENT)]
    public async Task<Parameter> Get()
    {
        var parameter = await context.Parameters.FirstOrDefaultAsync()
            ?? throw new ArgumentException("Parâmetros não encotrados.");

        var itemsTemplates = await context.ItemsTemplates.ToListAsync();

        var weaponsInfos = Deserialize<IEnumerable<WeaponInfo>>(parameter.WeaponsInfosJSON)!;
        foreach (var weaponInfo in weaponsInfos)
        {
            weaponInfo.AmmoItemTemplateName = itemsTemplates
                .FirstOrDefault(x => x.Id == weaponInfo.AmmoItemTemplateId)?.Name ?? string.Empty;

            foreach (var component in weaponInfo.Components)
                component.ItemTemplateName = itemsTemplates
                    .FirstOrDefault(x => x.Id == component.ItemTemplateId)?.Name ?? string.Empty;
        }

        parameter!.SetWeaponsInfosJSON(Serialize(weaponsInfos));

        return parameter;
    }

    [HttpPut, Authorize(Policy = PolicySettings.POLICY_MANAGEMENT)]
    public async Task Put([FromBody] ParametersRequest parameter)
    {
        var parameters = await context.Parameters.FirstOrDefaultAsync()
            ?? throw new ArgumentException("Parâmetros não encotrados.");

        if (parameter.InitialTimeCrackDen < 0 || parameter.InitialTimeCrackDen > 23)
            throw new ArgumentException("Hora Inicial para Uso da Boca de Fumo não foi preenchida corretamente.");

        if (parameter.EndTimeCrackDen < 0 || parameter.EndTimeCrackDen > 23)
            throw new ArgumentException("Hora Final para Uso da Boca de Fumo não foi preenchida corretamente.");

        var vehicleDismantlingPartsChances = Deserialize<List<VehicleDismantlingPartsChance>>(parameter.VehicleDismantlingPartsChanceJSON)!;
        if (vehicleDismantlingPartsChances.Sum(x => x.Percentage) != 100)
            throw new ArgumentException("Probabilidade de partes de veículo em um desmanche não foi configurada corretamente.");

        if (vehicleDismantlingPartsChances.GroupBy(x => x.Percentage).Count() != vehicleDismantlingPartsChances.Count)
            throw new ArgumentException("Porcentagem repetida em probabilidade de partes de veículo em um desmanche.");

        var fishingItemsChances = Deserialize<List<FishingItemChance>>(parameter.FishingItemsChanceJSON)!;
        if (fishingItemsChances.Sum(x => x.Percentage) != 100)
            throw new ArgumentException("Probabilidade de itens na pesca não foi configurada corretamente.");

        if (fishingItemsChances.GroupBy(x => x.Percentage).Count() != fishingItemsChances.Count)
            throw new ArgumentException("Porcentagem repetida em probabilidade de itens na pesca.");

        var itemsTemplates = await context.ItemsTemplates.ToListAsync();

        foreach (var fishingItemChance in fishingItemsChances)
        {
            if (!itemsTemplates.Any(x => x.Name.ToLower() == fishingItemChance.ItemTemplateName.ToLower()))
                throw new ArgumentException($"Item {fishingItemChance.ItemTemplateName} não encontrado.");
        }

        var weaponsInfos = Deserialize<List<WeaponInfo>>(parameter.WeaponsInfosJSON)!;
        foreach (var weaponInfo in weaponsInfos)
        {
            if (!GlobalFunctions.CheckIfWeaponExists(weaponInfo.Name))
                throw new ArgumentException($"Arma {weaponInfo.Name} não encontrada.");

            if (weaponInfo.Recoil < 0)
                throw new ArgumentException($"Recuo da arma {weaponInfo.Name} deve ser maior ou igual a 0.");

            if (weaponInfo.Damage <= 0)
                throw new ArgumentException($"Dano da arma {weaponInfo.Name} deve ser maior que 0.");

            if (string.IsNullOrWhiteSpace(weaponInfo.AmmoItemTemplateName))
            {
                weaponInfo.AmmoItemTemplateId = null;
            }
            else
            {
                var ammoItemTemplate = itemsTemplates
                    .FirstOrDefault(x => x.Name.ToLower() == weaponInfo.AmmoItemTemplateName.ToLower())
                    ?? throw new ArgumentException($"Munição {weaponInfo.AmmoItemTemplateName} não encontrada.");

                if (!GlobalFunctions.CheckIfIsAmmo(ammoItemTemplate.Category))
                    throw new ArgumentException($"Item {weaponInfo.AmmoItemTemplateName} não é uma munição.");

                weaponInfo.AmmoItemTemplateId = ammoItemTemplate.Id;

                foreach (var component in weaponInfo.Components)
                {
                    var componentItemTemplate = itemsTemplates
                        .FirstOrDefault(x => x.Name.ToLower() == component.ItemTemplateName.ToLower())
                        ?? throw new ArgumentException($"Componente {component.ItemTemplateName} não encontrado.");

                    if (ammoItemTemplate.Category == ItemCategory.WeaponComponent)
                        throw new ArgumentException($"Item {component.ItemTemplateName} não é um componente.");

                    component.ItemTemplateId = componentItemTemplate.Id;
                }
            }
        }

        parameter.WeaponsInfosJSON = Serialize(weaponsInfos);

        var oldParameter = Serialize(parameters);
        parameters.Update(parameter.HospitalValue, parameter.BarberValue,
            parameter.ClothesValue, parameter.DriverLicenseBuyValue, parameter.Paycheck, parameter.DriverLicenseRenewValue,
            parameter.AnnouncementValue, parameter.ExtraPaymentGarbagemanValue, parameter.Blackout,
            parameter.IPLsJSON ?? "[]", parameter.TattooValue, parameter.CooldownDismantleHours,
            parameter.PropertyRobberyConnectedTime, parameter.CooldownPropertyRobberyRobberHours, parameter.CooldownPropertyRobberyPropertyHours,
            parameter.PoliceOfficersPropertyRobbery, parameter.InitialTimeCrackDen, parameter.EndTimeCrackDen, parameter.FirefightersBlockHeal,
            parameter.FuelValue, parameter.PropertyProtectionLevelPercentageValue,
            parameter.VehicleDismantlingPercentageValue, parameter.VehicleDismantlingSeizedDays,
            parameter.VehicleDismantlingPartsChanceJSON, parameter.VehicleDismantlingMinutes, parameter.PlasticSurgeryValue,
            parameter.WhoCanLogin, parameter.FishingItemsChanceJSON, parameter.VehicleInsurancePercentage,
            parameter.PrisonInsidePosX, parameter.PrisonInsidePosY, parameter.PrisonInsidePosZ, parameter.PrisonInsideDimension,
            parameter.PrisonOutsidePosX, parameter.PrisonOutsidePosY, parameter.PrisonOutsidePosZ, parameter.PrisonOutsideDimension,
            parameter.WeaponsInfosJSON, parameter.BodyPartsDamagesJSON, parameter.WeaponLicenseMonths, parameter.WeaponLicenseMaxWeapon, parameter.WeaponLicenseMaxAmmo,
            parameter.WeaponLicenseMaxAttachment, parameter.WeaponLicensePurchaseDaysInterval, parameter.PremiumItemsJSON,
            parameter.AudioRadioStationsJSON, parameter.UnemploymentAssistance, parameter.PremiumPointPackagesJSON,
            parameter.MOTD, parameter.EntranceBenefitValue, parameter.EntranceBenefitCooldownUsers, parameter.EntranceBenefitCooldownHours);

        context.Parameters.Update(parameters);

        var ucpAction = new UCPAction();
        ucpAction.Create(UCPActionType.ReloadParameters, UserId, string.Empty);
        await context.UCPActions.AddAsync(ucpAction);

        await context.SaveChangesAsync();

        await WriteLog(LogType.Staff, $"Parâmetros | {oldParameter} | {Serialize(parameters)}");
    }
}