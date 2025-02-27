using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TrevizaniRoleplay.Core.Extesions;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server;

public static class Functions
{
    public static async Task<string> GenerateVehiclePlate(bool government)
    {
        var context = GetDatabaseContext();
        var plate = string.Empty;
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        do
        {
            if (government)
                plate = $"1{random.Next(0, 9999999).ToString().PadLeft(7, '0')}";
            else
                plate = $"{chars[random.Next(25)]}{chars[random.Next(25)]}{chars[random.Next(25)]}{random.Next(0, 999).ToString().PadLeft(3, '0')}";
        } while (await context.Vehicles.AnyAsync(x => x.Plate == plate));
        return plate;
    }

    public static List<string> GetIPLsByInterior(PropertyInterior propertyInterior)
    {
        return propertyInterior switch
        {
            PropertyInterior.Modern1Apartment => ["apa_v_mp_h_01_a"],
            PropertyInterior.Modern2Apartment => ["apa_v_mp_h_01_c"],
            PropertyInterior.Modern3Apartment => ["apa_v_mp_h_01_b"],
            PropertyInterior.Mody1Apartment => ["apa_v_mp_h_02_a"],
            PropertyInterior.Mody2Apartment => ["apa_v_mp_h_02_c"],
            PropertyInterior.Mody3Apartment => ["apa_v_mp_h_02_b"],
            PropertyInterior.Vibrant1Apartment => ["apa_v_mp_h_03_a"],
            PropertyInterior.Vibrant2Apartment => ["apa_v_mp_h_03_c"],
            PropertyInterior.Vibrant3Apartment => ["apa_v_mp_h_03_b"],
            PropertyInterior.Sharp1Apartment => ["apa_v_mp_h_04_a"],
            PropertyInterior.Sharp2Apartment => ["apa_v_mp_h_04_c"],
            PropertyInterior.Sharp3Apartment => ["apa_v_mp_h_04_b"],
            PropertyInterior.Monochrome1Apartment => ["apa_v_mp_h_05_a"],
            PropertyInterior.Monochrome2Apartment => ["apa_v_mp_h_05_c"],
            PropertyInterior.Monochrome3Apartment => ["apa_v_mp_h_05_b"],
            PropertyInterior.Seductive1Apartment => ["apa_v_mp_h_06_a"],
            PropertyInterior.Seductive2Apartment => ["apa_v_mp_h_06_c"],
            PropertyInterior.Seductive3Apartment => ["apa_v_mp_h_06_b"],
            PropertyInterior.Regal1Apartment => ["apa_v_mp_h_07_a"],
            PropertyInterior.Regal2Apartment => ["apa_v_mp_h_07_c"],
            PropertyInterior.Regal3Apartment => ["apa_v_mp_h_07_b"],
            PropertyInterior.Aqua1Apartment => ["apa_v_mp_h_08_a"],
            PropertyInterior.Aqua2Apartment => ["apa_v_mp_h_08_c"],
            PropertyInterior.Aqua3Apartment => ["apa_v_mp_h_08_b"],
            PropertyInterior.ArcadiusExecutiveRich => ["ex_dt1_02_office_02b"],
            PropertyInterior.ArcadiusExecutiveCool => ["ex_dt1_02_office_02c"],
            PropertyInterior.ArcadiusExecutiveContrast => ["ex_dt1_02_office_02a"],
            PropertyInterior.ArcadiusOldSpiceWarm => ["ex_dt1_02_office_01a"],
            PropertyInterior.ArcadiusOldSpiceClassical => ["ex_dt1_02_office_01b"],
            PropertyInterior.ArcadiusOldSpiceVintage => ["ex_dt1_02_office_01c"],
            PropertyInterior.ArcadiusPowerBrokerIce => ["ex_dt1_02_office_03a"],
            PropertyInterior.ArcadiusPowerBrokeConservative => ["ex_dt1_02_office_03b"],
            PropertyInterior.ArcadiusPowerBrokePolished => ["ex_dt1_02_office_03c"],
            PropertyInterior.MazeBankExecutiveRich => ["ex_dt1_11_office_02b"],
            PropertyInterior.MazeBankExecutiveCool => ["ex_dt1_11_office_02c"],
            PropertyInterior.MazeBankExecutiveContrast => ["ex_dt1_11_office_02a"],
            PropertyInterior.MazeBankOldSpiceWarm => ["ex_dt1_11_office_01a"],
            PropertyInterior.MazeBankOldSpiceClassical => ["ex_dt1_11_office_01b"],
            PropertyInterior.MazeBankOldSpiceVintage => ["ex_dt1_11_office_01c"],
            PropertyInterior.MazeBankPowerBrokerIce => ["ex_dt1_11_office_03a"],
            PropertyInterior.MazeBankPowerBrokeConservative => ["ex_dt1_11_office_03b"],
            PropertyInterior.MazeBankPowerBrokePolished => ["ex_dt1_11_office_03c"],
            PropertyInterior.LomBankExecutiveRich => ["ex_sm_13_office_02b"],
            PropertyInterior.LomBankExecutiveCool => ["ex_sm_13_office_02c"],
            PropertyInterior.LomBankExecutiveContrast => ["ex_sm_13_office_02a"],
            PropertyInterior.LomBankOldSpiceWarm => ["ex_sm_13_office_01a"],
            PropertyInterior.LomBankOldSpiceClassical => ["ex_sm_13_office_01b"],
            PropertyInterior.LomBankOldSpiceVintage => ["ex_sm_13_office_01c"],
            PropertyInterior.LomBankPowerBrokerIce => ["ex_sm_13_office_03a"],
            PropertyInterior.LomBankPowerBrokeConservative => ["ex_sm_13_office_03b"],
            PropertyInterior.LomBankPowerBrokePolished => ["ex_sm_13_office_03c"],
            PropertyInterior.MazeBankWestExecutiveRich => ["ex_sm_15_office_02b"],
            PropertyInterior.MazeBankWestExecutiveCool => ["ex_sm_15_office_02c"],
            PropertyInterior.MazeBankWestExecutiveContrast => ["ex_sm_15_office_02a"],
            PropertyInterior.MazeBankWestOldSpiceWarm => ["ex_sm_15_office_01a"],
            PropertyInterior.MazeBankWestOldSpiceClassical => ["ex_sm_15_office_01b"],
            PropertyInterior.MazeBankWestOldSpiceVintage => ["ex_sm_15_office_01c"],
            PropertyInterior.MazeBankWestPowerBrokerIce => ["ex_sm_15_office_03a"],
            PropertyInterior.MazeBankWestPowerBrokeConservative => ["ex_sm_15_office_03b"],
            PropertyInterior.MazeBankWestPowerBrokePolished => ["ex_sm_15_office_03c"],
            PropertyInterior.Clubhouse1 => ["bkr_biker_interior_placement_interior_0_biker_dlc_int_01_milo"],
            PropertyInterior.Clubhouse2 => ["bkr_biker_interior_placement_interior_1_biker_dlc_int_02_milo"],
            PropertyInterior.MethLab => ["bkr_biker_interior_placement_interior_2_biker_dlc_int_ware01_milo"],
            PropertyInterior.WeedFarm => ["bkr_biker_interior_placement_interior_3_biker_dlc_int_ware02_milo"],
            PropertyInterior.CocaineLockup => ["bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo"],
            PropertyInterior.CounterfeitCashFactory => ["bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo"],
            PropertyInterior.DocumentForgeryOffice => ["bkr_biker_interior_placement_interior_6_biker_dlc_int_ware05_milo"],
            PropertyInterior.WarehouseSmall => ["ex_exec_warehouse_placement_interior_1_int_warehouse_s_dlc_milo"],
            PropertyInterior.WarehouseMedium => ["ex_exec_warehouse_placement_interior_0_int_warehouse_m_dlc_milo"],
            PropertyInterior.WarehouseLarge => ["ex_exec_warehouse_placement_interior_2_int_warehouse_l_dlc_milo"],
            PropertyInterior.Morgue => ["Coroner_Int_On"],
            _ => [],
        };
    }

    public static void SendJobMessage(CharacterJob characterJob, string message, string color = "#FFFFFF")
    {
        foreach (var player in Global.SpawnedPlayers.Where(x => x.Character.Job == characterJob && x.OnDuty))
            player.SendMessage(Models.MessageType.None, message, color);
    }

    public static string GetBodyPartName(BodyPart bodyPart)
    {
        return bodyPart switch
        {
            BodyPart.Pelvis => "Pélvis",
            BodyPart.LeftHip => "Quadril Esquerdo",
            BodyPart.LeftLeg => "Perna Esquerda",
            BodyPart.LeftFoot => "Pé Esquerdo",
            BodyPart.RightHip => "Quadril Direito",
            BodyPart.RightLeg => "Perna Direita",
            BodyPart.RightFoot => "Pé Direito",
            BodyPart.LowerTorso => "Torso Inferior",
            BodyPart.UpperTorso => "Torso Superior",
            BodyPart.Chest => "Peito",
            BodyPart.UnderNeck => "Sob o Pescoço",
            BodyPart.LeftShoulder => "Ombro Esquerdo",
            BodyPart.LeftUpperArm => "Braço Esquerdo",
            BodyPart.LeftElbow => "Cotovelo Esquerdo",
            BodyPart.LeftWrist => "Pulso Esquerdo",
            BodyPart.RightShoulder => "Ombro Direito",
            BodyPart.RightUpperArm => "Braço Direito",
            BodyPart.RightElbow => "Cotovelo Direito",
            BodyPart.RightWrist => "Pulso Direito",
            BodyPart.Neck => "Pescoço",
            BodyPart.Head => "Cabeça",
            _ => "Desconhecida",
        };
    }

    public static void GetException(Exception ex) => ConsoleLog($"{ex.InnerException?.Message ?? ex.Message} - {ex.Source} - {ex.StackTrace}");

    public static async Task SendServerMessage(string message, UserStaff minUserStaff, bool discord)
    {
        foreach (var player in Global.SpawnedPlayers.Where(x => x.User.Staff >= minUserStaff))
        {
            if (minUserStaff != UserStaff.None && player.StaffToggle)
                continue;

            player.SendMessage(Models.MessageType.None, $"[{Constants.SERVER_INITIALS}] {message}", Constants.STAFF_COLOR);
        }

        if (!discord
            || Global.DiscordClient == null
            || Global.DiscordClient.GetChannel(Global.StaffDiscordChannel) is not SocketTextChannel channel)
            return;

        var embedBuilder = new EmbedBuilder
        {
            Title = Constants.SERVER_NAME,
            Description = message,
            Color = new Discord.Color(Global.MainRgba.Red, Global.MainRgba.Green, Global.MainRgba.Blue),
        };
        embedBuilder.WithFooter($"Enviada em {DateTime.Now}.");

        await channel.SendMessageAsync(embed: embedBuilder.Build());
    }

    public static UserPremium CheckVIPVehicle(string model)
    {
        var vip = UserPremium.None;
        model = model.ToLower();
        if (model == VehicleModel.Speeder.ToString().ToLower()
            || model == VehicleModel.TriBike3.ToString().ToLower()
            || model == VehicleModel.Supervolito2.ToString().ToLower()
            || model == VehicleModel.Bf400.ToString().ToLower()
            || model == VehicleModel.Dominator3.ToString().ToLower()
            || model == VehicleModel.Dubsta3.ToString().ToLower()
            || model == VehicleModel.Luxor2.ToString().ToLower()
            || model == VehicleModel.Contender.ToString().ToLower()
            || model == VehicleModel.Patriot2.ToString().ToLower()
            || model == VehicleModel.Deveste.ToString().ToLower()
            || model == VehicleModel.Elegy.ToString().ToLower()
            || model == VehicleModel.Neon.ToString().ToLower()
            || model == VehicleModel.Issi7.ToString().ToLower()
            || model == VehicleModel.Pfister811.ToString().ToLower()
            || model == VehicleModel.Banshee2.ToString().ToLower()
            || model == VehicleModel.Shinobi.ToString().ToLower()
            || model == VehicleModel.Reever.ToString().ToLower()
            || model == VehicleModel.Comet7.ToString().ToLower()
            || model == VehicleModel.Deity.ToString().ToLower()
            || model == VehicleModel.Granger2.ToString().ToLower()
            || model == VehicleModel.Zeno.ToString().ToLower()
            || model == VehicleModel.Blazer2.ToString().ToLower()
        )
        {
            vip = UserPremium.Gold;
        }
        else if (model == VehicleModel.Tropic2.ToString().ToLower()
            || model == VehicleModel.Issi2.ToString().ToLower()
            || model == VehicleModel.Windsor2.ToString().ToLower()
            || model == VehicleModel.TriBike2.ToString().ToLower()
            || model == VehicleModel.Akuma.ToString().ToLower()
            || model == VehicleModel.CarbonRs.ToString().ToLower()
            || model == VehicleModel.Yosemite2.ToString().ToLower()
            || model == VehicleModel.Brawler.ToString().ToLower()
            || model == VehicleModel.Everon.ToString().ToLower()
            || model == VehicleModel.Nimbus.ToString().ToLower()
            || model == VehicleModel.Comet5.ToString().ToLower()
            || model == VehicleModel.Ninef2.ToString().ToLower()
            || model == VehicleModel.Entity2.ToString().ToLower()
            || model == VehicleModel.Prototipo.ToString().ToLower()
            || model == VehicleModel.Emerus.ToString().ToLower()
            || model == VehicleModel.Reever.ToString().ToLower()
            || model == VehicleModel.Iwagen.ToString().ToLower()
            || model == VehicleModel.Astron.ToString().ToLower()
            || model == VehicleModel.Jubilee.ToString().ToLower()
            || model == VehicleModel.Ignus.ToString().ToLower()
            || model == VehicleModel.Patriot3.ToString().ToLower()
        )
        {
            vip = UserPremium.Silver;
        }
        else if (model == VehicleModel.Seashark.ToString().ToLower()
            || model == VehicleModel.Seashark3.ToString().ToLower()
            || model == VehicleModel.TriBike.ToString().ToLower()
            || model == VehicleModel.Havok.ToString().ToLower()
            || model == VehicleModel.Double.ToString().ToLower()
            || model == VehicleModel.Hakuchou2.ToString().ToLower()
            || model == VehicleModel.Vindicator.ToString().ToLower()
            || model == VehicleModel.Baller2.ToString().ToLower()
            || model == VehicleModel.Locust.ToString().ToLower()
            || model == VehicleModel.Komoda.ToString().ToLower()
            || model == VehicleModel.Turismo2.ToString().ToLower()
            || model == VehicleModel.Krieger.ToString().ToLower()
            || model == VehicleModel.Nero2.ToString().ToLower()
            || model == VehicleModel.Tyrant.ToString().ToLower()
            || model == VehicleModel.Cinquemila.ToString().ToLower()
            || model == VehicleModel.Buffalo4.ToString().ToLower()
            || model == VehicleModel.Baller7.ToString().ToLower()
            || model == VehicleModel.Youga4.ToString().ToLower()
            || model == VehicleModel.Mule5.ToString().ToLower()
            || model == VehicleModel.Sanchez2.ToString().ToLower()
            || model == VehicleModel.Blazer3.ToString().ToLower()
        )
        {
            vip = UserPremium.Bronze;
        }

        return vip;
    }

    public static string CheckFinalDot(string message)
    {
        message = message.Trim();
        var caracter = message.LastOrDefault();
        if (caracter != '.' && caracter != '!' && caracter != '?')
            message += ".";

        return message;
    }

    public static async Task WriteLog(LogType type, string description)
    {
        var context = GetDatabaseContext();
        var log = new Log();
        log.Create(type, description);
        await context.Logs.AddAsync(log);
        await context.SaveChangesAsync();
    }

    public static async Task SendDiscordMessage(ulong discordId, string text)
    {
        try
        {
            if (Global.DiscordClient is null)
                return;

            var user = Global.DiscordClient.GetUser(discordId);
            if (user is null)
                return;

            await user.SendMessageAsync(text);
        }
        catch (Exception ex)
        {
            GetException(ex);
        }
    }

    public static bool CanDropItem(Faction? faction, CharacterItem characterItem)
    {
        if (faction?.HasDuty ?? false)
        {
            if (characterItem.OnlyOnDuty || characterItem.GetCategory() == ItemCategory.WalkieTalkie)
                return false;
        }

        return true;
    }

    public static string GetItemsTemplatesResponse()
    {
        static CharacterItem GetItem(ItemTemplate itemTemplate)
        {
            var characterItem = new CharacterItem();
            characterItem.Create(itemTemplate.Id, 0, 1, null);
            return characterItem;
        }

        return Serialize(
            Global.ItemsTemplates
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                IsStack = GetItem(x).GetIsStack(),
            })
        );
    }

    public static bool CheckIfIsAmmo(ItemCategory itemCategory)
    {
        return itemCategory == ItemCategory.PistolAmmo || itemCategory == ItemCategory.ShotgunAmmo
            || itemCategory == ItemCategory.AssaultRifleAmmo || itemCategory == ItemCategory.LightMachineGunAmmo
            || itemCategory == ItemCategory.SniperRifleAmmo || itemCategory == ItemCategory.SubMachineGunAmmo;
    }

    public static bool CheckIfIsBulletShell(ItemCategory itemCategory)
    {
        return itemCategory == ItemCategory.PistolBulletShell
            || itemCategory == ItemCategory.ShotgunBulletShell || itemCategory == ItemCategory.AssaultRifleBulletShell
            || itemCategory == ItemCategory.LightMachineGunBulletShell || itemCategory == ItemCategory.SniperRifleBulletShell
            || itemCategory == ItemCategory.SubMachineGunBulletShell;
    }

    public static (int, float) GetTuningPrice(int vehiclePrice, Company? company, CompanyTuningPriceType type)
    {
        if (company is null)
            return (0, 0);

        var sellPercentagePrice = company.TuningPrices!.FirstOrDefault(x => x.Type == type)?.SellPercentagePrice ?? 0;
        return (Convert.ToInt32(vehiclePrice * (sellPercentagePrice / 100f)), sellPercentagePrice);
    }

    public static void CMDTuning(MyPlayer player, MyPlayer? target, Company? company, bool staff)
    {
        if (player.Vehicle is not MyVehicle vehicle || vehicle.Driver != player)
        {
            player.SendMessage(Models.MessageType.Error, Globalization.VEHICLE_DRIVER_ERROR_MESSAGE);
            return;
        }

        var vehiclePrice = GetVehiclePrice(vehicle.VehicleDB.Model);
        var realVehiclePrice = vehiclePrice?.Item1 ?? 0;

        if (staff)
        {
            if (!vehicle.VehicleDB.FactionId.HasValue && vehicle.SpawnType != MyVehicleSpawnType.Admin)
            {
                player.SendMessage(Models.MessageType.Error, "Veículo deve pertencer a uma facção ou ser administrativo.");
                return;
            }
        }
        else
        {
            if (vehiclePrice is null)
            {
                player.SendMessage(Models.MessageType.Error, "Preço do veículo não foi encontrado.");
                return;
            }

            if (company is null)
            {
                player.SendMessage(Models.MessageType.Error, "Empresa não encontrada.");
                return;
            }

            foreach (var companyTuningPriceType in Enum.GetValues<CompanyTuningPriceType>())
            {
                var tuningPrice = GetTuningPrice(realVehiclePrice, company, companyTuningPriceType);

                var costPercentagePrice = company.TuningPrices!.FirstOrDefault(x => x.Type == companyTuningPriceType)?.CostPercentagePrice ?? 0;
                var costTuningPrice = Convert.ToInt32(realVehiclePrice * (costPercentagePrice / 100f));
                if (tuningPrice.Item1 < costTuningPrice)
                {
                    player.SendMessage(Models.MessageType.Error, $"[{company.Name}] {companyTuningPriceType.GetDisplay()} possui Preço de Venda menor que o Preço de Custo. Por favor, avise ao proprietário.");
                    return;
                }

                if ((tuningPrice.Item2 - costPercentagePrice) > 5)
                {
                    player.SendMessage(Models.MessageType.Error, $"[{company.Name}] {companyTuningPriceType.GetDisplay()} possui Preço de Venda acima do limite. Por exemplo, se o Preço de Custo for 5%, o máximo que o Preço de Venda pode ser é 10%. Por favor, avise ao proprietário.");
                    return;
                }
            }
        }

        var realMods = Deserialize<List<VehicleMod>>(vehicle.VehicleDB.ModsJSON);

        var vehicleTuning = new VehicleTuning();
        if (player.VehicleTuning?.VehicleId == vehicle.VehicleDB.Id)
        {
            vehicleTuning = player.VehicleTuning;
        }
        else
        {
            vehicleTuning = new VehicleTuning
            {
                CompanyId = company?.Id,
                TargetId = target?.Character?.Id,
                Staff = staff,
                CurrentMods = GetTuningTypes()
                    .Select(x => new VehicleTuning.Mod
                    {
                        Type = Convert.ToByte(x),
                        Name = x.GetDisplay(),
                        UnitaryValue = GetTuningPrice(realVehiclePrice, company, x).Item1,
                        Current = realMods.FirstOrDefault(y => y.Type == (byte)x)?.Id ?? -1,
                        Selected = realMods.FirstOrDefault(y => y.Type == (byte)x)?.Id ?? -1,
                        MultiplyValue = CheckCompanyTuningPriceTypeMultiplyValue(x),
                    })
                    .ToList(),
                RepairValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Repair).Item1,
                WheelType = vehicle.VehicleDB.WheelType,
                WheelVariation = vehicle.VehicleDB.WheelVariation,
                WheelColor = vehicle.VehicleDB.WheelColor,
                WheelValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Wheel).Item1,
                Color1 = $"#{vehicle.VehicleDB.Color1R:X2}{vehicle.VehicleDB.Color1G:X2}{vehicle.VehicleDB.Color1B:X2}",
                Color2 = $"#{vehicle.VehicleDB.Color2R:X2}{vehicle.VehicleDB.Color2G:X2}{vehicle.VehicleDB.Color2B:X2}",
                ColorValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Color).Item1,
                NeonColor = $"#{vehicle.VehicleDB.NeonColorR:X2}{vehicle.VehicleDB.NeonColorG:X2}{vehicle.VehicleDB.NeonColorB:X2}",
                NeonLeft = Convert.ToByte(vehicle.VehicleDB.NeonLeft),
                NeonRight = Convert.ToByte(vehicle.VehicleDB.NeonRight),
                NeonFront = Convert.ToByte(vehicle.VehicleDB.NeonFront),
                NeonBack = Convert.ToByte(vehicle.VehicleDB.NeonBack),
                NeonValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Neon).Item1,
                HeadlightColor = vehicle.VehicleDB.HeadlightColor,
                LightsMultiplier = vehicle.VehicleDB.LightsMultiplier,
                XenonColorValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.XenonColor).Item1,
                WindowTint = vehicle.VehicleDB.WindowTint,
                WindowTintValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Insufilm).Item1,
                TireSmokeColor = $"#{vehicle.VehicleDB.TireSmokeColorR:X2}{vehicle.VehicleDB.TireSmokeColorG:X2}{vehicle.VehicleDB.TireSmokeColorB:X2}",
                TireSmokeColorValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.TireSmoke).Item1,
                ProtectionLevel = vehicle.VehicleDB.ProtectionLevel,
                ProtectionLevelValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.ProtectionLevel).Item1,
                XMR = Convert.ToByte(vehicle.VehicleDB.XMR),
                XMRValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.XMR).Item1,
                Livery = vehicle.VehicleDB.Livery,
                LiveryValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Livery).Item1,
                Extras = Deserialize<bool[]>(vehicle.VehicleDB.ExtrasJSON),
                ExtrasValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Extra).Item1,
                Drift = Convert.ToByte(vehicle.VehicleDB.Drift),
                DriftValue = GetTuningPrice(realVehiclePrice, company, CompanyTuningPriceType.Drift).Item1,
            };

            vehicleTuning.CurrentWheelType = vehicleTuning.WheelType;
            vehicleTuning.CurrentWheelVariation = vehicleTuning.WheelVariation;
            vehicleTuning.CurrentColor1 = vehicleTuning.Color1;
            vehicleTuning.CurrentColor2 = vehicleTuning.Color2;
            vehicleTuning.CurrentNeonColor = vehicleTuning.NeonColor;
            vehicleTuning.CurrentNeonLeft = vehicleTuning.NeonLeft;
            vehicleTuning.CurrentNeonRight = vehicleTuning.NeonRight;
            vehicleTuning.CurrentNeonFront = vehicleTuning.NeonFront;
            vehicleTuning.CurrentNeonBack = vehicleTuning.NeonBack;
            vehicleTuning.CurrentHeadlightColor = vehicleTuning.HeadlightColor;
            vehicleTuning.CurrentLightsMultiplier = vehicleTuning.LightsMultiplier;
            vehicleTuning.CurrentWindowTint = vehicleTuning.WindowTint;
            vehicleTuning.CurrentTireSmokeColor = vehicleTuning.TireSmokeColor;
            vehicleTuning.CurrentProtectionLevel = vehicleTuning.ProtectionLevel;
            vehicleTuning.CurrentXMR = vehicleTuning.XMR;
            vehicleTuning.CurrentLivery = vehicleTuning.Livery;
            vehicleTuning.CurrentExtras = vehicleTuning.Extras;
            vehicleTuning.CurrentDrift = vehicleTuning.Drift;
        }

        player.Emit("VehicleTuning", Serialize(vehicleTuning));
    }

    public static async Task<uint> GetNewCellphoneNumber()
    {
        uint cellphone;

        do
        {
            cellphone = Convert.ToUInt32(new Random().Next(1111111, 9999999));
        } while (await CheckIfCellphoneExists(cellphone));

        var phoneNumber = new PhoneNumber();
        phoneNumber.Create(cellphone);

        var context = GetDatabaseContext();
        await context.PhonesNumbers.AddAsync(phoneNumber);
        await context.SaveChangesAsync();

        return cellphone;
    }

    public static async Task<bool> CheckIfCellphoneExists(uint cellphone)
    {
        var context = GetDatabaseContext();
        return cellphone == Constants.MECHANIC_NUMBER
            || cellphone == Constants.TAXI_NUMBER
            || cellphone == Constants.EMERGENCY_NUMBER
            || cellphone == Constants.INSURANCE_NUMBER
            || await context.PhonesNumbers.AnyAsync(x => x.Number == cellphone);
    }

    public static string Serialize(object data) => JsonSerializer.Serialize(data, Global.JsonSerializerOptions);

    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Global.JsonSerializerOptions)!;

    public static MyMarker CreateMarker(int type, Vector3 position, float scale, GTANetworkAPI.Color color, uint dimension)
    {
        var marker = (MyMarker)NAPI.Marker.CreateMarker(type, position, new(), new(), scale, color, false, dimension);
        return marker;
    }

    public static MyObject CreateObject(string model, Vector3 position, Vector3 rotation, uint dimension, bool frozen, bool collision,
        Guid? propertyFurnitureId = null, bool door = false, bool doorLocked = false)
    {
        var myObject = (MyObject)NAPI.Object.CreateObject(Hash(model), position, rotation, 255, dimension);
        myObject.SetSharedDataEx("Frozen", frozen);
        myObject.SetSharedDataEx("Collision", collision);
        myObject.SetSharedDataEx("PropertyFurnitureId", propertyFurnitureId?.ToString());
        myObject.SetSharedDataEx("Door", door);
        myObject.SetSharedDataEx("DoorLocked", doorLocked);

        if (model == Constants.METAL_DETECTOR_OBJECT_MODEL)
        {
            var colShape = CreateColShapeCylinder(position, 1, 1.5f, dimension);
            colShape.Object = myObject;
        }
        else if (model == Constants.SPIKE_STRIP_OBJECT_MODEL)
        {
            var colShape = CreateColShapeCircle(position, 2f, dimension);
            colShape.Object = myObject;
        }

        return myObject;
    }

    public static MyColShape CreateColShapeCylinder(Vector3 position, float range, float height, uint dimension)
    {
        return (MyColShape)NAPI.ColShape.CreateCylinderColShape(position, range, height, dimension);
    }

    public static MyColShape CreateColShapeCircle(Vector3 position, float range, uint dimension)
    {
        return (MyColShape)NAPI.ColShape.CreatCircleColShape(position.X, position.Y, range, dimension);
    }

    public static MyPed CreatePed(uint model, Vector3 position, Vector3 rotation, uint dimension, Body? body = null)
    {
        var myPed = new MyPed().SetupAllClients(model, rotation.Z, position, dimension, body?.PersonalizationJSON, body?.OutfitJSON);
        return myPed;
    }

    public static (int, Guid)? GetVehiclePrice(string model)
    {
        var dealershipVehicle = Global.DealershipsVehicles.FirstOrDefault(x => x.Model.ToLower() == model.ToLower());
        if (dealershipVehicle is null)
            return null;

        return new(dealershipVehicle.Value, dealershipVehicle.DealershipId);
    }

    public static (int, Guid)? GetVehicleSellPrice(string model)
    {
        var dealershipVehicle = Global.DealershipsVehicles.FirstOrDefault(x => x.Model.ToLower() == model.ToLower());
        if (dealershipVehicle is null)
            return null;

        return new(Convert.ToInt32(dealershipVehicle.Value / 2), dealershipVehicle.DealershipId);
    }

    public static string GetTimespan(DateTime dateTime)
    {
        var ts = DateTime.Now - dateTime;
        return $"{ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    public static bool CheckIfVehicleExists(string model)
    {
        return Enum.TryParse(model, true, out VehicleModel _)
            || Enum.TryParse(model, true, out VehicleModelMods _);
    }

    public static bool IsValidImageUrl(string url)
    {
        return url.StartsWith("https://i.imgur.com/");
    }

    public static string GetWeaponName(uint type)
    {
        return ((WeaponModel)type).ToString();
    }

    public static uint GetWeaponType(string name)
    {
        Enum.TryParse(name, true, out WeaponModel type);
        return (uint)type;
    }

    public static Guid? GetAmmoItemTemplateIdByWeapon(uint weapon)
    {
        return Global.WeaponsInfos.FirstOrDefault(x => x.Name == GetWeaponName(weapon))?.AmmoItemTemplateId;
    }

    public static IEnumerable<Guid> GetComponentsItemsTemplatesByWeapon(uint weapon)
    {
        return Global.WeaponsInfos.FirstOrDefault(x => x.Name == GetWeaponName(weapon))?.Components.Select(x => x.ItemTemplateId) ?? [];
    }

    public static async Task<Guid> GetNewWeaponId()
    {
        var context = GetDatabaseContext();
        var weaponId = new WeaponId();
        do
        {
            weaponId = new WeaponId();
        } while (await context.WeaponsIds.AnyAsync(x => x.Id == weaponId.Id));

        await context.WeaponsIds.AddAsync(weaponId);
        await context.SaveChangesAsync();

        return weaponId.Id;
    }

    public static WeaponBody CheckDefaultWeaponBody(WeaponBody? weaponBody)
    {
        return weaponBody ?? new()
        {
            PosX = -0.01f,
            PosY = 0.18f,
            PosZ = 0.28f,
            RotR = 0,
            RotP = 40.10f,
            RotY = 12.03f,
        };
    }

    public static string GetBulletShellTemperature(string extra)
    {
        var bulletShell = Deserialize<BulletShellItem>(extra);
        var totalMinutes = (DateTime.Now - bulletShell.Date).TotalMinutes;
        if (totalMinutes >= 20)
            return "Fria";

        if (totalMinutes >= 10)
            return "Morna";

        return Globalization.HOT;
    }

    public static bool IsOwnedByState(CompanyType type) => type == CompanyType.ConvenienceStore ||
        type == CompanyType.Fishmonger ||
        type == CompanyType.WeaponStore;

    public static string GetMovementByWalkStyle(CharacterWalkStyle walkStyle)
    {
        return walkStyle switch
        {
            CharacterWalkStyle.Arrogant => "move_f@arrogant@a",
            CharacterWalkStyle.Casual => "move_m@casual@a",
            CharacterWalkStyle.Casual2 => "move_m@casual@b",
            CharacterWalkStyle.Casual3 => "move_m@casual@c",
            CharacterWalkStyle.Casual4 => "move_m@casual@d",
            CharacterWalkStyle.Casual5 => "move_m@casual@e",
            CharacterWalkStyle.Casual6 => "move_m@casual@f",
            CharacterWalkStyle.Confident => "move_m@confident",
            CharacterWalkStyle.Business => "move_m@business@a",
            CharacterWalkStyle.Business2 => "move_m@business@b",
            CharacterWalkStyle.Business3 => "move_m@business@c",
            CharacterWalkStyle.Femme => "move_f@femme@",
            CharacterWalkStyle.Flee => "move_f@flee@a",
            CharacterWalkStyle.Gangster => "move_m@gangster@generic",
            CharacterWalkStyle.Gangster2 => "move_m@gangster@ng",
            CharacterWalkStyle.Gangster3 => "move_m@gangster@var_e",
            CharacterWalkStyle.Gangster4 => "move_m@gangster@var_f",
            CharacterWalkStyle.Gangster5 => "move_m@gangster@var_i",
            CharacterWalkStyle.Heels => "move_f@heels@c",
            CharacterWalkStyle.Heels2 => "move_f@heels@d",
            CharacterWalkStyle.Hiking => "move_m@hiking",
            CharacterWalkStyle.Muscle => "move_m@muscle@a",
            CharacterWalkStyle.Quick => "move_m@quick",
            CharacterWalkStyle.Wide => "move_m@bag",
            CharacterWalkStyle.Scared => "move_f@scared",
            CharacterWalkStyle.Brave => "move_m@brave",
            CharacterWalkStyle.Tipsy => "move_m@drunk@slightlydrunk",
            CharacterWalkStyle.Injured => "move_m@injured",
            CharacterWalkStyle.Tough => "move_m@tough_guy@",
            CharacterWalkStyle.Sassy => "move_m@sassy",
            CharacterWalkStyle.Sad => "move_m@sad@a",
            CharacterWalkStyle.Posh => "move_m@posh@",
            CharacterWalkStyle.Alien => "move_m@alien",
            CharacterWalkStyle.Nonchalant => "move_m@non_chalant",
            CharacterWalkStyle.Hobo => "move_m@hobo@a",
            CharacterWalkStyle.Money => "move_m@money",
            CharacterWalkStyle.Swagger => "move_m@swagger",
            CharacterWalkStyle.Shady => "move_m@shadyped@a",
            CharacterWalkStyle.Maneater => "move_f@maneater",
            CharacterWalkStyle.Chichi => "move_f@chichi",
            _ => string.Empty,
        };
    }

    public static MyBlip CreateBlip(Vector3 position, uint sprite, byte color, string name, float scale, bool shortRange)
    {
        var myBlip = (MyBlip)NAPI.Blip.CreateBlip(sprite, position, scale, color, name, 255, 0, shortRange);
        return myBlip;
    }

    public static IQueryable<Character> GetActiveCharacters()
    {
        var context = GetDatabaseContext();
        return context.Characters.Where(x => !x.DeletedDate.HasValue
            && !x.DeathDate.HasValue
            && x.NameChangeStatus != CharacterNameChangeStatus.Done);
    }

    public static bool CheckCompanyTuningPriceTypeMultiplyValue(CompanyTuningPriceType type)
    {
        return type == CompanyTuningPriceType.Engine || type == CompanyTuningPriceType.Brakes
            || type == CompanyTuningPriceType.Transmission || type == CompanyTuningPriceType.Suspension
            || type == CompanyTuningPriceType.Armor;
    }

    public static DatabaseContext GetDatabaseContext() => new();

    public static void SetWeatherInfo()
    {
        foreach (var target in Global.SpawnedPlayers)
        {
            target.SetOwnSharedDataEx("Temperature", $"{Global.WeatherInfo.Main.Temp:N0}ºC");
            target.SetOwnSharedDataEx("WeatherType", Global.WeatherInfo.WeatherType.ToString().ToUpper());
            if (target.GetDimension() == 0)
                target.SyncWeather(Global.WeatherInfo.WeatherType);
        }
    }

    public static void PlaySMSSound(MyPlayer player)
    {
        if (player.CellphoneItem.RingtoneVolume == 0 || player.SPECId.HasValue)
            return;

        var smsAudioSpot = new AudioSpot
        {
            Source = Constants.URL_AUDIO_CELLPHONE_SMS,
            Dimension = player.GetDimension(),
            Loop = false,
            Position = player.GetPosition(),
            Range = 5,
            PlayerId = player.Id,
            Volume = player.CellphoneItem.RingtoneVolume / 100,
        };

        smsAudioSpot.SetupAllClients();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                smsAudioSpot.RemoveAllClients();
            }
            catch (Exception ex)
            {
                GetException(ex);
            }
        });
    }

    public static MyVehicle CreateVehicle(string model, Vector3 position, Vector3 rotation, MyVehicleSpawnType spawnType)
    {
        var vehicle = RunOnMainThread(() => (MyVehicle)NAPI.Vehicle.CreateVehicle(Hash(model), position, rotation.Z, 0, 0, string.Empty, 255, false, false));
        vehicle.SpawnType = spawnType;
        vehicle.SetSharedDataEx(Constants.VEHICLE_META_DATA_MODEL, model.ToUpper());
        vehicle.Frozen = false;
        return vehicle;
    }

    public static bool CheckIfPedModelExists(string model)
    {
        var hash = Hash(model);
        return Enum.IsDefined(typeof(PedHash), hash);
    }

    public static uint Hash(string text) => NAPI.Util.GetHashKey(text);

    public static async Task ShowBank(MyPlayer player, bool atm, bool success, bool update)
    {
        if (atm && !success)
        {
            player.SendMessage(Models.MessageType.Error, "Você não está em uma ATM.");
            return;
        }

        if (player.IsActionsBlocked())
        {
            player.SendMessage(Models.MessageType.Error, Globalization.ACTIONS_BLOCKED_MESSAGE);
            return;
        }

        var context = GetDatabaseContext();
        var fines = await context.Fines.Where(x => !x.PaymentDate.HasValue && x.CharacterId == player.Character.Id)
            .OrderBy(x => x.RegisterDate)
            .Select(x => new
            {
                x.Id,
                x.Reason,
                Date = x.RegisterDate,
                x.Value,
            })
            .ToListAsync();

        var transactions = await context.FinancialTransactions.Where(x => x.CharacterId == player.Character.Id)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Description,
                x.Type,
                x.Value,
                Date = x.RegisterDate,
            })
            .ToListAsync();

        player.Emit("BankShow",
            update,
            atm,
            player.Character.BankAccount, player.Character.Name,
            player.Character.Bank,
            Serialize(fines),
            Serialize(transactions));
    }

    public static MyPlayer CastPlayer(Player? entity)
    {
        if (entity is null)
            throw new ArgumentException("Player is null");

        return (MyPlayer)entity;
    }

    public static MyColShape CastColshape(ColShape? entity)
    {
        if (entity is null)
            throw new ArgumentException("ColShape is null");

        return (MyColShape)entity;
    }

    public static MyVehicle CastVehicle(GTANetworkAPI.Vehicle? entity)
    {
        if (entity is null)
            throw new ArgumentException("Vehicle is null");

        return (MyVehicle)entity;
    }

    public static void RunOnMainThread(Action action)
    {
        if (Environment.CurrentManagedThreadId == NAPI.MainThreadId)
            action();
        else
            NAPI.Task.Run(() =>
            {
                action();
            });
    }

    private static Task<T> RunReturn<T>(this GTANetworkMethods.Task task, Func<T> func)
    {
        var taskCompletionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        task.Run(() =>
        {
            var result = func();
            taskCompletionSource.SetResult(result);
        });
        return taskCompletionSource.Task;
    }

    public static T RunOnMainThread<T>(Func<T> action)
    {
        if (Environment.CurrentManagedThreadId == NAPI.MainThreadId)
            return action();
        else
            return NAPI.Task.RunReturn(() =>
            {
                return action();
            }).Result;
    }

    public static void ConsoleLog(string message)
    {
        RunOnMainThread(() =>
        {
            NAPI.Util.ConsoleOutput($"[{DateTime.Now:HH:mm:ss}] {message}");
        });
    }

    public static IEnumerable<CompanyTuningPriceType> GetTuningTypes() =>
        Enum.GetValues<CompanyTuningPriceType>()
            .Where(x => x <= CompanyTuningPriceType.TrimColor);

    public static async Task SendSMS(MyPlayer? playerOrigin, uint[] targetsNumbers, PhoneMessage phoneMessage)
    {
        if (targetsNumbers.Length == 0)
            return;

        var context = GetDatabaseContext();
        if (playerOrigin is not null && phoneMessage.PhoneGroupId is null)
        {
            var targetNumber = targetsNumbers[0];
            if (await context.PhonesContacts.AnyAsync(x => x.Origin == targetNumber && x.Number == phoneMessage.Origin && x.Blocked))
            {
                playerOrigin.SendMessage(Models.MessageType.Error, $"Você foi bloqueado por {playerOrigin.GetCellphoneContactName(targetNumber)}.");
                return;
            }

            var targetCharacter = await context.Characters
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Cellphone == targetNumber && !x.DeletedDate.HasValue);
            if (targetCharacter is null)
            {
                playerOrigin.SendMessage(Models.MessageType.Error, $"{playerOrigin.GetCellphoneContactName(targetNumber)} não existe.");
                return;
            }
        }

        await context.PhonesMessages.AddAsync(phoneMessage);
        await context.SaveChangesAsync();

        if (playerOrigin is not null)
        {
            if (phoneMessage.Type == PhoneMessageType.Text)
            {
                if (phoneMessage.Number.HasValue)
                    playerOrigin.SendMessage(Models.MessageType.None, $"[CELULAR] SMS para {playerOrigin.GetCellphoneContactName(phoneMessage.Number.Value)}: {phoneMessage.Message}", Constants.CELLPHONE_SECONDARY_COLOR);
                else if (phoneMessage.PhoneGroupId.HasValue)
                    playerOrigin.SendMessage(Models.MessageType.None, $"[CELULAR] SMS para {Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneMessage.PhoneGroupId)?.Name}: {phoneMessage.Message}", Constants.CELLPHONE_SECONDARY_COLOR);
            }
            else if (phoneMessage.Type == PhoneMessageType.Location)
            {
                if (phoneMessage.Number.HasValue)
                    playerOrigin.SendMessage(Models.MessageType.None, $"[CELULAR] Você enviou sua localização para {playerOrigin.GetCellphoneContactName(phoneMessage.Number.Value)}.", Constants.CELLPHONE_SECONDARY_COLOR);
                else if (phoneMessage.PhoneGroupId.HasValue)
                    playerOrigin.SendMessage(Models.MessageType.None, $"[CELULAR] Você enviou sua localizaçaõ para {Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneMessage.PhoneGroupId)?.Name}.", Constants.CELLPHONE_SECONDARY_COLOR);
            }

            playerOrigin.SendMessageToNearbyPlayers("envia uma mensagem de texto.", MessageCategory.Ame);
            await playerOrigin.UpdatePhoneLastMessages();
        }

        foreach (var targetNumber in targetsNumbers)
        {
            var targetPlayer = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Cellphone == targetNumber);
            if (targetPlayer is null)
            {
                var targetCharacter = await context.Characters
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.Cellphone == targetNumber && !x.DeletedDate.HasValue);
                if (targetCharacter is null)
                    continue;

                try
                {
                    if (targetCharacter.User!.ReceiveSMSDiscord == UserReceiveSMSDiscord.None)
                        continue;

                    if (targetCharacter.User.ReceiveSMSDiscord == UserReceiveSMSDiscord.OnlyIndividuals
                        && phoneMessage.PhoneGroupId is not null)
                        continue;

                    var title = string.Empty;
                    if (phoneMessage.Number.HasValue)
                        title = $"SMS de {(await context.PhonesContacts.FirstOrDefaultAsync(x => x.Origin == targetNumber && x.Number == phoneMessage.Origin))?.Name ?? phoneMessage.Origin.ToString()}";
                    else if (phoneMessage.PhoneGroupId.HasValue)
                        title = $"SMS em {Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneMessage.PhoneGroupId)?.Name} de {(await context.PhonesContacts.FirstOrDefaultAsync(x => x.Origin == targetNumber && x.Number == phoneMessage.Origin))?.Name ?? phoneMessage.Origin.ToString()}";

                    if (string.IsNullOrWhiteSpace(title)
                        || Global.DiscordClient is null
                        || Global.DiscordClient.GetUser(Convert.ToUInt64(targetCharacter.User.DiscordId)) is not SocketUser discordUser)
                        continue;

                    var embedBuilder = new EmbedBuilder
                    {
                        Title = title,
                        Color = new Discord.Color(Global.MainRgba.Red, Global.MainRgba.Green, Global.MainRgba.Blue),
                        Description = phoneMessage.Type == PhoneMessageType.Location ? "Localização" : phoneMessage.Message,
                    };

                    embedBuilder.WithFooter($"Recebido em {DateTime.Now}.");

                    await discordUser.SendMessageAsync(embed: embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    GetException(ex);
                }
            }
            else
            {
                if (targetPlayer.CellphoneItem.FlightMode)
                    continue;

                if (phoneMessage.Type == PhoneMessageType.Text)
                {
                    if (phoneMessage.Number.HasValue)
                        targetPlayer.SendMessage(Models.MessageType.None, $"[CELULAR] SMS de {targetPlayer.GetCellphoneContactName(phoneMessage.Origin)}: {phoneMessage.Message}", Constants.CELLPHONE_MAIN_COLOR);
                    else if (phoneMessage.PhoneGroupId.HasValue)
                        targetPlayer.SendMessage(Models.MessageType.None, $"[CELULAR] SMS em {Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneMessage.PhoneGroupId)?.Name} de {targetPlayer.GetCellphoneContactName(phoneMessage.Origin)}: {phoneMessage.Message}", Constants.CELLPHONE_MAIN_COLOR);
                }
                else if (phoneMessage.Type == PhoneMessageType.Location)
                {
                    if (phoneMessage.Number.HasValue)
                        targetPlayer.SendMessage(Models.MessageType.None, $"[CELULAR] Você recebeu a localização de {targetPlayer.GetCellphoneContactName(phoneMessage.Origin)}.", Constants.CELLPHONE_SECONDARY_COLOR);
                    else if (phoneMessage.PhoneGroupId.HasValue)
                        targetPlayer.SendMessage(Models.MessageType.None, $"[CELULAR] Você recebeu uma localização de {targetPlayer.GetCellphoneContactName(phoneMessage.Origin)} em {Global.PhonesGroups.FirstOrDefault(x => x.Id == phoneMessage.PhoneGroupId)?.Name}.", Constants.CELLPHONE_SECONDARY_COLOR);
                }

                targetPlayer.SendMessageToNearbyPlayers("recebe uma mensagem de texto.", MessageCategory.Ame);
                await targetPlayer.UpdatePhoneLastMessages();
                PlaySMSSound(targetPlayer);
            }
        }
    }

    public static List<string> ChunkString(string text, int chunkSize)
    {
        var chunks = new List<string>();
        for (var i = 0; i < text.Length; i += chunkSize)
            chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        return chunks;
    }

    public static bool IsWeaponWithAmmo(uint weapon)
    {
        return Global.WeaponsInfos.Any(x => x.Name == Functions.GetWeaponName(weapon) && x.AmmoItemTemplateId.HasValue);
    }
}