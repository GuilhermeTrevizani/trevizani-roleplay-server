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
            0, "https://i.imgur.com/z5afrcD.png", "bkr_prop_money_sorted_01");
        await InsertItemTemplate(new Guid(Constants.VEHICLE_PART_ITEM_TEMPLATE_ID), ItemCategory.VehiclePart, Resources.VehiclePart,
            100, "https://i.imgur.com/w3D4MtH.png", "imp_prop_impexp_exhaust_06");
        await InsertItemTemplate(new Guid(Constants.BLOOD_SAMPLE_ITEM_TEMPLATE_ID), ItemCategory.BloodSample, Resources.BloodSample,
            1, "https://i.imgur.com/5pguCOS.png", "p_bloodsplat_s");
        await InsertItemTemplate(new Guid(Constants.PISTOL_AMMO_ITEM_TEMPLATE_ID), ItemCategory.PistolAmmo, Resources.PistolAmmo,
           1, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.SHOTGUN_AMMO_ITEM_TEMPLATE_ID), ItemCategory.ShotgunAmmo, Resources.ShotgunAmmo,
           1, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.ASSAULT_RIFLE_AMMO_ITEM_TEMPLATE_ID), ItemCategory.AssaultRifleAmmo, Resources.AssaultRifleAmmo,
           1, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.LIGHT_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID), ItemCategory.LightMachineGunAmmo, Resources.LightMachineGunAmmo,
           1, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.SNIPER_RIFLE_AMMO_ITEM_TEMPLATE_ID), ItemCategory.SniperRifleAmmo, Resources.SniperRifleAmmo,
           1, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.SUB_MACHINE_GUN_AMMO_ITEM_TEMPLATE_ID), ItemCategory.SubMachineGunAmmo, Resources.SubMachineGunAmmo,
           1, "https://i.imgur.com/QoQqfEq.png", string.Empty);
        await InsertItemTemplate(new Guid(Constants.PISTOL_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.PistolBulletShell, Resources.PistolBulletShell,
           1, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshoth4_shell");
        await InsertItemTemplate(new Guid(Constants.SHOTGUN_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.ShotgunBulletShell, Resources.ShotgunBulletShell,
           1, "https://i.imgur.com/zUZ41G6.png", "prop_sgun_casing");
        await InsertItemTemplate(new Guid(Constants.ASSAULT_RIFLE_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.AssaultRifleBulletShell, Resources.AssaultRifleBulletShell,
           1, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshot_shell");
        await InsertItemTemplate(new Guid(Constants.LIGHT_MACHINE_GUN_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.LightMachineGunBulletShell, Resources.LightMachineGunBulletShell,
           1, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshot_shell");
        await InsertItemTemplate(new Guid(Constants.SNIPER_RIFLE_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.SniperRifleBulletShell, Resources.SniperRifleBulletShell,
           1, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshot_shell");
        await InsertItemTemplate(new Guid(Constants.SUB_MACHINE_GUN_BULLET_SHELL_ITEM_TEMPLATE_ID), ItemCategory.SubMachineGunBulletShell, Resources.SubMachineGunBulletShell,
           1, "https://i.imgur.com/zUZ41G6.png", "w_pi_singleshoth4_shell");
    }

    private async Task InsertItemTemplate(Guid id, ItemCategory itemCategory, string name, uint weight, string image, string objectModel)
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
        Global.DatabaseConnection = GetSetting("databaseConnection");

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
        await context.Vehicles.Where(x => x.Spawned).ExecuteUpdateAsync(x => x.SetProperty(y => y.Spawned, false));
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

        Global.Commands = [.. Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)];
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
            await DebitCompaniesWeekRent();
            await SyncWeather();
            Functions.ConsoleLog("MainTimer_Elapsed End");
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
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
            var context = Functions.GetDatabaseContext();

            var itemsToDelete = new List<Item>();

            foreach (var item in Global.Items.Where(x => x.GetCategory() == ItemCategory.BloodSample
                || GlobalFunctions.CheckIfIsBulletShell(x.GetCategory())))
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
                context.Items.RemoveRange(itemsToDelete);
                await context.SaveChangesAsync();
            }

            var sos = Global.HelpRequests.Count(x => x.Type == HelpRequestType.SOS);
            if (sos >= 5)
                await Functions.SendServerMessage($"Existem {sos} SOS pendentes na fila. Utilize /listasos para visualizar.", UserStaff.Tester, false);

            var reports = Global.HelpRequests.Count(x => x.Type == HelpRequestType.Report);
            if (reports >= 5)
                await Functions.SendServerMessage($"Existem {reports} Reports pendentes na fila. Utilize /listareport para visualizar.", UserStaff.GameAdmin, false);

            var characterIds = Global.SpawnedPlayers.Select(x => x.Character.Id).ToList();
            await context.Characters.Where(x => x.Connected && !characterIds.Contains(x.Id)).ExecuteUpdateAsync(x => x.SetProperty(y => y.Connected, false));

            var serverStatic = new ServerStatistic(Global.AllPlayers.Count(), Global.SpawnedPlayers.Count());
            await context.ServerStatistics.AddAsync(serverStatic);
            await context.SaveChangesAsync();

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
                    Global.Parameter = (await context.Parameters.FirstOrDefaultAsync())!;

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
                                await spawnedVehicle.VehicleDB.ChangeOwner(data.NewCharacterId!.Value);
                                context.Vehicles.Update(spawnedVehicle.VehicleDB);
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                await vehicle.ChangeOwner(data.NewCharacterId!.Value);
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
                        await Functions.SendServerMessage("Uma nova aplicação de personagem foi recebida.", UserStaff.Tester, true);
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

                    await Functions.SendServerMessage("Servidor offline para atualização.", UserStaff.Tester, true);
                }
                else if (ucpAction.Type == UCPActionType.ReloadCharactersAcess)
                {
                    foreach (var player in Global.SpawnedPlayers)
                    {
                        player.PropertiesAccess = await context.CharactersProperties.Where(x => x.CharacterId == player.Character.Id).Select(x => x.PropertyId).ToListAsync();
                        player.VehiclesAccess = await context.CharactersVehicles.Where(x => x.CharacterId == player.Character.Id).Select(x => x.VehicleId).ToListAsync();
                    }
                }
                else if (ucpAction.Type == UCPActionType.ReloadFactions)
                {
                    Global.Factions = await context.Factions.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.ReloadFactionVehicle)
                {
                    var data = Functions.Deserialize<UCPActionReloadFactionVehicleRequest>(ucpAction.Json);

                    var vehicle = await context.Vehicles.FirstOrDefaultAsync(x => x.Id == data.Id);

                    var spawnedVehicle = Global.Vehicles.FirstOrDefault(x => x.VehicleDB.Id == vehicle!.Id);
                    if (spawnedVehicle is null)
                    {
                        vehicle!.SetDescription(data.Description);
                        context.Vehicles.Update(vehicle);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        spawnedVehicle.VehicleDB.SetDescription(data.Description);
                    }

                    await Functions.WriteLog(LogType.Faction, $"Editar Veículo | {vehicle!.Plate} {data.Description}", ucpAction.UserId);
                }
                else if (ucpAction.Type == UCPActionType.ReloadFactionsRanks)
                {
                    Global.FactionsRanks = await context.FactionsRanks.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.SaveFactionMember)
                {
                    var data = Functions.Deserialize<UCPActionSaveFactionMemberRequest>(ucpAction.Json);

                    var character = context.Characters.Local.FirstOrDefault(x => x.Id == data.Id);
                    character ??= await context.Characters.FirstOrDefaultAsync(x => x.Id == data.Id);

                    var faction = Global.Factions.FirstOrDefault(x => x.Id == character!.FactionId)!;

                    var factionRank = Global.FactionsRanks.FirstOrDefault(x => x.Id == data.FactionRankId)!;

                    var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character!.Id);
                    if (target is not null)
                    {
                        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == ucpAction.UserId);
                        target.Character.UpdateFaction(data.FactionRankId, Functions.Serialize(data.Flags));
                        target.FactionFlags = [.. data.Flags];
                        target.SendMessage(MessageType.Success, $"{user!.Name} alterou suas informações na facção.");
                        await target.Save();
                    }
                    else
                    {
                        character!.UpdateFaction(data.FactionRankId, Functions.Serialize(data.Flags));
                        context.Characters.Update(character);
                        await context.SaveChangesAsync();
                    }

                    await Functions.WriteLog(LogType.Faction, $"Salvar Membro Facção {faction.Name} {character!.Name} {factionRank.Name} {string.Join(", ", data.Flags.Select(x => x.GetDescription()))}", ucpAction.UserId);
                }
                else if (ucpAction.Type == UCPActionType.RemoveFactionMember)
                {
                    var characterId = Functions.Deserialize<Guid>(ucpAction.Json);

                    var character = context.Characters.Local.FirstOrDefault(x => x.Id == characterId);
                    character ??= await context.Characters.FirstOrDefaultAsync(x => x.Id == characterId);

                    var faction = Global.Factions.FirstOrDefault(x => x.Id == character!.FactionId)!;

                    var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == character!.Id);
                    if (target is not null)
                    {
                        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == ucpAction.UserId);
                        await target.RemoveFromFaction();
                        await target.Save();
                        target.SendMessage(MessageType.Success, $"{user!.Name} expulsou você da facção.");
                    }
                    else
                    {
                        if (faction.HasDuty)
                        {
                            var items = await context.CharactersItems.Where(x => x.CharacterId == character!.Id).ToListAsync();
                            items = [.. items.Where(x => !Functions.CanDropItem(faction, x))];
                            if (items.Count > 0)
                                context.CharactersItems.RemoveRange(items);
                        }

                        character!.ResetFaction();

                        context.Characters.Update(character);
                        await context.SaveChangesAsync();
                    }

                    await Functions.WriteLog(LogType.Faction, $"Expulsar Facção {faction.Name} {character!.Name}", ucpAction.UserId);
                }
                else if (ucpAction.Type == UCPActionType.StaffSaveFaction)
                {
                    var data = Functions.Deserialize<FactionRequest>(ucpAction.Json);

                    Character? leader = null;
                    if (!string.IsNullOrWhiteSpace(data.Leader))
                        leader = await context.Characters.FirstOrDefaultAsync(x => x.Name == data.Leader);

                    var isNew = !data.Id.HasValue;
                    var faction = new Faction();
                    if (isNew)
                    {
                        faction.Create(data.Name, data.Type, data.Slots, leader?.Id, data.ShortName);
                    }
                    else
                    {
                        faction = Global.Factions.FirstOrDefault(x => x.Id == data.Id)!;

                        if (faction.CharacterId.HasValue && faction.CharacterId != leader?.Id)
                        {
                            var oldLeader = await context.Characters.FirstOrDefaultAsync(x => x.Id == faction.CharacterId)!;
                            var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == oldLeader!.Id);
                            if (target is not null)
                            {
                                await target.RemoveFromFaction();
                            }
                            else
                            {
                                oldLeader!.ResetFaction();
                                context.Characters.Update(oldLeader);

                                if (faction.HasDuty)
                                {
                                    var items = await context.CharactersItems.Where(x => x.CharacterId == oldLeader.Id).ToListAsync();
                                    items = items.Where(x => !Functions.CanDropItem(faction, x)).ToList();
                                    if (items.Count > 0)
                                        context.CharactersItems.RemoveRange(items);
                                }

                                await context.SaveChangesAsync();
                            }
                        }

                        faction.Update(data.Name, data.Type, data.Slots, leader?.Id, data.ShortName);
                    }

                    if (isNew)
                        await context.Factions.AddAsync(faction);
                    else
                        context.Factions.Update(faction);

                    await context.SaveChangesAsync();

                    if (isNew)
                        Global.Factions.Add(faction);

                    if (leader is not null && leader.FactionId != faction.Id)
                    {
                        var lastRank = Global.FactionsRanks
                            .Where(x => x.FactionId == faction.Id)
                            .OrderByDescending(x => x.Position)
                            .FirstOrDefault();
                        if (lastRank is null)
                        {
                            lastRank = new();
                            lastRank.Create(faction.Id, 1, "Rank 1", 0);
                            await context.FactionsRanks.AddAsync(lastRank);
                            await context.SaveChangesAsync();
                            Global.FactionsRanks.Add(lastRank);
                        }

                        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == leader.Id);
                        if (target is not null)
                        {
                            target.Character.SetFaction(faction.Id, lastRank.Id, faction.Type == FactionType.Criminal);
                            await target.Save();
                        }
                        else
                        {
                            leader.SetFaction(faction.Id, lastRank.Id, faction.Type == FactionType.Criminal);
                            context.Characters.Update(leader);
                            await context.SaveChangesAsync();
                        }
                    }

                    await Functions.WriteLog(LogType.Staff, $"Gravar Facção | {Functions.Serialize(faction)}", ucpAction.UserId);
                }
                else if (ucpAction.Type == UCPActionType.ReloadFactionsFrequencies)
                {
                    Global.FactionsFrequencies = await context.FactionsFrequencies.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.ReloadFactionsEquipments)
                {
                    Global.FactionsEquipments = await context.FactionsEquipments.Include(x => x.Items).ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.ReloadItemsTemplates)
                {
                    Global.ItemsTemplates = await context.ItemsTemplates.ToListAsync();
                }
                else if (ucpAction.Type == UCPActionType.ReloadDrugs)
                {
                    Global.Drugs = await context.Drugs.ToListAsync();
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
                        Weapon = Resources.Bleeding,
                        Damage = BleedingDamage,
                        BodyPart = Resources.Bleeding,
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