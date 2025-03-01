using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class SpotTypeExtensions
{
    public static string GetDescription(this SpotType spotType)
    {
        return spotType switch
        {
            SpotType.Bank => Resources.Bank,
            SpotType.Company => Resources.Company,
            SpotType.ClothesStore => Resources.ClothesStore,
            SpotType.FactionVehicleSpawn => Resources.FactionVehicleSpawn,
            SpotType.VehicleSeizure => Resources.VehicleSeizure,
            SpotType.VehicleRelease => Resources.VehicleRelease,
            SpotType.BarberShop => Resources.BarberShop,
            SpotType.Morgue => Resources.Morgue,
            SpotType.ForensicLaboratory => Resources.ForensicLaboratory,
            SpotType.DMV => Resources.DMV,
            SpotType.HealMe => Resources.HealMe,
            SpotType.GarbageCollector => Resources.GarbageCollector,
            SpotType.TattooShop => Resources.TattooShop,
            SpotType.VehicleDismantling => Resources.VehicleDismantling,
            SpotType.PlasticSurgery => Resources.PlasticSurgery,
            _ => spotType.ToString(),
        };
    }
}