using Discord.WebSocket;
using GTANetworkAPI;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrevizaniRoleplay.Server.Factories;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server;

public sealed class Global
{
    public static Color MainRgba { get; } = new Color(16, 151, 230, 75);

    public static Parameter Parameter { get; set; } = default!;

    public static List<Domain.Entities.Blip> Blips { get; set; } = [];

    public static List<Faction> Factions { get; set; } = [];

    public static List<FactionRank> FactionsRanks { get; set; } = [];

    public static List<Property> Properties { get; set; } = [];

    public static List<Job> Jobs { get; set; } = [];

    public static List<Spot> Spots { get; set; } = [];

    public static List<FactionStorage> FactionsStorages { get; set; } = [];

    public static List<FactionStorageItem> FactionsStoragesItems { get; set; } = [];

    public static List<HelpRequest> HelpRequests { get; set; } = [];

    public static List<EmergencyCall> EmergencyCalls { get; set; } = [];

    public static DiscordSocketClient? DiscordClient { get; set; }

    public static List<Spotlight> Spotlights { get; set; } = [];

    public static IEnumerable<MyPlayer> AllPlayers => NAPI.Pools.GetAllPlayers().Cast<MyPlayer>();

    public static IEnumerable<MyPlayer> SpawnedPlayers => AllPlayers.Where(x => x.Character?.PersonalizationStep == CharacterPersonalizationStep.Ready);

    public static IEnumerable<MyVehicle> Vehicles => NAPI.Pools.GetAllVehicles().Cast<MyVehicle>();

    public static List<FactionUnit> FactionsUnits { get; set; } = [];

    public static List<Item> Items { get; set; } = [];

    public static WeatherInfo WeatherInfo { get; set; } = new();

    public static List<Door> Doors { get; set; } = [];

    public static List<string> IPLs { get; set; } = [];

    public static List<Info> Infos { get; set; } = [];

    public static List<CrackDen> CrackDens { get; set; } = [];

    public static List<CrackDenItem> CrackDensItems { get; set; } = [];

    public static List<TruckerLocation> TruckerLocations { get; set; } = [];

    public static List<TruckerLocationDelivery> TruckerLocationsDeliveries { get; set; } = [];

    public static List<AudioSpot> AudioSpots { get; set; } = [];

    public static List<Furniture> Furnitures { get; set; } = [];

    public static List<Animation> Animations { get; set; } = [];

    public static List<Company> Companies { get; set; } = [];

    public static List<MethodInfo> Commands { get; set; } = [];

    public static ulong AnnouncementDiscordChannel { get; set; }

    public static ulong GovernmentAnnouncementDiscordChannel { get; set; }

    public static ulong StaffDiscordChannel { get; set; }

    public static ulong CompanyAnnouncementDiscordChannel { get; set; }

    public static ulong PremiumBronzeDiscordRole { get; set; }

    public static ulong PremiumSilverDiscordRole { get; set; }

    public static ulong PremiumGoldDiscordRole { get; set; }

    public static ulong MainDiscordGuild { get; set; }

    public static IEnumerable<MyObject> Objects => NAPI.Pools.GetAllObjects().Cast<MyObject>();

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public static IEnumerable<MyColShape> ColShapes => NAPI.Pools.GetAllColShapes().Cast<MyColShape>();

    public static IEnumerable<MyMarker> Markers => NAPI.Pools.GetAllMarkers().Cast<MyMarker>();

    public static IEnumerable<MyBlip> MyBlips => NAPI.Pools.GetAllBlips().Cast<MyBlip>();

    public static List<FactionFrequency> FactionsFrequencies { get; set; } = [];

    public static bool TransmissionActive { get; set; }

    public static List<FactionUniform> FactionsUniforms { get; set; } = [];

    public static List<Dealership> Dealerships { get; set; } = [];

    public static List<DealershipVehicle> DealershipsVehicles { get; set; } = [];

    public static List<Smuggler> Smugglers { get; set; } = [];

    public static List<MyPed> Peds { get; set; } = [];

    public static List<VehicleDismantlingPartsChance> VehicleDismantlingPartsChances { get; set; } = [];

    public static List<ItemTemplate> ItemsTemplates { get; set; } = [];

    public static List<FishingItemChance> FishingItemsChances { get; set; } = [];

    public static List<Announcement> Announcements { get; set; } = [];

    public static List<Crime> Crimes { get; set; } = [];

    public static List<WeaponInfo> WeaponsInfos { get; set; } = [];

    public static List<Graffiti> Graffitis { get; set; } = [];

    public static List<BodyPartDamage> BodyPartsDamages { get; set; } = [];

    public static List<Body> Bodies { get; set; } = [];

    public static List<FactionEquipment> FactionsEquipments { get; set; } = [];

    public static List<PremiumItem> PremiumItems { get; set; } = [];

    public static List<AudioRadioStation> AudioRadioStations { get; set; } = [];

    public static List<Drug> Drugs { get; set; } = [];

    public static List<AdminObject> AdminObjects { get; set; } = [];

    public static List<PhoneGroup> PhonesGroups { get; set; } = [];

    public static List<Fire> Fires { get; set; } = [];

    public static List<ActiveFire> ActiveFires { get; set; } = [];

    public static bool StaffToggleBlocked { get; set; }

    public static ulong RoleplayAnnouncementDiscordChannel { get; set; }

    public static string DiscordClientId { get; set; } = string.Empty;

    public static string DiscordClientSecret { get; set; } = string.Empty;

    public static List<Particle> Particles { get; set; } = [];

    public static ulong FirefighterEmergencyCallDiscordChannel { get; set; }

    public static ulong PoliceEmergencyCallDiscordChannel { get; set; }

    public static string DatabaseConnection { get; set; } = string.Empty;

    public static string CommandsHelpJson { get; set; } = string.Empty;

    public static string CommandsChatJson { get; set; } = string.Empty;
}