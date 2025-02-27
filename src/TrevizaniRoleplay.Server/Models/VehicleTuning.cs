namespace TrevizaniRoleplay.Server.Models;

public class VehicleTuning
{
    public Guid? VehicleId { get; set; }
    public Guid? TargetId { get; set; }
    public Guid? CompanyId { get; set; }
    public bool Staff { get; set; }
    public List<Mod> Mods { get; set; } = [];
    public List<Mod> CurrentMods { get; set; } = [];
    public byte Repair { get; set; }
    public int RepairValue { get; set; }
    public byte WheelType { get; set; }
    public byte CurrentWheelType { get; set; }
    public byte WheelVariation { get; set; }
    public byte CurrentWheelVariation { get; set; }
    public byte WheelColor { get; set; }
    public byte CurrentWheelColor { get; set; }
    public int WheelValue { get; set; }
    public string Color1 { get; set; } = string.Empty;
    public string CurrentColor1 { get; set; } = string.Empty;
    public string Color2 { get; set; } = string.Empty;
    public string CurrentColor2 { get; set; } = string.Empty;
    public int ColorValue { get; set; }
    public string NeonColor { get; set; } = string.Empty;
    public string CurrentNeonColor { get; set; } = string.Empty;
    public byte NeonLeft { get; set; }
    public byte CurrentNeonLeft { get; set; }
    public byte NeonRight { get; set; }
    public byte CurrentNeonRight { get; set; }
    public byte NeonFront { get; set; }
    public byte CurrentNeonFront { get; set; }
    public byte NeonBack { get; set; }
    public byte CurrentNeonBack { get; set; }
    public int NeonValue { get; set; }
    public byte HeadlightColor { get; set; }
    public float LightsMultiplier { get; set; }
    public int XenonColorValue { get; set; }
    public byte CurrentHeadlightColor { get; set; }
    public float CurrentLightsMultiplier { get; set; }
    public byte WindowTint { get; set; }
    public byte CurrentWindowTint { get; set; }
    public int WindowTintValue { get; set; }
    public string TireSmokeColor { get; set; } = string.Empty;
    public string CurrentTireSmokeColor { get; set; } = string.Empty;
    public int TireSmokeColorValue { get; set; }
    public byte ProtectionLevel { get; set; }
    public byte CurrentProtectionLevel { get; set; }
    public int ProtectionLevelValue { get; set; }
    public byte XMR { get; set; }
    public byte CurrentXMR { get; set; }
    public int XMRValue { get; set; }
    public byte Livery { get; set; }
    public byte CurrentLivery { get; set; }
    public int LiveryValue { get; set; }
    public bool[] Extras { get; set; } = [];
    public int ExtrasValue { get; set; }
    public bool[] CurrentExtras { get; set; } = [];
    public byte Drift { get; set; }
    public byte CurrentDrift { get; set; }
    public int DriftValue { get; set; }

    public class Mod
    {
        public byte Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UnitaryValue { get; set; }
        public int Value { get; set; }
        public short Current { get; set; }
        public short Selected { get; set; }
        public int MaxMod { get; set; }
        public bool MultiplyValue { get; set; }
    }
}