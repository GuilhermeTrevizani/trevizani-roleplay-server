using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class CompanyTuningPriceTypeExtensions
{
    public static string GetDescription(this CompanyTuningPriceType companyTuningPriceType)
    {
        return companyTuningPriceType switch
        {
            CompanyTuningPriceType.Spoilers => Resources.Spoilers,
            CompanyTuningPriceType.FrontBumper => Resources.FrontBumper,
            CompanyTuningPriceType.RearBumper => Resources.RearBumper,
            CompanyTuningPriceType.SideSkirt => Resources.SideSkirt,
            CompanyTuningPriceType.Exhaust => Resources.Exhaust,
            CompanyTuningPriceType.Frame => Resources.Frame,
            CompanyTuningPriceType.Grille => Resources.Grille,
            CompanyTuningPriceType.Hood => Resources.Hood,
            CompanyTuningPriceType.Fender => Resources.Fender,
            CompanyTuningPriceType.RightFender => Resources.RightFender,
            CompanyTuningPriceType.Roof => Resources.Roof,
            CompanyTuningPriceType.Engine => Resources.Engine,
            CompanyTuningPriceType.Brakes => Resources.Brakes,
            CompanyTuningPriceType.Transmission => Resources.Transmission,
            CompanyTuningPriceType.Horns => Resources.Horns,
            CompanyTuningPriceType.Suspension => Resources.Suspension,
            CompanyTuningPriceType.Armor => Resources.Armor,
            CompanyTuningPriceType.Turbo => Resources.Turbo,
            CompanyTuningPriceType.Xenon => Resources.Xenon,
            CompanyTuningPriceType.PlateHolders => Resources.PlateHolders,
            CompanyTuningPriceType.TrimDesign or CompanyTuningPriceType.Trim => Resources.Trim,
            CompanyTuningPriceType.Ornaments => Resources.Ornaments,
            CompanyTuningPriceType.DialDesign => Resources.DialDesign,
            CompanyTuningPriceType.DoorSpeakers => Resources.DoorSpeakers,
            CompanyTuningPriceType.Seats => Resources.Seats,
            CompanyTuningPriceType.SteeringWheel => Resources.SteeringWheel,
            CompanyTuningPriceType.ShiftLever => Resources.ShiftLever,
            CompanyTuningPriceType.Plaques => Resources.Plate,
            CompanyTuningPriceType.Speakers => Resources.Speakers,
            CompanyTuningPriceType.Trunk => Resources.Trunk,
            CompanyTuningPriceType.Hydraulics => Resources.Hydraulics,
            CompanyTuningPriceType.EngineBlock => Resources.EngineBlock,
            CompanyTuningPriceType.AirFilter => Resources.AirFilter,
            CompanyTuningPriceType.Struts => Resources.Struts,
            CompanyTuningPriceType.ArchCover => Resources.ArchCover,
            CompanyTuningPriceType.Aerials => Resources.Aerials,
            CompanyTuningPriceType.Tank => Resources.Tank,
            CompanyTuningPriceType.DoorsTrim1 or CompanyTuningPriceType.DoorsTrim2 => Resources.DoorsTrim,
            CompanyTuningPriceType.Enveloping or CompanyTuningPriceType.Enveloping2 => Resources.Enveloping,
            CompanyTuningPriceType.WindowTint2 or CompanyTuningPriceType.WindowTint2 => Resources.WindowTint,
            CompanyTuningPriceType.Color1 => Resources.PrimaryColor,
            CompanyTuningPriceType.Color2 => Resources.SecondaryColor,
            CompanyTuningPriceType.DashboardColor => Resources.DashboardColor,
            CompanyTuningPriceType.TrimColor => Resources.TrimColor,
            CompanyTuningPriceType.Drift => Resources.Drift,
            CompanyTuningPriceType.Extra => Resources.Extra,
            CompanyTuningPriceType.Livery => Resources.Livery,
            CompanyTuningPriceType.XMR => Resources.XMR,
            CompanyTuningPriceType.ProtectionLevel => Resources.ProtectionLevel,
            CompanyTuningPriceType.TireSmokeColor => Resources.TireSmokeColor,
            CompanyTuningPriceType.Insufilm => Resources.Insufilm,
            CompanyTuningPriceType.XenonColor => Resources.XenonColor,
            _ => companyTuningPriceType.ToString(),
        };
    }
}