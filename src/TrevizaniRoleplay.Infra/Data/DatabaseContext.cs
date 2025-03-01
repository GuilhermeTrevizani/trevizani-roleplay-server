using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Infra.Data.Maps;

namespace TrevizaniRoleplay.Infra.Data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<AdminObject> AdminObjects { get; set; }
    public DbSet<Animation> Animations { get; set; }
    public DbSet<Banishment> Banishments { get; set; }
    public DbSet<Blip> Blips { get; set; }
    public DbSet<Body> Bodies { get; set; }
    public DbSet<BodyItem> BodiesItems { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<CharacterItem> CharactersItems { get; set; }
    public DbSet<CrackDen> CrackDens { get; set; }
    public DbSet<CrackDenItem> CrackDensItems { get; set; }
    public DbSet<CrackDenSell> CrackDensSells { get; set; }
    public DbSet<Crime> Crimes { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyCharacter> CompaniesCharacters { get; set; }
    public DbSet<CompanyItem> CompaniesItems { get; set; }
    public DbSet<CompanySell> CompaniesSells { get; set; }
    public DbSet<CompanyTuningPrice> CompaniesTuningPrices { get; set; }
    public DbSet<Confiscation> Confiscations { get; set; }
    public DbSet<ConfiscationItem> ConfiscationsItems { get; set; }
    public DbSet<Dealership> Dealerships { get; set; }
    public DbSet<DealershipVehicle> DealershipsVehicles { get; set; }
    public DbSet<Door> Doors { get; set; }
    public DbSet<Drug> Drugs { get; set; }
    public DbSet<EmergencyCall> EmergencyCalls { get; set; }
    public DbSet<Faction> Factions { get; set; }
    public DbSet<FactionEquipment> FactionsEquipments { get; set; }
    public DbSet<FactionEquipmentItem> FactionsEquipmentsItems { get; set; }
    public DbSet<FactionFrequency> FactionsFrequencies { get; set; }
    public DbSet<FactionStorage> FactionsStorages { get; set; }
    public DbSet<FactionStorageItem> FactionsStoragesItems { get; set; }
    public DbSet<FactionRank> FactionsRanks { get; set; }
    public DbSet<FactionUniform> FactionsUniforms { get; set; }
    public DbSet<FactionUnit> FactionsUnits { get; set; }
    public DbSet<FactionUnitCharacter> FactionsUnitsCharacters { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<Fire> Fires { get; set; }
    public DbSet<ForensicTest> ForensicTests { get; set; }
    public DbSet<ForensicTestItem> ForensicTestsItems { get; set; }
    public DbSet<Furniture> Furnitures { get; set; }
    public DbSet<Graffiti> Graffitis { get; set; }
    public DbSet<HelpRequest> HelpRequests { get; set; }
    public DbSet<Info> Infos { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemTemplate> ItemsTemplates { get; set; }
    public DbSet<Jail> Jails { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<Parameter> Parameters { get; set; }
    public DbSet<PhoneCall> PhonesCalls { get; set; }
    public DbSet<PhoneContact> PhonesContacts { get; set; }
    public DbSet<PhoneGroup> PhonesGroups { get; set; }
    public DbSet<PhoneGroupUser> PhonesGroupsUsers { get; set; }
    public DbSet<PhoneMessage> PhonesMessages { get; set; }
    public DbSet<PhoneMessageRead> PhonesMessagesReads { get; set; }
    public DbSet<PhoneNumber> PhonesNumbers { get; set; }
    public DbSet<PremiumPointPurchase> PremiumPointPurchases { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyEntrance> PropertiesEntrances { get; set; }
    public DbSet<PropertyFurniture> PropertiesFurnitures { get; set; }
    public DbSet<PropertyItem> PropertiesItems { get; set; }
    public DbSet<Punishment> Punishments { get; set; }
    public DbSet<SeizedVehicle> SeizedVehicles { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Smuggler> Smugglers { get; set; }
    public DbSet<Spot> Spots { get; set; }
    public DbSet<TruckerLocation> TruckerLocations { get; set; }
    public DbSet<TruckerLocationDelivery> TruckerLocationsDeliveries { get; set; }
    public DbSet<UCPAction> UCPActions { get; set; }
    public DbSet<UCPActionExecuted> UCPActionsExecuted { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleItem> VehiclesItems { get; set; }
    public DbSet<Wanted> Wanted { get; set; }
    public DbSet<WeaponId> WeaponsIds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(UserMap))!);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            ChangeTracker.Clear();
        }
    }
}