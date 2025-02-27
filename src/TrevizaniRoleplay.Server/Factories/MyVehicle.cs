using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Factories;

public class MyVehicle(NetHandle netHandle) : GTANetworkAPI.Vehicle(netHandle)
{
    public Domain.Entities.Vehicle VehicleDB { get; set; } = new();

    /// <summary>
    /// Character name who used /fspawn or /valugar
    /// </summary>
    public string NameInCharge { get; set; } = string.Empty;

    /// <summary>
    /// true = Closed
    /// 0 = Front Left Door
    /// 1 = Front Right Door
    /// 2 = Back Left Door
    /// 3 = Back Right Door
    /// 4 = Hood
    /// </summary>
    public List<bool> DoorsStates { get; set; } = [true, true, true, true, true, true];

    public int Speed
    {
        get
        {
            var velocity = NAPI.Entity.GetEntityVelocity(this);
            var speed = Math.Sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y +
                velocity.Z * velocity.Z
            );
            return Convert.ToInt32(Math.Abs(speed * 3.6));
        }
    }

    public int FuelHUD => Convert.ToInt32(Convert.ToDecimal(VehicleDB.Fuel) / Convert.ToDecimal(VehicleDB.GetMaxFuel()) * 100);

    public bool HasFuelTank => !(string.IsNullOrWhiteSpace(VehicleDB.Model)
            || VehicleDB.Model.ToLower() == VehicleModel.Polmav.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModelMods.AS332.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Predator.ToString().ToLower()
            || new List<VehicleClass> { VehicleClass.Boats, VehicleClass.Cycles, VehicleClass.Planes, VehicleClass.Helicopters }.Contains(GetClass()));

    public List<VehicleDamage> Damages { get; set; } = [];

    public bool HasWindows => !(VehicleDB.Model.ToLower() == VehicleModel.Policeb.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Predator.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Wastlndr.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Raptor.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Veto.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Veto2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Banshee2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Voltic2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Airtug.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Caddy.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Caddy2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Caddy3.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Forklift.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Mower.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Tractor.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Bifta.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer3.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer4.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer5.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Bodhi2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune3.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune4.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune5.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Outlaw.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Trophytruck.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Vagrant.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Verus.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Winky.ToString().ToLower()
            || new List<VehicleClass> { VehicleClass.Boats, VehicleClass.Cycles, VehicleClass.Motorcycles }.Contains(GetClass()));

    public bool SpotlightActive { get; set; }

    public bool HasStorage => string.IsNullOrWhiteSpace(VehicleDB.Model)
            || !(VehicleDB.Model.ToLower() == VehicleModel.Policeb.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Predator.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Wastlndr.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Raptor.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Veto.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Veto2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Banshee2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Voltic2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Tractor.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Tractor2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Tractor3.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Bifta.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer3.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer4.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Blazer5.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Bodhi2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune2.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune3.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune4.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Dune5.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Outlaw.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Trophytruck.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Vagrant.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Verus.ToString().ToLower()
            || VehicleDB.Model.ToLower() == VehicleModel.Winky.ToString().ToLower()
            || new List<VehicleClass> { VehicleClass.Boats, VehicleClass.Cycles, VehicleClass.Motorcycles }.Contains(GetClass()));

    public System.Timers.Timer Timer { get; set; } = new(TimeSpan.FromMinutes(1));

    public List<Spot> CollectSpots { get; set; } = [];

    public TruckerLocation? TruckerLocation { get; set; }

    public AudioSpot? AudioSpot { get; set; }

    public AudioSpot? AlarmAudioSpot { get; set; }

    public Vector3 ICPosition
    {
        get
        {
            if (Dimension == 0)
                return Position;

            var property = Global.Properties.FirstOrDefault(x => x.Number == Dimension);
            return property is null ? Position : property.GetEntrancePosition();
        }
    }

    public bool HasSpikeStrip { get; set; } = true;

    public string Identifier => string.IsNullOrWhiteSpace(VehicleDB.Model) ? $"{Id} {GetNumberPlate()}" : $"{VehicleDB.Model} {VehicleDB.Plate}".ToUpper();

    public DateTime? RentExpirationDate { get; set; }

    public List<Guid> FactionsEquipmentsIds { get; set; } = [];

    public MyVehicleSpawnType SpawnType { get; set; } = MyVehicleSpawnType.Normal;

    public MyPlayer? Driver => Global.SpawnedPlayers.FirstOrDefault(x => x.VehicleSeat == Constants.VEHICLE_SEAT_DRIVER && x.Vehicle == this);

    public bool Frozen
    {
        get
        {
            return GetSharedDataEx<bool>("Frozen");
        }
        set
        {
            SetSharedDataEx("Frozen", value);
        }
    }

    private bool[] OpenedWindows { get; set; } = [false, false, false, false];

    public MyVehicle? Attached { get; set; }

    public bool IsHelicopter => GetClass() == VehicleClass.Helicopters
        || Model == Functions.Hash(VehicleModelMods.AW139.ToString())
        || Model == Functions.Hash(VehicleModelMods.LCPDMAV.ToString())
        || Model == Functions.Hash(VehicleModelMods.NEWSMAV.ToString())
        || Model == Functions.Hash(VehicleModelMods.AS350.ToString())
        || Model == Functions.Hash(VehicleModelMods.AS332.ToString());

    public async Task Park(MyPlayer? player)
    {
        Timer?.Stop();
        StopAlarm();
        AudioSpot?.RemoveAllClients();
        AudioSpot = null;

        if (SpawnType == MyVehicleSpawnType.Normal)
        {
            VehicleDB.SetDamages(GetEngineHealth(), GetBodyHealth(), Functions.Serialize(Damages));

            var context = Functions.GetDatabaseContext();
            context.Vehicles.Update(VehicleDB);
            await context.SaveChangesAsync();

            if (player is not null)
                await player.WriteLog(LogType.ParkVehicle, Identifier, null);
        }

        Functions.RunOnMainThread(() =>
        {
            Delete();
        });
    }

    public void RepairEx()
    {
        Damages = [];
        OpenedWindows = [false, false, false, false];
        ResetSharedDataEx("TyresBurst");

        Functions.RunOnMainThread(Repair);
    }

    public void ShowInventory(MyPlayer player, bool update)
    {
        player.ShowInventory(InventoryShowType.Vehicle,
            $"{VehicleDB.Model.ToUpper()} {VehicleDB.Plate.ToUpper()}",
            Functions.Serialize(
                VehicleDB.Items!.Select(x => new
                {
                    x.Id,
                    Image = x.GetImage(),
                    Name = x.GetName(),
                    x.Quantity,
                    x.Slot,
                    Extra = x.GetExtra(),
                    Weight = x.GetWeight(),
                    IsStack = x.GetIsStack(),
                })
        ), update, VehicleDB.Id);
    }

    public bool CanAccess(MyPlayer player)
    {
        return VehicleDB.CharacterId == player.Character.Id
            || (VehicleDB.FactionId.HasValue && VehicleDB.FactionId == player.Character.FactionId)
            || (!string.IsNullOrWhiteSpace(NameInCharge) && NameInCharge == player.Character.Name)
            || player.Items.Any(x => x.GetCategory() == ItemCategory.VehicleKey && x.Subtype == VehicleDB.LockNumber);
    }

    public async Task ActivateProtection(MyPlayer player)
    {
        if (VehicleDB.ProtectionLevel >= 1)
            StartAlarm();

        if (VehicleDB.ProtectionLevel >= 2)
        {
            var context = Functions.GetDatabaseContext();
            var targetCellphone = (await context.Characters.FirstOrDefaultAsync(x => x.Id == VehicleDB.CharacterId))?.Cellphone ?? 0;
            if (targetCellphone != 0)
            {
                var phoneMessage = new PhoneMessage();
                phoneMessage.CreateTextToContact(targetCellphone, Constants.EMERGENCY_NUMBER,
                    $"O alarme do seu {Identifier} foi acionado.");

                await Functions.SendSMS(null, [targetCellphone], phoneMessage);
            }
        }

        if (VehicleDB.ProtectionLevel >= 3)
        {
            player.VehicleEmergencyCall = new EmergencyCall();
            player.VehicleEmergencyCall.Create(EmergencyCallType.Police, Constants.EMERGENCY_NUMBER, ICPosition.X, ICPosition.Y,
                $"O alarme do veículo {VehicleDB.Model.ToUpper()} {VehicleDB.Plate.ToUpper()} foi acionado.", string.Empty, string.Empty);
            player.AreaNameType = AreaNameType.VehicleEmergencyCall;
            player.Emit(Constants.SET_AREA_NAME);
        }
    }

    public void StartAlarm()
    {
        if (AlarmAudioSpot is null)
        {
            AlarmAudioSpot = new AudioSpot
            {
                Dimension = Dimension,
                VehicleId = Id,
                Source = Constants.URL_AUDIO_VEHICLE_ALARM,
                Loop = true,
                Range = 50,
            };
            AlarmAudioSpot.SetupAllClients();
        }
    }

    public void StopAlarm()
    {
        AlarmAudioSpot?.RemoveAllClients();
        AlarmAudioSpot = null;
    }

    public void SetDefaultMods()
    {
        Functions.RunOnMainThread(() =>
        {
            foreach (var mod in Functions.GetTuningTypes().Cast<byte>())
                SetMod(mod, -1);

            var mods = Functions.Deserialize<List<VehicleMod>>(VehicleDB.ModsJSON);
            foreach (var mod in mods)
                SetMod(mod.Type, mod.Id);

            if (VehicleDB.WheelType != 0 || VehicleDB.WheelVariation != 0)
            {
                WheelType = VehicleDB.WheelType;
                SetMod(23, VehicleDB.WheelVariation);
                SetMod(24, VehicleDB.WheelVariation);
            }
            WheelColor = VehicleDB.WheelColor;
            WindowTint = VehicleDB.WindowTint;
            NeonColor = new Color(VehicleDB.NeonColorR, VehicleDB.NeonColorG, VehicleDB.NeonColorB);
            SetSharedDataEx("Mods", Functions.Serialize(new
            {
                VehicleDB.NeonLeft,
                VehicleDB.NeonRight,
                VehicleDB.NeonFront,
                VehicleDB.NeonBack,
                VehicleDB.HeadlightColor,
                VehicleDB.LightsMultiplier,
                VehicleDB.TireSmokeColorR,
                VehicleDB.TireSmokeColorG,
                VehicleDB.TireSmokeColorB,
            }));

            Livery = VehicleDB.Livery;
            PearlescentColor = 0;
            CustomPrimaryColor = new Color(VehicleDB.Color1R, VehicleDB.Color1G, VehicleDB.Color1B);
            CustomSecondaryColor = new Color(VehicleDB.Color2R, VehicleDB.Color2G, VehicleDB.Color2B);

            var extras = Functions.Deserialize<List<bool>>(VehicleDB.ExtrasJSON);
            var index = 1;
            foreach (var extra in extras)
            {
                SetExtra(index, extra);
                index++;
            }
        });
    }

    public void SetFuel(int fuel)
    {
        if (!HasFuelTank)
            return;

        VehicleDB.SetFuel(fuel);
        SetSharedDataEx(Constants.VEHICLE_META_DATA_FUEL, FuelHUD);
    }

    public bool IsWindowOpened(int window)
    {
        return OpenedWindows[window];
    }

    public void SetWindowOpened(int window, bool state)
    {
        OpenedWindows[window] = state;
        SetSharedDataEx("Windows", Functions.Serialize(OpenedWindows));
    }

    public void SetSharedDataEx(string key, object value)
    {
        Functions.RunOnMainThread(() =>
        {
            SetSharedData(key, value);
        });
    }

    public void ResetSharedDataEx(string key)
    {
        Functions.RunOnMainThread(() =>
        {
            ResetSharedData(key);
        });
    }

    public bool GetEngineStatus()
    {
        return Functions.RunOnMainThread(() => EngineStatus);
    }

    public void SetEngineStatus(bool status)
    {
        Functions.RunOnMainThread(() =>
        {
            EngineStatus = status;
        });
    }

    public Vector3 GetPosition()
    {
        return Functions.RunOnMainThread(() => Position);
    }

    public void SetPosition(Vector3 position)
    {
        Functions.RunOnMainThread(() =>
        {
            Position = position;
        });
    }

    public uint GetDimension()
    {
        return Functions.RunOnMainThread(() => Dimension);
    }

    public void SetDimension(uint dimension)
    {
        Functions.RunOnMainThread(() =>
        {
            Dimension = dimension;
        });
    }

    public VehicleClass GetClass()
    {
        return Functions.RunOnMainThread(() => (VehicleClass)Class);
    }

    public float GetEngineHealth()
    {
        return Functions.RunOnMainThread(() => NAPI.Vehicle.GetVehicleEngineHealth(this));
    }

    public float GetBodyHealth()
    {
        return Functions.RunOnMainThread(() => NAPI.Vehicle.GetVehicleBodyHealth(this));
    }

    public void SetLocked(bool locked)
    {
        Functions.RunOnMainThread(() =>
        {
            Locked = locked;
        });
    }

    public bool GetLocked()
    {
        return Functions.RunOnMainThread(() => Locked);
    }

    public T GetSharedDataEx<T>(string key)
    {
        return Functions.RunOnMainThread(() => GetSharedData<T>(key));
    }

    public void Detach()
    {
        ResetSharedDataEx(Constants.VEHICLE_META_DATA_ATTACHED);
    }

    public void AttachToVehicle(MyVehicle target, int bone, Vector3 position, Vector3 rotation)
    {
        SetSharedDataEx(Constants.VEHICLE_META_DATA_ATTACHED, Functions.Serialize(new
        {
            target.Id,
            bone,
            Position = new { position.X, position.Y, position.Z },
            Rotation = new { rotation.X, rotation.Y, rotation.Z },
        }));
    }

    public string GetNumberPlate()
    {
        return Functions.RunOnMainThread(() => NumberPlate);
    }
}