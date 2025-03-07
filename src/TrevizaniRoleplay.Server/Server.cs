using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Timers;
using TrevizaniRoleplay.Core.Models.Requests;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server;

public class Server : Script
{
    System.Timers.Timer MainTimer { get; set; } = new(TimeSpan.FromMinutes(30));

    System.Timers.Timer MinuteTimer { get; set; } = new(TimeSpan.FromMinutes(1));

    private bool ProcessingUCPActions { get; set; }
    System.Timers.Timer UCPActionsTimer { get; set; } = new(TimeSpan.FromSeconds(2));

    private async Task InsertJobs()
    {
        var context = Functions.GetDatabaseContext();
        foreach (var characterJob in Enum.GetValues<CharacterJob>().Where(x => x != CharacterJob.Unemployed))
        {
            var job = await context.Jobs.FirstOrDefaultAsync(x => x.CharacterJob == characterJob);
            if (job is null)
            {
                job = new();
                job.Create(characterJob);
                await context.Jobs.AddAsync(job);
                await context.SaveChangesAsync();
            }
        }
    }

    private async Task InsertItemsTemplates()
    {
        await InsertItemTemplate(new Guid(Constants.MONEY_ITEM_TEMPLATE_ID), ItemCategory.Money, Resources.Money,
            0.000001f, "https://i.imgur.com/z5afrcD.png", "bkr_prop_money_sorted_01");
        await InsertItemTemplate(new Guid(Constants.VEHICLE_PART_ITEM_TEMPLATE_ID), ItemCategory.VehiclePart, Resources.VehiclePart,
            0.1f, "https://i.imgur.com/w3D4MtH.png", "imp_prop_impexp_exhaust_06");
        await InsertItemTemplate(new Guid(Constants.VEHICLE_KEY_ITEM_TEMPLATE_ID), ItemCategory.VehicleKey, Resources.VehicleKey,
            0.1f, "https://i.imgur.com/bkYGL8a.png", "p_car_keys_01");
        await InsertItemTemplate(new Guid(Constants.PROPERTY_KEY_ITEM_TEMPLATE_ID), ItemCategory.PropertyKey, Resources.PropertyKey,
            0.1f, "https://i.imgur.com/IEDSv1j.png", "prop_cs_keys_01");
        await InsertItemTemplate(new Guid(Constants.BLOOD_SAMPLE_ITEM_TEMPLATE_ID), ItemCategory.BloodSample, Resources.BloodSample,
            0.0001f, "https://i.imgur.com/5pguCOS.png", "p_bloodsplat_s");
        await InsertItemTemplate(new Guid(Constants.PISTOL_AMMO_ITEM_TEMPLATE_ID), ItemCategory.PistolAmmo, Resources.PistolAmmo,
           0.0001f, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.SHOTGUN_AMMO_ITEM_TEMPLATE_ID), ItemCategory.ShotgunAmmo, Resources.ShotgunAmmo,
           0.0001f, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.ASSAULT_RIFLE_AMMO_ITEM_TEMPLATE_ID), ItemCategory.AssaultRifleAmmo, Resources.AssaultRifleAmmo,
           0.0001f, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.LIGHT_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID), ItemCategory.LightMachineGunAmmo, Resources.LightMachineGunAmmo,
           0.0001f, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.SNIPER_RIFLE_AMMO_ITEM_TEMPLATE_ID), ItemCategory.SniperRifleAmmo, Resources.SniperRifleAmmo,
           0.0001f, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.SUB_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID), ItemCategory.SubMachineGunAmmo, Resources.SubMachineGunAmmo,
           0.0001f, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.PISTOL_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.PistolBulletShell, Resources.PistolBulletShell,
           0.0001f, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshoth4_shell");
        await InsertItemTemplate(new Guid(Constants.SHOTGUN_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.ShotgunBulletShell, Resources.ShotgunBulletShell,
           0.0001f, "https://i.imgur.com/zUZ41G6.png", "prop_sgun_casing");
        await InsertItemTemplate(new Guid(Constants.ASSAULT_RIFLE_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.AssaultRifleBulletShell, Resources.AssaultRifleBulletShell,
           0.0001f, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshot_shell");
        await InsertItemTemplate(new Guid(Constants.LIGHT_MACHINE_GUN_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.LightMachineGunBulletShell, Resources.LightMachineGunBulletShell,
           0.0001f, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshot_shell");
        await InsertItemTemplate(new Guid(Constants.SNIPER_RIFLE_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.SniperRifleBulletShell, Resources.SniperRifleBulletShell,
           0.0001f, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshot_shell");
        await InsertItemTemplate(new Guid(Constants.SUB_MACHINE_GUN_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.SubMachineGunBulletShell, Resources.SubMachineGunBulletShell,
           0.0001f, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshoth4_shell");
    }

    private async Task InsertItemTemplate(Guid id, ItemCategory itemCategory, string name, float weight, string image, string objectModel)
    {
        var context = Functions.GetDatabaseContext();
        var itemTemplate = await context.ItemsTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (itemTemplate is null)
        {
            itemTemplate = new();
            itemTemplate.SetId(id);
            itemTemplate.Create(itemCategory, 0, name, weight, image, objectModel);
            await context.ItemsTemplates.AddAsync(itemTemplate);
            await context.SaveChangesAsync();
        }
    }

    private static void InsertPremiumItem(string name, int value)
    {
        if (Global.PremiumItems.Any(x => x.Name == name))
            return;

        Global.PremiumItems.Add(new()
        {
            Name = name,
            Value = value,
        });
    }

    private JsonObject? settingsJson;
    string GetSetting(string key)
    {
        if (settingsJson is null)
        {
            var jsonObject = JsonNode.Parse(File.ReadAllText("C:\\RAGEMP\\server-files\\conf.json"));
            settingsJson = jsonObject?["settings"]?.AsObject();
        }

        return settingsJson is not null && settingsJson.ContainsKey(key) ? settingsJson[key]!.ToString() : string.Empty;
    }

    [ServerEvent(Event.ResourceStart)]
    public async Task ResourceStart()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture =
              CultureInfo.GetCultureInfo(GetSetting("language"));

        NAPI.Server.SetAutoRespawnAfterDeath(false);
        NAPI.Server.SetGlobalServerChat(false);

        Global.AnnouncementDiscordChannel = GetSetting("announcementDiscordChannel").ToULong();
        Global.GovernmentAnnouncementDiscordChannel = GetSetting("governmentAnnouncementDiscordChannel").ToULong();
        Global.StaffDiscordChannel = GetSetting("staffDiscordChannel").ToULong();
        Global.CompanyAnnouncementDiscordChannel = GetSetting("companyAnnouncementDiscordChannel").ToULong();
        Global.PremiumGoldDiscordRole = GetSetting("premiumGoldDiscordRole").ToULong();
        Global.PremiumSilverDiscordRole = GetSetting("premiumSilverDiscordRole").ToULong();
        Global.PremiumBronzeDiscordRole = GetSetting("premiumBronzeDiscordRole").ToULong();
        Global.MainDiscordGuild = GetSetting("mainDiscordGuild").ToULong();
        Global.RoleplayAnnouncementDiscordChannel = GetSetting("roleplayAnnouncementDiscordChannel").ToULong();
        Global.PoliceEmergencyCallDiscordChannel = GetSetting("policeEmergencyCallDiscordChannel").ToULong();
        Global.FirefighterEmergencyCallDiscordChannel = GetSetting("firefighterEmergencyCallDiscordChannel").ToULong();
        var discordBotToken = GetSetting("discordBotToken");
        Global.DiscordClientId = GetSetting("discordClientId");
        Global.DiscordClientSecret = GetSetting("discordClientSecret");

        if (string.IsNullOrWhiteSpace(Global.DiscordClientId))
            throw new Exception(Resources.DiscordClientIdIsNotConfigured);

        if (string.IsNullOrWhiteSpace(Global.DiscordClientSecret))
            throw new Exception(Resources.DiscordClientSecretIsNotConfigured);

        var context = Functions.GetDatabaseContext();
        await context.Database.MigrateAsync();

        await context.HelpRequests.Where(x => !x.AnswerDate.HasValue).ExecuteUpdateAsync(x => x.SetProperty(y => y.AnswerDate, DateTime.Now));
        Functions.ConsoleLog("Cleaned Help Requests");

        await context.Sessions.Where(x => !x.FinalDate.HasValue).ExecuteUpdateAsync(x => x.SetProperty(y => y.FinalDate, y => y.RegisterDate));
        await context.Characters.Where(x => x.Connected).ExecuteUpdateAsync(x => x.SetProperty(y => y.Connected, false));
        Functions.ConsoleLog("Cleaned Invalid Sessions");

        await InsertJobs();
        Functions.ConsoleLog("Jobs Inserted");

        await InsertItemsTemplates();
        Functions.ConsoleLog("Items Templates Inserted");

        var parameter = await context.Parameters.FirstOrDefaultAsync();
        if (parameter is null)
        {
            parameter ??= new();
            await context.Parameters.AddAsync(parameter);
            await context.SaveChangesAsync();
        }

        if (parameter.BodyPartsDamagesJSON == "[]")
        {
            parameter.SetBodyPartsDamagesJSON(Functions.Serialize(Enum.GetValues<BodyPart>().Select(x => new BodyPartDamage
            {
                Name = Functions.GetBodyPartName(x),
                DamageMultiplier = 1,
            })));
            context.Parameters.Update(parameter);
            await context.SaveChangesAsync();
        }

        Global.PremiumItems = Functions.Deserialize<List<PremiumItem>>(parameter.PremiumItemsJSON);
        InsertPremiumItem(Resources.NameChange, 35);
        InsertPremiumItem(Resources.PlateChange, 25);
        InsertPremiumItem(Resources.NumberChange, 25);
        InsertPremiumItem(Resources.CharacterSlot, 150);
        InsertPremiumItem(Resources.Outfits10, 75);
        InsertPremiumItem(Resources.InternalFurnitures50, 75);
        InsertPremiumItem(Resources.InternalFurnitures500, 500);
        InsertPremiumItem(Resources.PremiumBronze, 150);
        InsertPremiumItem(Resources.PremiumSilver, 250);
        InsertPremiumItem(Resources.PremiumGold, 375);
        parameter.SetPremiumItemsJSON(Functions.Serialize(Global.PremiumItems));
        context.Parameters.Update(parameter);
        await context.SaveChangesAsync();

        Global.Parameter = parameter;
        LoadParameters();
        Functions.ConsoleLog("Loaded Parameters");

        Global.ItemsTemplates = await context.ItemsTemplates.ToListAsync();
        Functions.ConsoleLog($"ItemsTemplates: {Global.ItemsTemplates.Count}");

        Global.Blips = await context.Blips.ToListAsync();
        Global.Blips.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Blips: {Global.Blips.Count}");

        Global.Furnitures = await context.Furnitures.ToListAsync();
        Functions.ConsoleLog($"Furnitures: {Global.Furnitures.Count}");

        Global.Factions = await context.Factions.ToListAsync();
        Functions.ConsoleLog($"Factions: {Global.Factions.Count}");

        Global.FactionsRanks = await context.FactionsRanks.ToListAsync();
        Functions.ConsoleLog($"FactionsRanks: {Global.FactionsRanks.Count}");

        Global.FactionsFrequencies = await context.FactionsFrequencies.ToListAsync();
        Functions.ConsoleLog($"FactionsFrequencies: {Global.FactionsFrequencies.Count}");

        Global.FactionsUniforms = await context.FactionsUniforms.ToListAsync();
        Functions.ConsoleLog($"FactionsUniforms: {Global.FactionsUniforms.Count}");

        Global.Properties = await context.Properties
            .Include(x => x.Items)
            .Include(x => x.Furnitures)
            .Include(x => x.Entrances)
            .AsSplitQuery()
            .ToListAsync();
        foreach (var property in Global.Properties)
        {
            property.CreateIdentifier();

            foreach (var furniture in property.Furnitures!)
                furniture.CreateObject();
        }
        Functions.ConsoleLog($"Properties: {Global.Properties.Count}");

        Global.Jobs = await context.Jobs.ToListAsync();
        Global.Jobs.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Jobs: {Global.Jobs.Count}");

        Global.Spots = await context.Spots.ToListAsync();
        Global.Spots.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Spots: {Global.Spots.Count}");

        Global.FactionsStorages = await context.FactionsStorages.ToListAsync();
        Global.FactionsStorages.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"FactionsStorages: {Global.FactionsStorages.Count}");

        Global.FactionsStoragesItems = await context.FactionsStoragesItems.ToListAsync();
        Functions.ConsoleLog($"FactionsStoragesItems: {Global.FactionsStoragesItems.Count}");

        Global.FactionsUnits = await context.FactionsUnits
            .Where(x => !x.FinalDate.HasValue)
            .Include(x => x.Character!)
            .Include(x => x.Characters!)
                .ThenInclude(x => x.Character)
            .ToListAsync();
        Functions.ConsoleLog($"FactionsUnits: {Global.FactionsUnits.Count}");

        var emergencyCallsDate = DateTime.Now.AddHours(-24);
        Global.EmergencyCalls = await context.EmergencyCalls.Where(x => x.RegisterDate >= emergencyCallsDate).ToListAsync();
        Functions.ConsoleLog($"EmergencyCalls: {Global.EmergencyCalls.Count}");

        Global.Items = await context.Items.ToListAsync();
        Global.Items.ForEach(x => x.CreateObject());
        Functions.ConsoleLog($"Items: {Global.Items.Count}");

        Global.Doors = await context.Doors.ToListAsync();
        Functions.ConsoleLog($"Doors: {Global.Doors.Count}");

        Global.Infos = await context.Infos.ToListAsync();
        Global.Infos.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Infos: {Global.Infos.Count}");

        Global.CrackDens = await context.CrackDens.ToListAsync();
        Global.CrackDens.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"CrackDens: {Global.CrackDens.Count}");

        Global.CrackDensItems = await context.CrackDensItems.ToListAsync();
        Functions.ConsoleLog($"CrackDensItems: {Global.CrackDensItems.Count}");

        Global.TruckerLocations = await context.TruckerLocations.ToListAsync();
        Global.TruckerLocations.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"TruckerLocations: {Global.TruckerLocations.Count}");

        Global.TruckerLocationsDeliveries = await context.TruckerLocationsDeliveries.ToListAsync();
        Functions.ConsoleLog($"TruckerLocationsDeliveries: {Global.TruckerLocationsDeliveries.Count}");

        Global.Animations = await context.Animations.ToListAsync();
        Functions.ConsoleLog($"Animations: {Global.Animations.Count}");

        Global.Companies = await context.Companies
            .Include(x => x.Characters)
            .Include(x => x.Items)
            .Include(x => x.TuningPrices)
            .AsSplitQuery()
            .ToListAsync();
        Global.Companies.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Companies: {Global.Companies.Count}");

        Global.Commands = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToList();
        Functions.ConsoleLog($"Commands: {Global.Commands.Count}");

        Global.Dealerships = await context.Dealerships.ToListAsync();
        Global.Dealerships.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Dealerships: {Global.Dealerships.Count}");

        Global.DealershipsVehicles = await context.DealershipsVehicles.ToListAsync();
        Functions.ConsoleLog($"DealershipsVehicles: {Global.DealershipsVehicles.Count}");

        Global.Smugglers = await context.Smugglers.ToListAsync();
        Functions.ConsoleLog($"Smugglers: {Global.Smugglers.Count}");

        Global.Crimes = await context.Crimes.ToListAsync();
        Functions.ConsoleLog($"Crimes: {Global.Crimes.Count}");

        Global.Graffitis = await context.Graffitis.ToListAsync();
        Global.Graffitis.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Graffitis: {Global.Graffitis.Count}");

        Global.Bodies = await context.Bodies
            .Where(x => !x.MorgueDate.HasValue)
            .Include(x => x.Items)
            .AsSplitQuery()
            .ToListAsync();
        Global.Bodies.ForEach(x => x.CreateIdentifier());
        Functions.ConsoleLog($"Bodies: {Global.Bodies.Count}");

        Global.FactionsEquipments = await context.FactionsEquipments.Include(x => x.Items).ToListAsync();
        Functions.ConsoleLog($"FactionsEquipments: {Global.FactionsEquipments.Count}");

        Global.Drugs = await context.Drugs.ToListAsync();
        Functions.ConsoleLog($"Drugs: {Global.Drugs.Count}");

        Global.AdminObjects = await context.AdminObjects.ToListAsync();
        Global.AdminObjects.ForEach(x => x.CreateObject());
        Functions.ConsoleLog($"AdminObjects: {Global.AdminObjects.Count}");

        Global.PhonesGroups = await context.PhonesGroups.Include(x => x.Users).ToListAsync();
        Functions.ConsoleLog($"PhonesGroups: {Global.PhonesGroups.Count}");

        Global.Fires = await context.Fires.ToListAsync();
        Functions.ConsoleLog($"Fires: {Global.Fires.Count}");

        MainTimer.Elapsed += MainTimer_Elapsed;
        MainTimer.Start();
        MainTimer_Elapsed(null, null);

        MinuteTimer.Elapsed += MinuteTimer_Elapsed;
        MinuteTimer.Start();
        MinuteTimer_Elapsed(null, null);

        UCPActionsTimer.Elapsed += UCPActionsTimer_Elapsed;
        UCPActionsTimer.Start();

        if (!string.IsNullOrWhiteSpace(discordBotToken))
            DiscordBOT.Main.MainAsync(discordBotToken).GetAwaiter().GetResult();
    }

    [ServerEvent(Event.ResourceStop)]
    public void ResourceStop()
    {
        try
        {
            MainTimer?.Stop();
            MinuteTimer?.Stop();
            UCPActionsTimer?.Stop();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            Functions.ConsoleLog("MainTimer_Elapsed Start");
            await RemoveExpiredInfos();
            await RemoveExpiredGraffitis();
            await RemovePropertyInativeOwners();
            await DebitCompaniesWeekRent();
            await SyncWeather();
            Functions.ConsoleLog("MainTimer_Elapsed End");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async Task RemovePropertyInativeOwners()
    {
        if ((Global.Parameter.InactivePropertiesDate ?? DateTime.MinValue) > DateTime.Now)
            return;

        Functions.ConsoleLog("RemovePropertyInativeOwners Start");
        var context = Functions.GetDatabaseContext();
        var properties = (await context.Properties
            .Where(x => x.CharacterId.HasValue && x.Value > 0)
            .Include(x => x.Character)
                .ThenInclude(x => x!.User)
            .Include(x => x.Character)
                .ThenInclude(x => x!.Sessions)
            .ToListAsync())
            .Where(x => x.PurchaseDate.HasValue && x.PurchaseDate <= DateTime.Now.AddDays(-10));
        foreach (var property in properties)
        {
            var serverProperty = Global.Properties.FirstOrDefault(x => x.Id == property.Id);
            if (serverProperty is null)
                continue;

            var userPremium = property.Character!.User!.GetCurrentPremium();
            if (userPremium != UserPremium.Gold)
                userPremium = property.Character.GetCurrentPremium();

            var days = userPremium switch
            {
                UserPremium.Gold => 30,
                UserPremium.Silver => 20,
                _ => 10,
            };

            var update = false;

            var hoursPlayedLastDays = property.Character.Sessions!.Where(x => x.Type == SessionType.Login
                && x.FinalDate.HasValue && x.RegisterDate >= DateTime.Now.AddDays(-days))
                .Sum(x => (x.FinalDate!.Value - x.RegisterDate).TotalHours);
            if (hoursPlayedLastDays < 3)
            {
                await Functions.WriteLog(LogType.Staff, $"Remover dono inativo propriedade {serverProperty.FormatedAddress} ({serverProperty.Id}) {property.Character.Name} {hoursPlayedLastDays}");
                await serverProperty.ChangeOwner(null);
                update = true;
            }

            if (userPremium == UserPremium.None
                && (serverProperty.Time.HasValue || serverProperty.Weather.HasValue))
            {
                serverProperty.SetTime(null);
                serverProperty.SetWeather(null);
                update = true;
            }

            if (update)
                context.Properties.Update(serverProperty);
        }

        Global.Parameter.SetInactivePropertiesDate();
        context.Parameters.Update(Global.Parameter);
        await context.SaveChangesAsync();
        Functions.ConsoleLog("RemovePropertyInativeOwners End");
    }

    private async Task DebitCompaniesWeekRent()
    {
        Functions.ConsoleLog("DebitCompaniesWeekRent Start");
        var context = Functions.GetDatabaseContext();
        var companies = Global.Companies
                .Where(x => x.CharacterId.HasValue && (x.RentPaymentDate ?? DateTime.MinValue) <= DateTime.Now)
                .ToList();
        if (companies.Count != 0)
        {
            foreach (var company in companies)
            {
                var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == company.CharacterId);
                if (target is not null)
                {
                    if (target.Character.Bank - company.WeekRentValue <= 0)
                    {
                        await company.RemoveOwner(context);
                        continue;
                    }

                    target.RemoveBank(company.WeekRentValue);
                    await target.Save();
                }
                else
                {
                    var character = await context.Characters.FirstOrDefaultAsync(x => x.Id == company.CharacterId);
                    if (character!.Bank - company.WeekRentValue <= 0)
                    {
                        await company.RemoveOwner(context);
                        continue;
                    }

                    character.RemoveBank(company.WeekRentValue);
                    context.Characters.Update(character);
                    await context.SaveChangesAsync();
                }

                company.RenewRent();
                context.Companies.Update(company);
                await context.SaveChangesAsync();

                var financialTransaction = new FinancialTransaction();
                financialTransaction.Create(FinancialTransactionType.Withdraw,
                    company.CharacterId!.Value,
                    company.WeekRentValue,
                    $"Pagamento do Aluguel de {company.Name}");

                await context.FinancialTransactions.AddAsync(financialTransaction);
                await context.SaveChangesAsync();
            }
        }
        Functions.ConsoleLog("DebitCompaniesWeekRent End");
    }

    private static async Task SyncWeather()
    {
        try
        {
            Functions.ConsoleLog("SyncWeather Start");
            if (Global.WeatherInfo.Manual)
                return;

            var unix = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat=34.0536909&lon=-118.242766&appid=401a061ac0ba4fb46e01ec97d0fb5593&units=metric&dt={unix}";

            using var httpClient = new HttpClient();
            Global.WeatherInfo = await httpClient.GetFromJsonAsync<WeatherInfo>(url);
            Global.WeatherInfo!.WeatherType = Global.WeatherInfo.Weather.FirstOrDefault()?.Main switch
            {
                "Drizzle" => Weather.CLEARING,
                "Clouds" => Weather.CLOUDS,
                "Rain" => Weather.RAIN,
                "Thunderstorm" or "Thunder" => Weather.THUNDER,
                "Foggy" or "Fog" or "Mist" or "Smoke" => Weather.FOGGY,
                "Smog" => Weather.SMOG,
                "Overcast" => Weather.OVERCAST,
                "Snowing" or "Snow" => Weather.SNOW,
                "Blizzard" => Weather.BLIZZARD,
                _ => Weather.CLEAR,
            };
            Functions.SetWeatherInfo();
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
        finally
        {
            Functions.ConsoleLog("SyncWeather End");
        }
    }

    private async Task RemoveExpiredInfos()
    {
        Functions.ConsoleLog("RemoveExpiredInfos Start");
        var infos = Global.Infos.Where(x => x.ExpirationDate < DateTime.Now).ToList();
        if (infos.Count > 0)
        {
            var context = Functions.GetDatabaseContext();
            context.Infos.RemoveRange(infos);
            await context.SaveChangesAsync();
            infos.ForEach(x => x.RemoveIdentifier());
            Global.Infos.RemoveAll(infos.Contains);
        }
        Functions.ConsoleLog("RemoveExpiredInfos End");
    }

    private async Task RemoveExpiredGraffitis()
    {
        Functions.ConsoleLog("RemoveExpiredGraffitis Start");
        var graffitis = Global.Graffitis.Where(x => x.ExpirationDate < DateTime.Now).ToList();
        if (graffitis.Count > 0)
        {
            var context = Functions.GetDatabaseContext();
            context.Graffitis.RemoveRange(graffitis);
            await context.SaveChangesAsync();
            graffitis.ForEach(x => x.RemoveIdentifier());
            Global.Graffitis.RemoveAll(graffitis.Contains);
        }
        Functions.ConsoleLog("RemoveExpiredGraffitis End");
    }

    private async void MinuteTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            Functions.ConsoleLog("MinuteTimer_Elapsed Start");
            var itemsToDelete = new List<Item>();

            foreach (var item in Global.Items.Where(x => x.GetCategory() == ItemCategory.BloodSample
                || Functions.CheckIfIsBulletShell(x.GetCategory())))
            {
                var minutesSinceCreation = (DateTime.Now - item.RegisterDate).TotalMinutes;
                if (minutesSinceCreation < 60)
                    continue;

                if (Global.SpawnedPlayers.Any(x => x.GetDimension() == item.Dimension
                    && x.GetPosition().DistanceTo(new(item.PosX, item.PosY, item.PosZ)) <= 10))
                    continue;

                itemsToDelete.Add(item);
            }

            if (itemsToDelete.Count > 0)
            {
                itemsToDelete.ForEach(item => item.DeleteObject());
                var context = Functions.GetDatabaseContext();
                context.Items.RemoveRange(itemsToDelete);
                await context.SaveChangesAsync();
            }

            var sos = Global.HelpRequests.Count(x => x.Type == HelpRequestType.SOS);
            if (sos >= 5)
                await Functions.SendServerMessage($"Existem {sos} SOS pendentes na fila. Utilize /listasos para visualizar.", UserStaff.ServerSupport, false);

            var reports = Global.HelpRequests.Count(x => x.Type == HelpRequestType.Report);
            if (reports >= 5)
                await Functions.SendServerMessage($"Existem {reports} Reports pendentes na fila. Utilize /listareport para visualizar.", UserStaff.JuniorServerAdmin, false);

            Functions.ConsoleLog("MinuteTimer_Elapsed End");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    private async void UCPActionsTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (ProcessingUCPActions)
            return;

        ProcessingUCPActions = true;
        Functions.ConsoleLog("UCPActionsTimer_Elapsed Start");

        try
        {
            var context = Functions.GetDatabaseContext();
            var ucpActionsExecuted = new List<UCPActionExecuted>();

            var ucpActions = await context.UCPActions
                .ToListAsync();
            foreach (var ucpAction in ucpActions)
            {
                if (ucpAction.Type == UCPActionType.GivePremiumPoints)
                {
                    var data = Functions.Deserialize<UCPActionGivePremiumPointsRequest>(ucpAction.Json);

                    var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == ucpAction.UserId);
                    if (target is not null)
                    {
                        target.User.AddPremiumPoints(data.Quantity);
                        context.Users.Update(target.User);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        var user = context.Users.Local.FirstOrDefault(x => x.Id == ucpAction.UserId);
                        user ??= await context.Users.FirstOrDefaultAsync(x => x.Id == ucpAction.UserId);
                        user!.AddPremiumPoints(data.Quantity);
                        context.Users.Update(user);
                        await context.SaveChangesAsync();
                    }
                }
                else if (ucpAction.Type == UCPActionType.AddCharacterApplicationsQuantity)
                {
                    var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == ucpAction.UserId);
                    if (target is not null)
                    {
                        target.User.AddCharacterApplicationsQuantity();
                        context.Users.Update(target.User);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        var user = context.Users.Local.FirstOrDefault(x => x.Id == ucpAction.UserId);
                        user ??= await context.Users.FirstOrDefaultAsync(x => x.Id == ucpAction.UserId);
                        user!.AddCharacterApplicationsQuantity();
                        context.Users.Update(user);
                        await context.SaveChangesAsync();
                    }
                }
                else if (ucpAction.Type == UCPActionType.SendDiscordMessage)
                {
                    var data = Functions.Deserialize<UCPActionSendDiscordMessageRequest>(ucpAction.Json);

                    await Functions.SendDiscordMessage(data.DiscordUserId, data.Message);
                }
                else if (ucpAction.Type == UCPActionType.ReloadParameters)
                {
                    Global.Parameter = await context.Parameters.FirstOrDefaultAsync();

                    LoadParameters();
                }
                else if (ucpAction.Type == UCPActionType.ReloadFurnitures)
                {
                    Global.Furnitures = await context.Furnitures.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.ReloadAnimations)
                {
                    Global.Animations = await context.Animations.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.ReloadCrimes)
                {
                    Global.Crimes = await context.Crimes.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.CreateCharacter)
                {
                    var data = Functions.Deserialize<UCPActionCreateCharacterRequest>(ucpAction.Json);
                    if (data.Namechange)
                    {
                        foreach (var property in Global.Properties.Where(x => x.CharacterId == data.OldCharacterId))
                            await property.ChangeOwner(data.NewCharacterId);

                        var vehicles = await context.Vehicles.Where(x => x.CharacterId == data.OldCharacterId).ToListAsync();
                        foreach (var vehicle in vehicles)
                        {
                            var spawnedVehicle = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == vehicle.Id);
                            if (spawnedVehicle is not null)
                            {
                                spawnedVehicle.VehicleDB.SetOwner(data.NewCharacterId!.Value);
                                context.Vehicles.Update(spawnedVehicle.VehicleDB);
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                vehicle.SetOwner(data.NewCharacterId!.Value);
                                context.Vehicles.Update(vehicle);
                                await context.SaveChangesAsync();
                            }
                        }

                        foreach (var company in Global.Companies.Where(x => x.CharacterId == data.OldCharacterId))
                        {
                            company.SetCharacterId(data.NewCharacterId!.Value);
                            context.Companies.Update(company);
                            await context.SaveChangesAsync();
                        }

                        var target = Global.AllPlayers.FirstOrDefault(x => x.User?.Id == ucpAction.UserId);
                        if (target is not null)
                        {
                            target.User.RemoveNameChange();
                            context.Users.Update(target.User);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            var user = context.Users.Local.FirstOrDefault(x => x.Id == ucpAction.UserId);
                            user ??= await context.Users.FirstOrDefaultAsync(x => x.Id == ucpAction.UserId);
                            user!.RemoveNameChange();
                            context.Users.Update(user);
                            await context.SaveChangesAsync();
                        }
                    }

                    if (data.SendStaffNotification)
                        await Functions.SendServerMessage("Uma nova aplicação de personagem foi recebida.", UserStaff.ServerSupport, true);
                }
                else if (ucpAction.Type == UCPActionType.GMX)
                {
                    foreach (var vehicle in Global.Vehicles)
                        await vehicle.Park(null);

                    foreach (var target in Global.SpawnedPlayers)
                    {
                        await target.Save();
                        target.KickEx("Servidor offline para atualização.");
                    }

                    await Functions.SendServerMessage("Servidor offline para atualização.", UserStaff.ServerSupport, true);
                }

                var ucpActionExecuted = new UCPActionExecuted();
                ucpActionExecuted.Create(ucpAction.Type, ucpAction.UserId, ucpAction.Json, ucpAction.RegisterDate);
                ucpActionsExecuted.Add(ucpActionExecuted);
            }

            if (ucpActionsExecuted.Count > 0)
            {
                await context.UCPActionsExecuted.AddRangeAsync(ucpActionsExecuted);
                context.UCPActions.RemoveRange(ucpActions);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
        finally
        {
            ProcessingUCPActions = false;
            Functions.ConsoleLog("UCPActionsTimer_Elapsed End");
        }
    }

    private static void LoadParameters()
    {
        NAPI.Task.Run(() =>
        {
            Global.IPLs.ForEach(ipl => NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Server:RemoveIpl", ipl));
            Global.IPLs =
                Functions.Deserialize<List<string>>(Global.Parameter.IPLsJSON);
            Global.IPLs.ForEach(ipl => NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Server:RequestIpl", ipl));
            Global.VehicleDismantlingPartsChances =
                Functions.Deserialize<List<VehicleDismantlingPartsChance>>(Global.Parameter.VehicleDismantlingPartsChanceJSON);
            Global.FishingItemsChances =
                Functions.Deserialize<List<FishingItemChance>>(Global.Parameter.FishingItemsChanceJSON);
            Global.WeaponsInfos =
                Functions.Deserialize<List<WeaponInfo>>(Global.Parameter.WeaponsInfosJSON);
            Global.BodyPartsDamages =
                Functions.Deserialize<List<BodyPartDamage>>(Global.Parameter.BodyPartsDamagesJSON);
            Global.AudioRadioStations =
                Functions.Deserialize<List<AudioRadioStation>>(Global.Parameter.AudioRadioStationsJSON);
            Global.PremiumItems =
                Functions.Deserialize<List<PremiumItem>>(Global.Parameter.PremiumItemsJSON);

            NAPI.ClientEventThreadSafe.TriggerClientEventForAll("Server:setArtificialLightsState", Global.Parameter.Blackout);
            NAPI.ClientEventThreadSafe.TriggerClientEventForAll("UpdateWeaponRecoils", Global.Parameter.WeaponsInfosJSON);
        });
    }

    private TimeSpan BleedingTimespan { get; } = TimeSpan.FromSeconds(15);
    private DateTime LastBleedingDate { get; set; } = DateTime.Now;
    private int BleedingDamage { get; } = 2;

    [ServerEvent(Event.Update)]
    public void OnUpdate()
    {
        try
        {
            if (DateTime.Now.Subtract(LastBleedingDate).TotalMilliseconds > BleedingTimespan.TotalMilliseconds)
            {
                LastBleedingDate = DateTime.Now;
                foreach (var player in Global.SpawnedPlayers.Where(x => x.Character.Bleeding).ToList())
                {
                    Functions.ConsoleLog($"Player Timer Bleeding {player.Character.Name}");

                    player.Health -= BleedingDamage;

                    player.Wounds.Add(new Wound
                    {
                        Weapon = "Sangramento",
                        Damage = BleedingDamage,
                        BodyPart = "Sangramento",
                    });

                    player.Emit("CreateBlood");
                }
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }
}