using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TrevizaniRoleplay.Server.Extensions;
using TrevizaniRoleplay.Server.Models;

namespace TrevizaniRoleplay.Server.Factories;

public class MyPlayer(NetHandle netHandle) : Player(netHandle)
{
    public User User { get; set; } = default!;

    public Character Character { get; set; } = default!;

    public Personalization Personalization { get; set; } = new();

    public List<Invite> Invites { get; set; } = [];

    public string ICName => Masked ? $"Mascarado {Character.Mask}" : (!string.IsNullOrWhiteSpace(TemporaryName) ? TemporaryName : Character.Name);

    public bool OnDuty { get; set; }

    private List<string> IPLs { get; set; } = [];

    public bool Cuffed { get; set; }

    public bool OnAdminDuty { get; set; }

    public List<Wound> Wounds { get; set; } = [];

    public Vector3 ICPosition
    {
        get
        {
            if (GetDimension() == 0)
                return GetPosition();

            var prop = Global.Properties.FirstOrDefault(x => x.Number == GetDimension());
            return prop is null ? GetPosition() : prop.GetEntrancePosition();
        }
    }

    public Vector3? SPECPosition { get; set; }
    public uint SPECDimension { get; set; }
    public int? SPECId { get; set; }

    public string StaffColor => User.Staff switch
    {
        UserStaff.Tester => "#955D41",
        UserStaff.GameAdmin => "#5B737B",
        UserStaff.LeadAdmin => "#009900",
        UserStaff.HeadAdmin => "#1D84BD",
        UserStaff.Management => "#CA4D4D",
        UserStaff.Founder => "#F1C40F",
        _ => string.Empty,
    };

    public bool Wounded => Wounds.Count > 0 || GetHealth() != MaxHealth || Character.Wound != CharacterWound.None;

    public List<Property> Properties => [.. Global.Properties.Where(x => x.CharacterId == Character.Id)];

    public bool Masked { get; set; }

    public List<Spot> CollectSpots { get; set; } = [];

    public Spot? CollectingSpot { get; set; }

    public bool VehicleAnimation { get; set; }

    public Faction? Faction => Global.Factions.FirstOrDefault(x => x.Id == Character.FactionId);

    public FactionRank? FactionRank => Global.FactionsRanks.FirstOrDefault(x => x.Id == Character.FactionRankId);

    public string RealIp => Functions.RunOnMainThread(() => Address.Replace("::ffff:", string.Empty));

    public string RealSocialClubName => Functions.RunOnMainThread(() => SocialClubName);

    public List<CharacterItem> Items { get; set; } = [];

    public CancellationTokenSource? CancellationTokenSourceSetarFerido { get; set; }

    public CharacterItem? DropItem { get; set; }

    public int DropItemQuantity { get; set; }

    public InventoryShowType InventoryShowType { get; set; }

    public Guid? InventoryRightTargetId { get; set; }

    public int Money
    {
        get
        {
            return Items.FirstOrDefault(x => x.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID))?.Quantity ?? 0;
        }
    }

    public CancellationTokenSource? CancellationTokenSourceAceitarHospital { get; set; }

    public CancellationTokenSource? CancellationTokenSourceAcao { get; set; }

    public System.Timers.Timer? Timer { get; set; }

    public int SessionId { get; set; } = -1;

    public WalkieTalkieItem WalkieTalkieItem { get; set; } = new();

    public EmergencyCall? EmergencyCall { get; set; }

    public EmergencyCall? VehicleEmergencyCall { get; set; }

    public CellphoneItem CellphoneItem { get; set; } = new();

    public CancellationTokenSource? CancellationTokenSourceDamaged { get; set; }

    public CancellationTokenSource? CancellationTokenSourceTextAction { get; set; }

    public CancellationTokenSource? CancellationTokenSourceCellphone { get; set; }

    public PhoneCall PhoneCall { get; set; } = new();

    public CharacterJob WaitingServiceType { get; set; }

    public Session? LoginSession { get; set; }

    public Session? FactionDutySession { get; set; }

    public Session? AdminDutySession { get; set; }

    public List<StaffFlag> StaffFlags { get; set; } = [];

    public List<FactionFlag> FactionFlags { get; set; } = [];

    public bool IsFactionLeader => Faction is not null && Faction.CharacterId == Character.Id;

    public System.Timers.Timer? DrugTimer { get; set; }

    public int ExtraPayment { get; set; }

    public string? DropBarrierModel { get; set; }

    public PropertyFurniture? DropPropertyFurniture { get; set; }

    public Spot? RadarSpot { get; set; }

    public List<Company> Companies => [.. Global.Companies.Where(x => x.CharacterId == Character.Id || x.Characters!.Any(y => y.CharacterId == Character.Id))];

    public VehicleTuning? VehicleTuning { get; set; }

    public AreaNameType AreaNameType { get; set; }
    public string AreaNameAuxiliar { get; set; } = string.Empty;

    public int? CurrentAnimFlag { get; set; }

    public bool NoClip { get; set; }

    public int MaxOutfit => User.GetCurrentPremium() switch
    {
        UserPremium.Gold => 30,
        UserPremium.Silver => 20,
        UserPremium.Bronze => 15,
        _ => 10,
    } + User.ExtraOutfitSlots;

    public List<(int, string)> TemporaryOutfits { get; set; } = [];

    public bool ToggleChatBackgroundColor { get; set; }

    public string TargetFactionDepartment { get; set; } = "ALL";

    public bool CanTalkInTransmission { get; set; }

    public bool FollowingTransmission { get; set; }

    public bool AutoLow { get; set; }

    public int? LastPMSessionId { get; set; }

    public bool UsingOutfitsOnDuty => OnDuty && Faction?.HasDuty == true;

    public string? CreatingOutfitName { get; set; }

    public uint ExclusiveDimension => Convert.ToUInt32(SessionId + 1_000_000);

    public Dealership? TestDriveDealership { get; set; }

    public MyVehicle? HotwireVehicle { get; set; }

    public bool HasSpikeStrip { get; set; }

    public bool PropertyNoClip { get; set; }

    public bool PhotoMode { get; set; }

    public bool Fishing { get; set; }

    /// <summary>
    /// Player who is carrying this player
    /// </summary>
    public uint? PlayerCarrying { get; set; }

    public string? AttachedWeapon { get; set; }

    public string? DropAdminObjectModel { get; set; }
    public Guid? DropAdminObjectId { get; set; }

    private bool seatBelt;
    public bool SeatBelt
    {
        get
        {
            return seatBelt;
        }
        set
        {
            Emit("ToggleSeatbelt", !value);
            seatBelt = value;
        }
    }

    public List<PhoneContact> Contacts { get; set; } = [];

    public AudioSpot? CellphoneAudioSpot { get; set; }

    public bool Debug { get; set; }

    public string TemporaryName { get; set; } = string.Empty;

    public Vector3? AnimationPosition { get; set; }

    public bool StaffChatToggle { get; set; }

    public bool StaffToggle { get; set; }

    public bool UsingHair { get; set; } = true;

    public (uint, int)? VehicleDeath { get; set; }

    public bool Anametag { get; set; }

    public bool ValidPed => GetModel() == (uint)PedHash.FreemodeMale01 || GetModel() == (uint)PedHash.FreemodeFemale01;

    public PedHash CorrectPed => Character.Sex == CharacterSex.Man ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01;

    public bool FactionWalkieTalkieToggle { get; set; }

    public int MaxHealth { get; } = Constants.MAX_HEALTH;

    public bool Visible
    {
        get
        {
            return GetSharedDataEx<bool>("Visible");
        }
        set
        {
            Functions.RunOnMainThread(() =>
            {
                Transparency = value ? 255 : 0;
            });
            SetSharedDataEx("Visible", value);
        }
    }

    public bool Invincible
    {
        get
        {
            return GetSharedDataEx<bool>("Invincible");
        }
        set
        {
            SetSharedDataEx("Invincible", value);
        }
    }

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

    public DateTime? AFKSince { get; set; }

    public (string, bool)? CurrentPhoneChat { get; set; }

    public Guid? RobberingPropertyId { get; set; }

    public ushort? DismantlingVehicleId { get; set; }

    public bool CellphoneSpeakers { get; set; }

    /// <summary>
    /// Vehicles that player has access to
    /// </summary>
    public List<Guid> VehiclesAccess { get; set; } = [];

    /// <summary>
    /// Properties that player has access to
    /// </summary>
    public List<Guid> PropertiesAccess { get; set; } = [];

    /// <summary>
    /// Player will drop items on death
    /// </summary>
    public bool DropItemsOnDeath => !(OnDuty && Faction?.Type == FactionType.Police);

    /// <summary>
    /// Weapons that player has equipped by faction
    /// </summary>
    public List<uint> FactionEquippedWeapons { get; set; } = [];

    public string GetCellphoneContactName(uint number)
    {
        var contact = Contacts.FirstOrDefault(x => x.Number == number);
        return contact is null ? number.ToString() : contact.Name;
    }

    public Task<string> GiveMoney(int quantity)
    {
        var characterItem = new CharacterItem();
        characterItem.Create(new Guid(Constants.MONEY_ITEM_TEMPLATE_ID), 0, quantity, null);
        return GiveItem(characterItem);
    }

    public async Task<string> GiveItem(CharacterItem item) => await GiveItem([item]);

    public async Task<string> GiveItem(IEnumerable<CharacterItem> items)
    {
        if (Items.Sum(x => x.Quantity * x.GetWeight()) +
            items.Sum(x => x.Quantity * x.GetWeight())
            > Constants.MAX_INVENTORY_WEIGHT)
            return $"Não é possível prosseguir pois os novos itens ultrapassarão o peso máximo do inventário ({Constants.MAX_INVENTORY_WEIGHT}).";

        if (Items.Count + items.Count(x => !x.GetIsStack() || !Items.Any(y => y.ItemTemplateId == x.ItemTemplateId))
            > Constants.MAX_INVENTORY_SLOTS)
            return $"Não é possível prosseguir pois os novos itens ultrapassarão a quantidade de slots do inventário ({Constants.MAX_INVENTORY_SLOTS}).";

        var context = Functions.GetDatabaseContext();
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
                continue;

            if (item.GetIsStack())
            {
                var currentItem = Items.FirstOrDefault(x => x.ItemTemplateId == item.ItemTemplateId);
                if (currentItem is not null)
                {
                    currentItem.SetQuantity(currentItem.Quantity + item.Quantity);
                    context.CharactersItems.Update(currentItem);

                    if (currentItem.InUse && GlobalFunctions.CheckIfIsAmmo(currentItem.GetCategory()))
                    {
                        var weapon = Items.FirstOrDefault(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
                            && Functions.GetAmmoItemTemplateIdByWeapon(x.GetItemType()) == currentItem.ItemTemplateId);
                        if (weapon is null)
                            continue;

                        SetWeaponAmmo((WeaponHash)weapon.GetItemType(), Convert.ToUInt16(currentItem.Quantity));
                    }
                    continue;
                }
            }

            var category = item.GetCategory();
            if (item.Subtype == 0)
            {
                if (category == ItemCategory.Cellphone)
                    item.SetSubtype(await Functions.GetNewCellphoneNumber());
            }

            if (string.IsNullOrWhiteSpace(item.Extra))
            {
                if (category == ItemCategory.Cellphone)
                    item.SetExtra(Functions.Serialize(new CellphoneItem()));
                else if (category == ItemCategory.WalkieTalkie)
                    item.SetExtra(Functions.Serialize(new WalkieTalkieItem()));
                else if (category == ItemCategory.Weapon)
                    item.SetExtra(Functions.Serialize(new WeaponItem
                    {
                        Id = await Functions.GetNewWeaponId(),
                    }));
                else if (category == ItemCategory.WeaponComponent)
                    item.SetExtra(Functions.Serialize(new WeaponComponentItem()));
            }

            item.SetCharacterId(Character.Id);

            if (item.Slot == 0)
                item.SetSlot(Convert.ToByte(Enumerable.Range(1, Constants.MAX_INVENTORY_SLOTS)
                    .FirstOrDefault(i => !Items.Any(x => x.Slot == i))));

            await context.CharactersItems.AddAsync(item);

            Items.Add(item);
        }

        await context.SaveChangesAsync();

        ShowInventory(update: true);

        if (items.Any(x => x.ItemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID)))
            UpdateMoneyHUD();

        return string.Empty;
    }

    private void SetIPLs(List<string> ipls)
    {
        foreach (var ipl in IPLs)
            Emit("Server:RemoveIpl", ipl);

        IPLs = ipls;
        foreach (var ipl in IPLs)
            Emit("Server:RequestIpl", ipl);
    }

    public void PlayAnimation(string dic, string name, int flag, bool freeze = false)
    {
        CurrentAnimFlag = flag;
        var flags = (AnimationFlags)CurrentAnimFlag;
        var allowPlayerControl = (flags & AnimationFlags.AllowPlayerControl) == AnimationFlags.AllowPlayerControl;
        if (freeze)
            allowPlayerControl = true;
        if (allowPlayerControl)
        {
            SetSharedDataEx("Animation", Functions.Serialize(new
            {
                Dictionary = dic,
                Name = name,
                Flag = flag,
            }));
        }
        else
        {
            Frozen = true;
            AnimationPosition ??= Position;
        }

        Emit("animation:Play", dic, name, flag, freeze, allowPlayerControl);
    }

    public void StopAnimationEx()
    {
        Functions.RunOnMainThread(() =>
        {
            Frozen = false;
            CurrentAnimFlag = null;
            Emit("animation:Clear");
            VehicleAnimation = false;
            if (AnimationPosition is not null)
                Position = AnimationPosition;
            AnimationPosition = null;
            ResetSharedDataEx("Animation");
        });
    }

    public void SetPosition(Vector3 position, uint dimension, bool spawn, List<string>? ipls = null)
    {
        Functions.RunOnMainThread(() =>
        {
            var property = Global.Properties.FirstOrDefault(x => x.Number == dimension);
            if (property is not null && User.FreezingTimePropertyEntrance > 0 && SPECPosition is null && !NoClip)
            {
                Visible = false;
                Frozen = true;
            }

            var carryingTarget = Global.SpawnedPlayers.FirstOrDefault(x => x.Id == PlayerCarrying);
            if (carryingTarget is not null)
            {
                carryingTarget.Detach();
                carryingTarget.StopAnimationEx();
                carryingTarget.SetPosition(position, dimension, spawn, ipls);
            }

            var changingDimension = Dimension != dimension;

            Dimension = dimension;
            if (spawn)
                NAPI.Player.SpawnPlayer(this, position);
            else
                Position = position;

            carryingTarget?.StartBeingCarried(this);

            foreach (var target in Global.SpawnedPlayers.Where(x => x.SPECId == SessionId))
                target.SetPosition(new(position.X, position.Y, position.Z + 5), Dimension, true);

            var address = string.Empty;
            var weather = Global.WeatherInfo.WeatherType;
            var hour = Convert.ToByte(DateTime.Now.Hour);
            if (ipls is not null)
            {
                SetIPLs(ipls);
            }
            else if (property is not null)
            {
                address = property.FormatedAddress;

                weather = property.Weather.HasValue ? (Weather)property.Weather : Weather.CLEAR;

                if (property.Time.HasValue)
                    hour = property.Time.Value;

                SetIPLs(Functions.GetIPLsByInterior(property.Interior));

                if (User.FreezingTimePropertyEntrance > 0 && SPECPosition is null && !NoClip)
                    _ = Task.Delay(TimeSpan.FromSeconds(User.FreezingTimePropertyEntrance)).ContinueWith((x) =>
                    {
                        Visible = true;
                        Frozen = false;
                    });
            }
            else
            {
                SetIPLs(Global.IPLs);
            }

            Emit("SetAddress", address);
            SyncWeather(weather);
            SetHour(hour);

            if (changingDimension || spawn)
            {
                foreach (var particle in Global.Particles)
                {
                    if (particle.Dimension == dimension)
                        particle.Setup(this);
                    else
                        particle.Remove(this);
                }
            }
        });
    }

    public async Task Unspectate()
    {
        Detach();
        await Task.Delay(1000);
        SetPosition(SPECPosition!, SPECDimension, true);
        await Task.Delay(1000);
        SPECPosition = null;
        SPECDimension = 0;
        SPECId = null;
        Visible = true;
        Invincible = Frozen = false;
        SetNametag();
        StopAnimationEx();
    }

    public void Heal(bool healOnlyPunches = false)
    {
        CancellationTokenSourceSetarFerido?.Cancel(false);
        CancellationTokenSourceSetarFerido = null;

        CancellationTokenSourceAceitarHospital?.Cancel(false);
        CancellationTokenSourceAceitarHospital = null;

        if (Character.Wound != CharacterWound.None)
        {
            SetPosition(GetPosition(), GetDimension(), true);
            StopAnimationEx();
        }

        Character.SetWound(CharacterWound.None);
        SetSharedDataEx(Constants.PLAYER_META_DATA_INJURED, 0);

        if (!OnAdminDuty)
            Invincible = false;

        if (healOnlyPunches)
        {
            SetHealth(25);
        }
        else
        {
            if (GetHealth() < MaxHealth)
                SetHealth(MaxHealth);

            ClearBloodDamage();
            StopBleeding();
            Wounds = [];
        }

        Emit("DeathPage:CloseServer");
    }

    public void GiveWeapon(CharacterItem item)
    {
        Functions.RunOnMainThread(() =>
        {
            var weapon = item.GetItemType();
            var ammoItemTemplateId = Functions.GetAmmoItemTemplateIdByWeapon(weapon);
            var ammoItem = Items.FirstOrDefault(x => x.InUse && x.ItemTemplateId == ammoItemTemplateId);

            GiveWeapon((WeaponHash)weapon, ammoItemTemplateId is null ? 1 : (ammoItem?.Quantity ?? 0));
            SetCurrentWeapon((uint)WeaponModel.Fist);

            var componentsItemsTemplates = Items.Where(x => x.InUse && Functions.GetComponentsItemsTemplatesByWeapon(weapon).Contains(x.ItemTemplateId)).ToList();
            componentsItemsTemplates.ForEach(x => AddWeaponComponent(weapon, x.GetItemType()));
            CheckAttachedWeapon((uint)WeaponModel.Fist);
        });
    }

    public void RemoveWeapon(CharacterItem item)
    {
        Functions.RunOnMainThread(() =>
        {
            var weaponType = item.GetItemType();
            RemoveWeaponEx((WeaponHash)weaponType);

            var ammoItem = Items.FirstOrDefault(x => x.ItemTemplateId == Functions.GetAmmoItemTemplateIdByWeapon(weaponType));
            ammoItem?.SetInUse(false);

            var componentsItemsTemplates = Items.Where(x => Functions.GetComponentsItemsTemplatesByWeapon(weaponType).Contains(x.ItemTemplateId)).ToList();
            componentsItemsTemplates.ForEach(x => x.SetInUse(false));
            CheckAttachedWeapon((uint)WeaponModel.Fist);
        });
    }

    public void SetNametag() => SetSharedDataEx(Constants.PLAYER_META_DATA_NAMETAG,
        Functions.Serialize(new
        {
            Show = SPECPosition is null && !NoClip,
            Name = OnAdminDuty ? User.Name : $"{ICName}{(Character.Age < 18 ? " (M)" : string.Empty)}",
            Color = OnAdminDuty ? StaffColor : "#FFFFFF",
            SessionId,
            UserName = User.DiscordUsername,
            PhotoMode,
        }));

    public async Task SpawnEx()
    {
        if (Global.Parameter.WhoCanLogin == WhoCanLogin.OnlyStaff
            && User.Staff < UserStaff.Tester)
        {
            SendNotification(NotificationType.Error, "Apenas staff pode logar no momento.");
            await ListCharacters(string.Empty, string.Empty);
            return;
        }

        if (Global.Parameter.WhoCanLogin == WhoCanLogin.OnlyStaffOrUsersWithPremiumPoints
            && User.Staff < UserStaff.Tester && User.PremiumPoints == 0)
        {
            SendNotification(NotificationType.Error, "Apenas staff ou usuários que possuam LS Points podem logar no momento.");
            await ListCharacters(string.Empty, string.Empty);
            return;
        }

        var context = Functions.GetDatabaseContext();
        var onlineCount = Global.SpawnedPlayers.Count();
        if (onlineCount > Global.Parameter.MaxCharactersOnline)
        {
            Global.Parameter.SetMaxCharactersOnline(onlineCount);
            context.Parameters.Update(Global.Parameter);
            await context.SaveChangesAsync();
            await Functions.SendServerMessage($"O novo recorde de jogadores online é: {Global.Parameter.MaxCharactersOnline}.", UserStaff.None, true);
        }

        LoginSession = new();
        LoginSession.Create(Character.Id, SessionType.Login, RealIp, RealSocialClubName);
        await context.Sessions.AddAsync(LoginSession);
        await context.SaveChangesAsync();

        SendMessage(MessageType.None, $"Olá {{{Constants.MAIN_COLOR}}}{User.Name}{{#FFFFFF}}, que bom te ver por aqui! Seu último login foi em {{{Constants.MAIN_COLOR}}}{Character.LastAccessDate}{{#FFFFFF}}.");
        SendMessage(MessageType.None, $"Seu ID é {{{Constants.MAIN_COLOR}}}{SessionId}{{#FFFFFF}}.");

        if (User.GetCurrentPremium() == UserPremium.None)
            User.SetPMToggle(false);

        SetNametag();
        ToggleGameControls(true);
        NametagsConfig();
        DlConfig();
        ConfigChat();
        ToggleAmbientSound();
        SetSharedDataEx(Constants.PLAYER_META_DATA_INJURED, 0);
        Invincible = false;
        Visible = true;

        if (Character.JailFinalDate.HasValue && Character.JailFinalDate > DateTime.Now)
            SetPosition(new(Global.Parameter.PrisonInsidePosX, Global.Parameter.PrisonInsidePosY, Global.Parameter.PrisonInsidePosZ), Global.Parameter.PrisonInsideDimension, true);
        else if (Character.JailFinalDate.HasValue)
            RemoveFromJail();
        else
            SetPosition(new(Character.PosX, Character.PosY, Character.PosZ), Character.Dimension, true);

        Character.SetLastAccessDate();
        Emit("Server:setArtificialLightsState", Global.Parameter.Blackout);
        Emit("UpdateWeaponRecoils", Global.Parameter.WeaponsInfosJSON);
        SetMovement();
        SetHealth(Character.Health);
        SetArmor(Character.Armor);

        FollowingTransmission = Global.TransmissionActive;

        if (Character.DriverLicenseValidDate.HasValue &&
            (Character.DriverLicenseValidDate ?? DateTime.MinValue).Date < DateTime.Now.Date)
            SendMessage(MessageType.Error, $"Sua licença de motorista está vencida.");

        var notificationsUnread = await context.Notifications.CountAsync(x => x.UserId == User.Id && !x.ReadDate.HasValue);
        if (notificationsUnread > 0)
        {
            var messageNotificationsUnread = notificationsUnread == 1 ?
                "notificação não lida"
                :
                "notificações não lidas";

            SendMessage(MessageType.Error, $"Você possui {notificationsUnread} {messageNotificationsUnread}. Use /notificacoes para visualizar.");
        }

        foreach (var x in Global.Spotlights)
            Emit("Spotlight:Add", x.Id, x.Position, x.Direction,
                x.Distance, x.Brightness, x.Hardness, x.Radius, x.Falloff);

        Global.Doors.ForEach(x => x.Setup(this));
        Global.Peds.ForEach(x => x.Setup(this));

        try
        {
            // TODO: Provisory
            Global.AudioSpots.Where(x => x is not null).ToList().ForEach(x => x.Setup(this));
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }

        var cellphoneItem = Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.Cellphone && x.InUse);
        if (cellphoneItem is not null)
        {
            Character.SetCellphone(cellphoneItem.Subtype);
            CellphoneItem = Functions.Deserialize<CellphoneItem>(cellphoneItem.Extra!);
            await ConfigureCellphone();
        }

        var walkieTalkieItem = Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.WalkieTalkie && x.InUse);
        if (walkieTalkieItem is not null)
            WalkieTalkieItem = Functions.Deserialize<WalkieTalkieItem>(walkieTalkieItem.Extra!);

        SetupDrugTimer(Character.DrugEndDate.HasValue);

        foreach (var weapon in Items.Where(x => x.GetCategory() == ItemCategory.Weapon && x.InUse))
            GiveWeapon(weapon);

        if (Character.Bleeding)
            StartBleeding();

        if (Character.Wound != CharacterWound.None)
            SetWound(true);

        if (User.Staff >= UserStaff.LeadAdmin
            && await context.Vehicles.AnyAsync(x => !string.IsNullOrWhiteSpace(x.NewPlate)))
            SendMessage(MessageType.Error, "Existem solicitações de alterações de placa. Use /alteracoesplaca.");

        if (!string.IsNullOrWhiteSpace(Global.Parameter.MOTD))
            SendMessage(MessageType.Error, Global.Parameter.MOTD);

        SetOwnSharedDataEx("Temperature", $"{Global.WeatherInfo.Main.Temp:N0}ºC");
        SetOwnSharedDataEx("WeatherType", Global.WeatherInfo.WeatherType.ToString().ToUpper());

        Timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
        Timer.Elapsed += async (s, e) =>
        {
            try
            {
                Functions.ConsoleLog($"Player Timer {Character.Name} Start");

                if (GetDimension() == 0)
                    SetHour(DateTime.Now.Hour);

                Character.AddConnectedTime();

                if (OnAdminDuty)
                    User.AddStaffDutyTime();

                if (Character.JailFinalDate.HasValue && Character.JailFinalDate <= DateTime.Now)
                    RemoveFromJail();

                if (Character.ConnectedTime % 60 == 0)
                    await Paycheck(false);

                if (Character.ConnectedTime % 1 == 0)
                    await Save();
            }
            catch (Exception ex)
            {
                ex.Source = Character.Id.ToString();
                Functions.GetException(ex);
            }
            finally
            {
                Functions.ConsoleLog($"Player Timer {Character.Name} End");
            }
        };
        Timer.Start();

        SelectCharacter();
    }

    public void SetWound(bool isSpawning)
    {
        if (Character.Wound >= CharacterWound.PK)
        {
            SendMessage(MessageType.Error, Resources.YouDiedAndLostYourMemory);

            if (Character.Wound == CharacterWound.CanHospitalCK)
            {
                SendMessage(MessageType.Error, "Digite /aceitarhospital para receber os cuidados dos médicos e ser levado ao hospital.");
                SendMessage(MessageType.Error, "Digite /aceitarck para aplicar CK no seu personagem. (ATENÇÃO: ESSA OPERAÇÃO É IRREVERSÍVEL!)");
            }
        }

        Cuffed = false;

        CancellationTokenSourceAceitarHospital?.Cancel();
        CancellationTokenSourceAceitarHospital = null;

        if (Character.Wound < CharacterWound.CanHospitalCK)
        {
            CancellationTokenSourceAceitarHospital = new();
            Task.Delay(TimeSpan.FromMinutes(5), CancellationTokenSourceAceitarHospital.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                if (Character.Wound < CharacterWound.PK)
                    SendMessage(MessageType.Error, Resources.YouDiedAndLostYourMemory);
                Character.SetWound(CharacterWound.CanHospitalCK);
                SetSharedDataEx(Constants.PLAYER_META_DATA_INJURED, (int)Character.Wound);
                SendMessage(MessageType.Error, "Digite /aceitarhospital para receber os cuidados dos médicos e ser levado ao hospital.");
                SendMessage(MessageType.Error, "Digite /aceitarck para aplicar CK no seu personagem. (ATENÇÃO: ESSA OPERAÇÃO É IRREVERSÍVEL!)");
                CancellationTokenSourceAceitarHospital = null;
            });
        }

        CancellationTokenSourceSetarFerido?.Cancel();
        CancellationTokenSourceSetarFerido = null;

        if (Character.Wound <= CharacterWound.SeriouslyInjuredInvincible)
        {
            Character.SetWound(CharacterWound.SeriouslyInjuredInvincible);

            CancellationTokenSourceSetarFerido = new();
            Task.Delay(TimeSpan.FromSeconds(5), CancellationTokenSourceSetarFerido.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                if (!isSpawning)
                    SetPosition(GetPosition(), GetDimension(), true);
                PlayAnimationDead();
                Character.SetWound(CharacterWound.SeriouslyInjured);
                CancellationTokenSourceSetarFerido = null;
            });
        }
        else
        {
            if (!isSpawning)
                SetPosition(GetPosition(), GetDimension(), true);
            PlayAnimationDead();
        }

        SendMessage(MessageType.Error, "Você foi gravemente ferido. Você deverá ser socorrido em 5 minutos ou sofrerá um PK.");
        SetSharedDataEx(Constants.PLAYER_META_DATA_INJURED, (int)Character.Wound);
        Emit("DeathPage:ShowServer", (int)Character.Wound);

        if (DropItemsOnDeath)
        {
            var droppableCategories = new List<ItemCategory> {
                ItemCategory.Weapon,
                ItemCategory.Drug,
                ItemCategory.WeaponComponent,
            };
            var itemsToRemove = Items.Where(x => droppableCategories.Contains(x.GetCategory())
                || GlobalFunctions.CheckIfIsAmmo(x.GetCategory()))
                .ToList();
            if (itemsToRemove.Count > 0)
            {
                SendMessage(MessageType.Title, "Se você morrer, perderá os itens abaixo! Tire uma screenshot caso precise para um refundo.");
                foreach (var itemToRemove in itemsToRemove)
                    SendMessage(MessageType.Title, $"{itemToRemove.Quantity}x {itemToRemove.GetName()}");
            }
        }
    }

    public void PlayAnimationDead()
    {
        Functions.RunOnMainThread(() =>
        {
            if (IsInVehicle)
                VehicleDeath = new(Vehicle.Id, VehicleSeat);

            if (VehicleDeath is not null)
            {
                var vehicle = Global.Vehicles.FirstOrDefault(x => x.Id == VehicleDeath.Value.Item1);
                if (vehicle is not null)
                {
                    SetIntoVehicleEx(vehicle, VehicleDeath.Value.Item2);
                    PlayAnimation("amb@code_human_in_car_idles@generic@ps@base", "base", (int)AnimationFlags.StopOnLastFrame, true);
                    return;
                }
            }

            PlayAnimation("dead", "dead_d", (int)AnimationFlags.StopOnLastFrame, true);
        });
    }

    public void ToggleGameControls(bool enabled)
    {
        Functions.RunOnMainThread(() =>
        {
            Frozen = !enabled;
            if (Vehicle is MyVehicle vehicle)
                vehicle.Frozen = !enabled;

            Emit("animation:SetFreeze", !enabled);
        });
    }

    public async Task Save()
    {
        if (Character.PersonalizationStep != CharacterPersonalizationStep.Ready)
            return;

        var context = Functions.GetDatabaseContext();
        var position = GetPosition();
        if (position.Z != 0)
        {
            Character.Update(GetModel(), position.X, position.Y, position.Z, GetHealth(), GetArmor(), GetDimension(),
                Functions.Serialize(Personalization), Functions.Serialize(Wounds), Functions.Serialize(FactionFlags));
            context.Characters.Update(Character);
            await context.SaveChangesAsync();
        }

        if (Items.Count != 0)
        {
            context.CharactersItems.UpdateRange(Items);
            await context.SaveChangesAsync();
        }

        User.SetLastAccessDate();
        context.Users.Update(User);
        await context.SaveChangesAsync();
    }

    public void SendNotification(NotificationType notificationType, string message)
    {
        Emit(Constants.CHAT_PAGE_NOTIFY, message, notificationType.ToString().ToLower());

        foreach (var target in Global.SpawnedPlayers.Where(x => x.SPECId == SessionId))
            target.SendMessage((MessageType)notificationType, $"[SPEC] {message}");
    }

    public void SendMessage(MessageType messageType, string message, string color = "#FFFFFF")
    {
        if (messageType == MessageType.Success)
            color = Constants.SUCCESS_COLOR;
        else if (messageType == MessageType.Error)
            color = Constants.ERROR_COLOR;
        else if (messageType == MessageType.Title)
            color = "#B0B0B0";

        var matches = new Regex("{#.*?}", RegexOptions.None, TimeSpan.FromSeconds(5)).Matches(message).ToList();

        foreach (Match x in matches)
            message = message.Replace(x.Value, $"{(matches.IndexOf(x) != 0 ? "</span>" : string.Empty)}<span style='color:{x.Value.Replace("{", string.Empty).Replace("}", string.Empty)}'>");

        if (matches.Count > 0)
            message += "</span>";

        Emit(Constants.CHAT_PAGE_SEND_MESSAGE, message, color);

        foreach (var target in Global.SpawnedPlayers.Where(x => x.SPECId == SessionId))
            target.Emit(Constants.CHAT_PAGE_SEND_MESSAGE, $"[SPEC] {message}", color);
    }

    public MyPlayer? GetCharacterByIdOrName(string idOrName, bool canBeOwnPlayer = true, bool showMessage = true)
    {
        if (int.TryParse(idOrName, out int id))
        {
            var p = Global.SpawnedPlayers.FirstOrDefault(x => x.SessionId == id);
            if (p != null)
            {
                if (!canBeOwnPlayer && this == p)
                {
                    if (showMessage)
                        SendMessage(MessageType.Error, $"O jogador não pode ser você.");
                    return null;
                }

                return p;
            }
        }

        var ps = Global.SpawnedPlayers.Where(x => x.Character.Name.ToLower().Contains(idOrName.ToLower())).ToList();
        if (ps.Count == 1)
        {
            if (!canBeOwnPlayer && this == ps.FirstOrDefault())
            {
                if (showMessage)
                    SendMessage(MessageType.Error, $"O jogador não pode ser você.");
                return null;
            }

            return ps.FirstOrDefault();
        }

        if (showMessage)
        {
            if (ps.Count > 0)
            {
                SendMessage(MessageType.Error, $"Mais de um jogador foi encontrado com a pesquisa: {idOrName}");
                foreach (var pl in ps)
                    SendMessage(MessageType.None, $"{pl.Character.Name} ({pl.SessionId})");
            }
            else
            {
                SendMessage(MessageType.Error, $"Nenhum jogador foi encontrado com a pesquisa: {idOrName}");
            }
        }

        return null;
    }

    public void SendMessageToNearbyPlayers(string message, MessageCategory type, string? destination = null)
    {
        if (Character.Wound != CharacterWound.None
            && type != MessageCategory.NormalDo && type != MessageCategory.Ado
            && type != MessageCategory.LowDo && type != MessageCategory.ShoutDo
            && type != MessageCategory.NormalMe && type != MessageCategory.Ame
            && type != MessageCategory.LowMe && type != MessageCategory.ShoutMe
            && type != MessageCategory.NormalIC
            && type != MessageCategory.OOC)
        {
            SendMessage(MessageType.Error, Resources.YouCanNotExecuteThisCommandBecauseYouAreSeriouslyInjured);
            return;
        }

        if (type != MessageCategory.OOC)
            message = Functions.CheckFinalDot(message);

        if (type == MessageCategory.Ame || type == MessageCategory.Ado)
        {
            SendMessage(MessageType.None, type == MessageCategory.Ame ? $"> {ICName} {message}" : $"> {message} (( {ICName} ))", Constants.ROLEPLAY_COLOR);

            CancellationTokenSourceTextAction?.Cancel();
            CancellationTokenSourceTextAction = new CancellationTokenSource();

            SetSharedDataEx(Constants.PLAYER_META_DATA_TEXT_ACTION, type == MessageCategory.Ame ? $"* {ICName} {message}" : $"* {message} (( {ICName} ))");

            Task.Delay(7_000, CancellationTokenSourceTextAction.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                ResetSharedDataEx(Constants.PLAYER_META_DATA_TEXT_ACTION);
                CancellationTokenSourceTextAction = null;
            });
            return;
        }

        if (AutoLow)
        {
            type = type switch
            {
                MessageCategory.NormalIC or MessageCategory.ClosedVehicleIC => MessageCategory.LowIC,
                MessageCategory.WalkieTalkie => MessageCategory.LowWalkieTalkie,
                MessageCategory.NormalMe => MessageCategory.LowMe,
                MessageCategory.NormalDo => MessageCategory.LowDo,
                _ => type,
            };
        }

        var range = type switch
        {
            MessageCategory.LowIC or MessageCategory.LowWalkieTalkie
                or MessageCategory.LowMe or MessageCategory.LowDo
                or MessageCategory.ClosedVehicleIC => 5,
            MessageCategory.NormalMe or MessageCategory.NormalDo or MessageCategory.DiceCoin
                or MessageCategory.OOC or MessageCategory.NormalIC or MessageCategory.Cellphone
                or MessageCategory.WalkieTalkie => 10,
            MessageCategory.ShoutMe or MessageCategory.ShoutDo => 20,
            MessageCategory.ShoutIC => 30,
            MessageCategory.Microphone => 50,
            MessageCategory.Megaphone => 60,
            _ => 0,
        };

        var excludePlayer = type == MessageCategory.WalkieTalkie;

        if (range == 0)
        {
            SendMessage(MessageType.Error, "Range da mensagem não configurado. Por favor, reporte o bug.");
            return;
        }

        var players = Global.SpawnedPlayers.Where(x => x.GetDimension() == Dimension).Select(x => new
        {
            Player = x,
            Distance = GetPosition().DistanceTo(x.GetPosition()),
        }).Where(x => x.Distance <= range).ToList();
        if (type == MessageCategory.ShoutIC)
        {
            if (Dimension == 0)
            {
                var property = Global.Properties
                    .Where(x => Position.DistanceTo(x.GetEntrancePosition()) <= Constants.RP_DISTANCE)
                    .MinBy(x => Position.DistanceTo(x.GetEntrancePosition()));
                if (property is not null)
                {
                    var pos = property.GetExitPosition();
                    players.AddRange([.. Global.SpawnedPlayers.Where(x => x.GetDimension() == property.Number).Select(x => new
                    {
                        Player = x,
                        Distance = pos.DistanceTo(x.GetPosition()),
                    }).Where(x => x.Distance <= range)]);
                }
            }
            else
            {
                var property = Global.Properties.FirstOrDefault(x => x.Number == Dimension);
                if (property is not null)
                {
                    var pos = property.GetEntrancePosition();
                    players.AddRange([.. Global.SpawnedPlayers.Select(x => new
                    {
                        Player = x,
                        Distance = pos.DistanceTo(x.GetPosition()),
                    }).Where(x => x.Distance <= range / 2)]);
                }
            }
        }

        players = [.. players.DistinctBy(x => x.Player)];

        destination ??= string.Empty;
        if (!string.IsNullOrWhiteSpace(destination))
        {
            if (type == MessageCategory.LowIC)
                destination = $" (baixo para {destination})";
            else
                destination = $" (para {destination})";
        }

        var distanceGap = range / 5;
        foreach (var target in players)
        {
            if (excludePlayer && this == target.Player)
                continue;

            var chatMessageColor = "#6E6E6E";

            if (target.Distance < distanceGap)
                chatMessageColor = "#E6E6E6";
            else if (target.Distance < distanceGap * 2)
                chatMessageColor = "#C8C8C8";
            else if (target.Distance < distanceGap * 3)
                chatMessageColor = "#AAAAAA";
            else if (target.Distance < distanceGap * 4)
                chatMessageColor = "#8C8C8C";

            switch (type)
            {
                case MessageCategory.NormalIC
                    or MessageCategory.ClosedVehicleIC:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz{destination}: {message}", chatMessageColor);
                    break;
                case MessageCategory.ShoutIC:
                    target.Player.SendMessage(MessageType.None, $"{ICName} grita{destination}: {message}", chatMessageColor);
                    break;
                case MessageCategory.NormalMe or MessageCategory.LowMe or MessageCategory.ShoutMe:
                    target.Player.SendMessage(MessageType.None, $"* {ICName} {message}", Constants.ROLEPLAY_COLOR);
                    break;
                case MessageCategory.NormalDo or MessageCategory.LowDo or MessageCategory.ShoutDo:
                    target.Player.SendMessage(MessageType.None, $"* {message} (( {ICName} ))", Constants.ROLEPLAY_COLOR);
                    break;
                case MessageCategory.OOC:
                    var cor = OnAdminDuty && !string.IsNullOrWhiteSpace(StaffColor) ? StaffColor : "#BABABA";
                    var nome = OnAdminDuty ? User.Name : ICName;
                    target.Player.SendMessage(MessageType.None, $"(( {{{cor}}}{nome} ({SessionId}){{#BABABA}}: {message} ))", "#BABABA");
                    break;
                case MessageCategory.LowIC:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz{(string.IsNullOrWhiteSpace(destination) ? " (baixo)" : destination)}: {message}", chatMessageColor);
                    break;
                case MessageCategory.Megaphone:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz (megafone): {message}", "#F2FF43");
                    break;
                case MessageCategory.Cellphone:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz (celular): {message}", chatMessageColor);
                    break;
                case MessageCategory.WalkieTalkie:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz (rádio): {message}", chatMessageColor);
                    break;
                case MessageCategory.LowWalkieTalkie:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz (rádio) (baixo): {message}", chatMessageColor);
                    break;
                case MessageCategory.DiceCoin:
                    target.Player.SendMessage(MessageType.None, message, Constants.ROLEPLAY_COLOR);
                    break;
                case MessageCategory.Microphone:
                    target.Player.SendMessage(MessageType.None, $"{ICName} diz (microfone): {message}", "#F2FF43");
                    break;
            }
        }
    }

    public void SendFactionMessage(string message)
    {
        foreach (var target in Global.SpawnedPlayers.Where(x => x.Character.FactionId == Character.FactionId && !x.User.FactionToggle))
            target.SendMessage(MessageType.None, message, $"#{Faction!.Color}");
    }

    public bool CheckAnimations(bool stopAnim = false, bool onlyInVehicle = false)
    {
        if (!stopAnim)
        {
            if (onlyInVehicle)
            {
                if (!IsInVehicle)
                {
                    SendMessage(MessageType.Error, "Você precisa estar dentro de um veículo.");
                    return false;
                }
            }
            else
            {
                if (IsInVehicle)
                {
                    SendMessage(MessageType.Error, "Você não pode utilizar comandos de animação em um veículo.");
                    return false;
                }
            }
        }

        if (Cuffed)
        {
            SendMessage(MessageType.Error, "Você não pode utilizar comandos de animação algemado.");
            return false;
        }

        if (Character.Wound != CharacterWound.None)
        {
            SendMessage(MessageType.Error, "Você não pode utilizar comandos de animação ferido.");
            return false;
        }

        if (CancellationTokenSourceAcao is not null)
        {
            SendMessage(MessageType.Error, "Você não pode utilizar comandos de animação enquanto aguarda uma ação.");
            return false;
        }

        if (stopAnim)
            StopAnimationEx();

        if (onlyInVehicle)
            VehicleAnimation = true;

        return true;
    }

    public async Task WriteLog(LogType type, string description, MyPlayer? target)
    {
        var context = Functions.GetDatabaseContext();
        var log = new Log();
        log.Create(type, description,
            Character?.Id, RealIp, RealSocialClubName, User?.Id,
            target?.Character?.Id, target?.RealIp ?? string.Empty, target?.RealSocialClubName ?? string.Empty);
        await context.Logs.AddAsync(log);
        await context.SaveChangesAsync();
    }

    public void ConfigChat() => Emit(Constants.CHAT_PAGE_CONFIGURE, User.TimeStampToggle, User.ChatFontType, User.ChatFontSize, User.ChatLines);

    public void ClearChat() => Emit(Constants.CHAT_PAGE_CLEAR_MESSAGES);

    public async Task Disconnect(string reason, bool real)
    {
        try
        {
            Timer?.Stop();
            Timer = null;

            var context = Functions.GetDatabaseContext();
            if (Character?.PersonalizationStep != CharacterPersonalizationStep.Ready
                || LoginSession is null)
            {
                if (Character is not null)
                {
                    Character.SetConnected(false);
                    context.Characters.Update(Character);
                    await context.SaveChangesAsync();
                }

                return;
            }

            LoginSession.End();
            context.Sessions.Update(LoginSession);
            await context.SaveChangesAsync();

            if (FactionDutySession is not null)
            {
                FactionDutySession.End();
                context.Sessions.Update(FactionDutySession);
                await context.SaveChangesAsync();
                LeaveDuty();
            }

            if (AdminDutySession is not null)
            {
                AdminDutySession.End();
                context.Sessions.Update(AdminDutySession);
                await context.SaveChangesAsync();
            }

            LoginSession = FactionDutySession = AdminDutySession = null;

            RadarSpot?.RemoveIdentifier();

            CancellationTokenSourceDamaged?.Cancel();
            CancellationTokenSourceDamaged = null;

            CancellationTokenSourceTextAction?.Cancel();
            CancellationTokenSourceTextAction = null;

            CancellationTokenSourceAcao?.Cancel();
            CancellationTokenSourceAcao = null;

            CancellationTokenSourceSetarFerido?.Cancel();
            CancellationTokenSourceSetarFerido = null;

            CancellationTokenSourceAceitarHospital?.Cancel();
            CancellationTokenSourceAceitarHospital = null;

            CollectSpots.ForEach(x => x.RemoveIdentifier());

            await EndCellphoneCall();

            foreach (var target in Global.SpawnedPlayers.Where(x => x.GetDimension() == GetDimension() && GetPosition().DistanceTo(x.GetPosition()) <= 20))
                target.SendMessage(MessageType.Error, $"(( {ICName} ({SessionId}) saiu do servidor. ))");

            foreach (var target in Global.SpawnedPlayers.Where(x => x.InventoryShowType == InventoryShowType.Inspect && x.InventoryRightTargetId == Character.Id))
                target.CloseInventory();

            if (SPECPosition is not null)
                await Unspectate();

            foreach (var target in Global.SpawnedPlayers.Where(x => x.SPECId == SessionId))
                await target.Unspectate();

            await WriteLog(LogType.Exit, reason, null);
            await Save();

            var company = Global.Companies.FirstOrDefault(x => x.EmployeeOnDuty == Character.Id);
            if (company is not null)
            {
                company.SetEmployeeOnDuty(null);
                company.ToggleOpen();
            }

            var sos = Global.HelpRequests.FirstOrDefault(x => x.CharacterName == Character.Name && x.Type == HelpRequestType.SOS);
            if (sos is not null)
            {
                sos.Answer(null);

                context.HelpRequests.Update(sos);
                await context.SaveChangesAsync();

                Global.HelpRequests.Remove(sos);
            }

            var report = Global.HelpRequests.FirstOrDefault(x => x.CharacterName == Character.Name && x.Type == HelpRequestType.Report);
            if (report is not null)
            {
                report.Answer(null);

                context.HelpRequests.Update(report);
                await context.SaveChangesAsync();

                Global.HelpRequests.Remove(report);
            }

            if (!real)
            {
                ResetSharedDataEx(Constants.PLAYER_META_DATA_NAMETAG);
                ResetSharedDataEx(Constants.PLAYER_META_DATA_ATTACHED_OBJECTS);
                ResetSharedDataEx(Constants.PLAYER_META_DATA_TEXT_ACTION);
                ClearDrugEffect();
                StopAnimationEx();
                Functions.RunOnMainThread(() =>
                {
                    RemoveAllWeapons();
                });
                Emit("ToggleAduty", false);
                if (NoClip || PropertyNoClip || PhotoMode)
                    StartNoClip(PhotoMode);

                Character = null!;
                Personalization = new();
                Invites = [];
                CellphoneItem = new();
                OnDuty = Masked = Cuffed = OnAdminDuty = VehicleAnimation =
                    CanTalkInTransmission = FollowingTransmission = AutoLow =
                    HasSpikeStrip = NoClip = PropertyNoClip = PhotoMode = Fishing = false;
                WaitingServiceType = CharacterJob.Unemployed;
                IPLs = [];
                Wounds = [];
                CollectSpots = [];
                CollectingSpot = new();
                Items = [];
                DropBarrierModel = null;
                DropPropertyFurniture = null;
                RadarSpot = null;
                WalkieTalkieItem = new();
                LastPMSessionId = null;
                PlayerCarrying = null;
                AttachedWeapon = null;
                AnimationPosition = null;
                UsingHair = true;
                AFKSince = null;
                CurrentPhoneChat = null;
                RobberingPropertyId = null;
                DismantlingVehicleId = null;
                VehiclesAccess = PropertiesAccess = [];

                EndHotwire();
                Emit("DeathPage:CloseServer");
                Global.AudioSpots.Where(x => x is not null).ToList().ForEach(x => x.Remove(this));
                Global.Particles.ForEach(x => x.Remove(this));
                Global.Peds.ForEach(x => x.Remove(this));
                SetPosition(GetPosition(), ExclusiveDimension, false);
                Visible = false;
            }
        }
        catch (Exception ex)
        {
            Functions.GetException(ex);
        }
    }

    public async Task ListCharacters(string reason, string warning)
    {
        await Disconnect(reason, false);

        Emit("Server:DisableHUD");

        if (!string.IsNullOrWhiteSpace(warning))
            SendNotification(NotificationType.Error, warning);

        if (User.AjailMinutes > 0)
        {
            Emit("ShowAjailInfo", $"Você está preso administrativamente. Tempo restante: {User.AjailMinutes} minuto(s)");
            Timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
            Timer.Elapsed += async (s, e) =>
            {
                try
                {
                    Functions.ConsoleLog($"Player Ajail Timer {User.DiscordUsername}");
                    var context = Functions.GetDatabaseContext();
                    User.SetAjailMinutes(User.AjailMinutes - 1);
                    context.Users.Update(User);
                    await context.SaveChangesAsync();
                    Emit("ShowAjailInfo", $"Você está preso administrativamente. Tempo restante: {User.AjailMinutes} minuto(s)");

                    if (User.AjailMinutes == 0)
                    {
                        Timer.Stop();
                        Timer = null;

                        await ListCharacters(string.Empty, string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    ex.Source = User.DiscordUsername;
                    Functions.GetException(ex);
                }
            };
            Timer.Start();
            return;
        }

        var context = Functions.GetDatabaseContext();
        Emit("Server:ListarPersonagens",
            Functions.Serialize((await context.Characters
                .Include(x => x.Faction)
                .Where(x => x.UserId == User.Id
                    && x.NameChangeStatus != CharacterNameChangeStatus.Done
                    && !x.DeletedDate.HasValue)
                .OrderBy(x => x.Name)
                .ToListAsync())
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.DeathReason,
                    Status = x.GetStatus(),
                    x.LastAccessDate,
                    ConnectedTime = Convert.ToInt32(x.ConnectedTime / 60),
                    Job = x.Job.GetDescription(),
                    Faction = x.Faction is not null ? x.Faction.Name : Resources.None,
                })),
                User.CharacterSlots);
    }

    public void SetCanDoDriveBy(int seat, bool? status = null)
    {
        if (seat >= Constants.VEHICLE_SEAT_PASSENGER_BACK_RIGHT) status = true;

        Emit("SetPlayerCanDoDriveBy", status.HasValue ?
            status
            :
            !((MyVehicle)Vehicle).HasWindows || ((MyVehicle)Vehicle).IsWindowOpened(seat));
    }

    public async Task<CharacterInfoResponse> Paycheck(bool preview)
    {
        var context = Functions.GetDatabaseContext();
        var propertyTaxPercentage = User.GetCurrentPremium() switch
        {
            UserPremium.Gold => 0.08,
            UserPremium.Silver => 0.10,
            UserPremium.Bronze => 0.13,
            _ => 0.15,
        };
        propertyTaxPercentage /= 100;

        var vehicleTaxPercentage = User.GetCurrentPremium() switch
        {
            UserPremium.Gold => 0.03,
            UserPremium.Silver => 0.05,
            UserPremium.Bronze => 0.07,
            _ => 0.10,
        };
        vehicleTaxPercentage /= 100;

        var valorImpostoPropriedade = 0;
        var valorImpostoVeiculo = 0;

        foreach (var x in Properties)
            valorImpostoPropriedade += Convert.ToInt32(x.Value * propertyTaxPercentage);

        var vehicles = await context.Vehicles
            .Where(x => x.CharacterId == Character.Id && !x.Sold)
            .ToListAsync();
        foreach (var x in vehicles)
            valorImpostoVeiculo += Convert.ToInt32((Functions.GetVehiclePrice(x.Model)?.Item1 ?? 0) * vehicleTaxPercentage);

        var jobSalary = 0;
        var factionSalary = 0;
        var unemploymentAssistance = 0;

        if (FactionRank?.Salary > 0)
            factionSalary = FactionRank.Salary;
        else if (Character.Job != CharacterJob.Unemployed)
            jobSalary = Global.Jobs.FirstOrDefault(x => x.CharacterJob == Character.Job)!.Salary;
        else
            unemploymentAssistance = Global.Parameter.UnemploymentAssistance;

        var initialHelp = Character.InitialHelpHours > 0 ? 5000 : 0;

        var salary = 0;
        salary += jobSalary;
        salary += factionSalary;
        salary += unemploymentAssistance;
        salary *= Global.Parameter.Paycheck;
        salary += Character.ExtraPayment;
        salary += initialHelp;
        salary -= valorImpostoPropriedade;
        salary -= valorImpostoVeiculo;

        var response = new CharacterInfoResponse();
        if (preview)
        {
            var items = new List<CharacterInfoResponse.Paycheck>();
            response.PaycheckMultiplier = Global.Parameter.Paycheck;
            response.PaycheckValue = salary;

            if (factionSalary > 0)
                items.Add(new()
                {
                    Description = $"Salário {Faction!.Name}",
                    Value = factionSalary,
                });

            if (jobSalary > 0)
                items.Add(new()
                {
                    Description = $"Salário {Character.Job.GetDescription()}",
                    Value = jobSalary,
                });

            if (unemploymentAssistance > 0)
                items.Add(new()
                {
                    Description = "Ajuda Desemprego",
                    Value = unemploymentAssistance,
                });

            if (Character.ExtraPayment > 0)
                items.Add(new()
                {
                    Description = $"Extra {Character.Job.GetDescription()}",
                    Value = Character.ExtraPayment,
                });

            if (initialHelp > 0)
                items.Add(new()
                {
                    Description = "Ajuda Inicial",
                    Value = initialHelp,
                });

            if (valorImpostoPropriedade > 0)
                items.Add(new()
                {
                    Description = "Imposto Propriedades",
                    Value = valorImpostoPropriedade,
                    Debit = true,
                });

            if (valorImpostoVeiculo > 0)
                items.Add(new()
                {
                    Description = "Imposto Veículos",
                    Value = valorImpostoVeiculo,
                    Debit = true,
                });

            response.PaycheckItems = items;
        }
        else
        {
            AddBank(salary);

            if (salary != 0)
            {
                SendMessage(MessageType.Title, $"Pagamento de {Character.Name} {(Global.Parameter.Paycheck > 1 ? $"(PAYCHECK {Global.Parameter.Paycheck}x)" : string.Empty)}");

                if (Character.FactionId.HasValue && FactionRank!.Salary > 0)
                    SendMessage(MessageType.None, $"Salário {Faction!.Name}: {{{Constants.SUCCESS_COLOR}}}+ ${FactionRank.Salary:N0}");

                if (jobSalary > 0)
                    SendMessage(MessageType.None, $"Salário {Character.Job.GetDescription()}: {{{Constants.SUCCESS_COLOR}}}+ ${jobSalary:N0}");

                if (unemploymentAssistance > 0)
                    SendMessage(MessageType.None, $"Ajuda Desemprego: {{{Constants.SUCCESS_COLOR}}}+ ${unemploymentAssistance:N0}");

                if (Character.ExtraPayment > 0)
                    SendMessage(MessageType.None, $"Extra {Character.Job.GetDescription()}: {{{Constants.SUCCESS_COLOR}}}+ ${Character.ExtraPayment:N0}");

                if (initialHelp > 0)
                {
                    SendMessage(MessageType.None, $"Ajuda Inicial: {{{Constants.SUCCESS_COLOR}}}+ ${initialHelp:N0}");
                    Character.ReduceInitialHelpHours();
                }

                if (valorImpostoPropriedade > 0)
                    SendMessage(MessageType.None, $"Imposto Propriedades: {{{Constants.ERROR_COLOR}}}- ${valorImpostoPropriedade:N0}");

                if (valorImpostoVeiculo > 0)
                    SendMessage(MessageType.None, $"Imposto Veículos: {{{Constants.ERROR_COLOR}}}- ${valorImpostoVeiculo:N0}");

                SendMessage(MessageType.None, $"Total: {(salary > 0 ? $"{{{Constants.SUCCESS_COLOR}}} +" : $"{{{Constants.ERROR_COLOR}}} -")} ${Math.Abs(salary):N0}");

                var financialTransaction = new FinancialTransaction();
                financialTransaction.Create(salary > 0 ? FinancialTransactionType.Deposit : FinancialTransactionType.Withdraw,
                    Character.Id, salary, "Pagamento");

                await context.FinancialTransactions.AddAsync(financialTransaction);
                await context.SaveChangesAsync();
            }

            Character.ResetExtraPayment();
        }

        return response;
    }

    public void SetPersonalization()
    {
        Functions.RunOnMainThread(() =>
        {
            if (!ValidPed)
                return;

            NAPI.Player.SetPlayerHeadBlend(this, new HeadBlend
            {
                ShapeFirst = Personalization.FaceFather,
                ShapeSecond = Personalization.FaceMother,
                ShapeThird = Personalization.FaceAncestry,
                SkinFirst = Personalization.SkinFather,
                SkinSecond = Personalization.SkinMother,
                SkinThird = Personalization.SkinAncestry,
                ShapeMix = Personalization.FaceMix,
                SkinMix = Personalization.SkinMix,
                ThirdMix = Personalization.AncestryMix,
            });

            for (byte i = 0; i < Personalization.Structure.Count; i++)
                SetFaceFeature(i, Personalization.Structure[i]);

            foreach (var opacityOverlay in Personalization.OpacityOverlays)
                SetHeadOverlay(opacityOverlay.Id, new()
                {
                    Opacity = opacityOverlay.Opacity,
                    Index = opacityOverlay.Value,
                });

            NAPI.Player.SetPlayerClothes(this, 2, UsingHair ? Personalization.Hair : 0, 0);
            NAPI.Player.SetPlayerHairColor(this, Personalization.HairColor1, Personalization.HairColor2);

            foreach (var colorOverlay in Personalization.ColorOverlays)
            {
                SetHeadOverlay(colorOverlay.Id, new()
                {
                    Opacity = colorOverlay.Opacity,
                    Index = colorOverlay.Value,
                    Color = colorOverlay.Color1,
                    SecondaryColor = colorOverlay.Color2,
                });
            }

            NAPI.Player.SetPlayerEyeColor(this, Personalization.Eyes);

            ClearDecorations();

            if (!string.IsNullOrWhiteSpace(Personalization.HairOverlay)
                && !string.IsNullOrWhiteSpace(Personalization.HairCollection))
                NAPI.Player.SetPlayerDecoration(this, new Decoration
                {
                    Collection = Functions.Hash(Personalization.HairCollection),
                    Overlay = Functions.Hash(Personalization.HairOverlay),
                });

            foreach (var tattoo in Personalization.Tattoos)
                NAPI.Player.SetPlayerDecoration(this, new Decoration
                {
                    Collection = Functions.Hash(tattoo.Collection),
                    Overlay = Functions.Hash(tattoo.Overlay),
                });
        });
    }

    private void SetCloth(byte component, Outfit.ClothAccessory cloth)
    {
        if (!cloth.Using)
        {
            short drawable = component switch
            {
                3 or 8 or 11 => Constants.CLOTH_3_8_11_DEFAULT_DRAWABLE,
                4 => Character.Sex == CharacterSex.Man ?
                    Constants.CLOTH_4_DEFAULT_DRAWABLE_MALE
                    :
                    Constants.CLOTH_4_DEFAULT_DRAWABLE_FEMALE,
                6 => Character.Sex == CharacterSex.Man ?
                    Constants.CLOTH_6_DEFAULT_DRAWABLE_MALE
                    :
                    Constants.CLOTH_6_DEFAULT_DRAWABLE_FEMALE,
                _ => 0,
            };
            NAPI.Player.SetPlayerClothes(this, component, drawable, 0);
            return;
        }

        NAPI.Player.SetPlayerClothes(this, component, cloth.Drawable, cloth.Texture);
    }

    private void SetAccessory(byte component, Outfit.ClothAccessory accessory)
    {
        if (accessory.Using && accessory.Drawable != -1)
            NAPI.Player.SetPlayerAccessory(this, component, accessory.Drawable, accessory.Texture);
        else
            NAPI.Player.ClearPlayerAccessory(this, component);
    }

    public Outfit GetOutfit()
    {
        var outfitsJson = UsingOutfitsOnDuty ? Character.OutfitsOnDutyJSON : Character.OutfitsJSON;

        return Functions.Deserialize<IEnumerable<Outfit>>(outfitsJson)
            .FirstOrDefault(x => x.Slot == (UsingOutfitsOnDuty ? Character.OutfitOnDuty : Character.Outfit))!;
    }

    public void SetOutfit()
    {
        Functions.RunOnMainThread(() =>
        {
            if (!ValidPed)
                return;

            var outfitsJson = UsingOutfitsOnDuty ? Character.OutfitsOnDutyJSON : Character.OutfitsJSON;
            if (string.IsNullOrWhiteSpace(outfitsJson))
                outfitsJson = "[]";

            var outfits = Functions.Deserialize<List<Outfit>>(outfitsJson);
            if (outfits.Count < MaxOutfit)
            {
                var cloth4Drawable = Character.Sex == CharacterSex.Man ?
                    Constants.CLOTH_4_DEFAULT_DRAWABLE_MALE
                    :
                    Constants.CLOTH_4_DEFAULT_DRAWABLE_FEMALE;

                var cloth6Drawable = Character.Sex == CharacterSex.Man ?
                    Constants.CLOTH_6_DEFAULT_DRAWABLE_MALE
                    :
                    Constants.CLOTH_6_DEFAULT_DRAWABLE_FEMALE;

                for (var i = outfits.Count + 1; i <= MaxOutfit; i++)
                    outfits.Add(new Outfit(i, cloth4Drawable, cloth6Drawable));

                if (UsingOutfitsOnDuty)
                    Character.SetOutfitOnDuty(1, Functions.Serialize(outfits));
                else
                    Character.SetOutfit(1, Functions.Serialize(outfits));
            }

            var outfit = GetOutfit();

            SetCloth(1, outfit.Cloth1);
            SetCloth(3, outfit.Cloth3);
            SetCloth(4, outfit.Cloth4);
            SetCloth(5, outfit.Cloth5);
            SetCloth(6, outfit.Cloth6);
            SetCloth(7, outfit.Cloth7);
            SetCloth(8, outfit.Cloth8);
            SetCloth(9, outfit.Cloth9);
            SetCloth(10, outfit.Cloth10);
            SetCloth(11, outfit.Cloth11);

            SetSharedDataEx("Accessories", Functions.Serialize(new
            {
                outfit.Accessory0,
                outfit.Accessory1,
                outfit.Accessory2,
                outfit.Accessory6,
                outfit.Accessory7,
            }));

            //SetAccessory(0, outfit.Accessory0);
            //SetAccessory(1, outfit.Accessory1);
            //SetAccessory(2, outfit.Accessory2);
            //SetAccessory(6, outfit.Accessory6);
            //SetAccessory(7, outfit.Accessory7);
        });
    }

    public void SetNametagDamaged()
    {
        CancellationTokenSourceDamaged?.Cancel();
        CancellationTokenSourceDamaged = new CancellationTokenSource();

        SetSharedDataEx(Constants.PLAYER_META_DATA_DAMAGED, true);

        Task.Delay(250, CancellationTokenSourceDamaged.Token).ContinueWith(t =>
        {
            if (t.IsCanceled)
                return;

            SetSharedDataEx(Constants.PLAYER_META_DATA_DAMAGED, false);
            CancellationTokenSourceDamaged = null;
        });
    }

    public bool SendWalkieTalkieMessage(int slot, string message, MessageCategory messageCategory)
    {
        if (IsActionsBlocked())
        {
            SendMessage(MessageType.Error, Resources.YouCanNotDoThisBecauseYouAreHandcuffedInjuredOrBeingCarried);
            return false;
        }

        var channel = slot switch
        {
            1 => WalkieTalkieItem.Channel1,
            2 => WalkieTalkieItem.Channel2,
            3 => WalkieTalkieItem.Channel3,
            4 => WalkieTalkieItem.Channel4,
            _ => WalkieTalkieItem.Channel5,
        };

        if (channel == 0)
        {
            SendMessage(MessageType.Error, $"Seu slot {slot} não possui um canal configurado.");
            return false;
        }

        var factionFrequency = Global.FactionsFrequencies.FirstOrDefault(x => x.Frequency == channel);
        if (factionFrequency is not null && FactionWalkieTalkieToggle)
        {
            SendMessage(MessageType.Error, "Você ocultou as mensagens de rádio da facção.");
            return false;
        }

        if (AutoLow)
            messageCategory = MessageCategory.LowIC;

        var channelName = factionFrequency?.Name ?? channel.ToString();
        message = Functions.CheckFinalDot(message);
        var formattedMessage = messageCategory switch
        {
            MessageCategory.NormalMe => $"{{{Constants.ROLEPLAY_COLOR}}}* {ICName} {message}",
            MessageCategory.NormalDo => $"{{{Constants.ROLEPLAY_COLOR}}}* {message} (( {ICName} ))",
            MessageCategory.NormalIC => $"{ICName} diz: {message}",
            MessageCategory.LowIC => $"{ICName} diz [baixo]: {message}",
            _ => string.Empty,
        };

        if (string.IsNullOrWhiteSpace(formattedMessage))
        {
            SendMessage(MessageType.Error, "Mensagem de rádio não foi configurada. Por favor, reporte o bug.");
            return false;
        }

        foreach (var player in Global.SpawnedPlayers
            .Where(x => x.WalkieTalkieItem.Channel1 == channel
                || x.WalkieTalkieItem.Channel2 == channel
                || x.WalkieTalkieItem.Channel3 == channel
                || x.WalkieTalkieItem.Channel4 == channel
                || x.WalkieTalkieItem.Channel5 == channel))
        {
            if (factionFrequency is not null && player.FactionWalkieTalkieToggle)
                continue;

            var slotPlayer = 1;
            if (player.WalkieTalkieItem.Channel2 == channel)
                slotPlayer = 2;
            else if (player.WalkieTalkieItem.Channel3 == channel)
                slotPlayer = 3;
            else if (player.WalkieTalkieItem.Channel4 == channel)
                slotPlayer = 4;
            else if (player.WalkieTalkieItem.Channel5 == channel)
                slotPlayer = 5;

            player.SendMessage(MessageType.None, $"[S: {slotPlayer} | CH: {channelName}] {formattedMessage}", slotPlayer == 1 ? "#FFEC8B" : "#A39759");
        }

        SendMessageToNearbyPlayers(message, MessageCategory.WalkieTalkie);
        return true;
    }

    public string GetInventoryItemsJson()
    {
        return Functions.Serialize(
            Items.Select(x => new
            {
                x.Id,
                Name = x.GetName(),
                Image = x.GetImage(),
                x.Quantity,
                x.Slot,
                Extra = x.GetExtra(),
                Weight = x.GetWeight(),
                IsUsable = x.GetIsUsable(),
                x.InUse,
                IsStack = x.GetIsStack(),
            }));
    }

    public void ShowInventory(InventoryShowType inventoryShowType = InventoryShowType.Default,
        string rightTitle = "Chão", string rightItems = "[]",
            bool update = false, Guid? rightTargetId = null)
    {
        if (update)
        {
            if (inventoryShowType != InventoryShowType)
                return;

            if (inventoryShowType == InventoryShowType.Vehicle
                || inventoryShowType == InventoryShowType.Property)
            {
                foreach (var target in Global.SpawnedPlayers.Where(x => x.InventoryRightTargetId == rightTargetId
                    && x.InventoryShowType == inventoryShowType))
                    ConfirmShowInventory(target);
            }
            else
            {
                if (inventoryShowType == InventoryShowType.Inspect)
                {
                    var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Character.Id == InventoryRightTargetId);
                    if (target is not null)
                    {
                        rightTitle = target.ICName;
                        rightItems = target.GetInventoryItemsJson();
                    }
                }

                ConfirmShowInventory(this);
            }
        }
        else
        {
            if (Global.SpawnedPlayers.Any(x => x.InventoryRightTargetId == Character.Id && x.InventoryShowType == InventoryShowType.Inspect))
            {
                SendNotification(NotificationType.Error, "Você está sendo revistado.");
                return;
            }

            if (Character.Wound != CharacterWound.None)
            {
                SendMessage(MessageType.Error, Resources.YouCanNotExecuteThisCommandBecauseYouAreSeriouslyInjured);
                return;
            }

            if (inventoryShowType == InventoryShowType.Default)
            {
                var vehicle = GetVehicle();
                var property = GetDimension() != 0
                    ? Global.Properties.FirstOrDefault(x => x.Number == GetDimension())
                    : null;

                if (vehicle is not null && vehicle.HasStorage && vehicle.CanAccess(this))
                {
                    vehicle.ShowInventory(this, false);
                    return;
                }
                else if (property is not null && property.RobberyValue == 0
                    && (property.CanAccess(this) || (Faction?.Type == FactionType.Police && OnDuty)))
                {
                    property.ShowInventory(this, false);
                    return;
                }
            }

            InventoryShowType = inventoryShowType;
            InventoryRightTargetId = rightTargetId;

            ConfirmShowInventory(this);
        }

        void ConfirmShowInventory(MyPlayer target)
        {
            if (inventoryShowType == InventoryShowType.Default)
            {
                var items = Global.Items.Where(x =>
                    GetPosition().DistanceTo(new(x.PosX, x.PosY, x.PosZ)) <= Constants.RP_DISTANCE
                        && x.Dimension == GetDimension())
                    .Take(Constants.MAX_INVENTORY_SLOTS)
                    .ToList();

                rightItems = Functions.Serialize(items.Select(x => new
                {
                    x.Id,
                    Image = x.GetImage(),
                    Name = x.GetName(),
                    x.Quantity,
                    Slot = items.IndexOf(x) + 1,
                    Extra = x.GetExtra(),
                    Weight = x.GetWeight(),
                }));
            }

            target.Emit("Inventory:Show",
                update,
                target.ICName,
                target.GetInventoryItemsJson(),
                Constants.MAX_INVENTORY_WEIGHT,
                Constants.MAX_INVENTORY_SLOTS,
                rightTitle,
                rightItems,
                inventoryShowType == InventoryShowType.Property ? Constants.MAX_PROPERTY_INVENTORY_SLOTS : Constants.MAX_INVENTORY_SLOTS);
        }
    }

    public void CloseInventory()
    {
        InventoryRightTargetId = null;
        InventoryShowType = InventoryShowType.Default;
        Emit("Inventory:CloseServer");
    }

    public Task RemoveMoney(int quantity) =>
        RemoveStackedItem(new Guid(Constants.MONEY_ITEM_TEMPLATE_ID), quantity);

    private void UpdateMoneyHUD() => Emit("HUDPage:UpdateMoney", Money);

    public async Task RemoveStackedItem(Guid itemTemplateId, int quantity)
    {
        if (quantity <= 0)
            return;

        var item = Items.FirstOrDefault(x => x.ItemTemplateId == itemTemplateId);
        if (item is null)
            return;

        var context = Functions.GetDatabaseContext();
        item.SetQuantity(item.Quantity - quantity);
        if (item.Quantity > 0)
        {
            context.CharactersItems.Update(item);
        }
        else
        {
            context.CharactersItems.Remove(item);
            Items.Remove(item);
        }
        await context.SaveChangesAsync();
        ShowInventory(update: true);

        if (itemTemplateId == new Guid(Constants.MONEY_ITEM_TEMPLATE_ID))
            UpdateMoneyHUD();

        if (item.InUse && GlobalFunctions.CheckIfIsAmmo(item.GetCategory()))
        {
            var weapon = Items.FirstOrDefault(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
                && Functions.GetAmmoItemTemplateIdByWeapon(x.GetItemType()) == item.ItemTemplateId);
            if (weapon is null)
                return;

            SetWeaponAmmo(weapon.GetItemType(), Convert.ToUInt16(item.Quantity));
        }
    }

    public async Task RemoveItem(CharacterItem item) => await RemoveItem([item]);

    public async Task RemoveItem(IEnumerable<CharacterItem> items)
    {
        var context = Functions.GetDatabaseContext();
        foreach (var item in items.ToList())
        {
            if (item.Quantity <= 0 || item.GetIsStack())
                continue;

            context.CharactersItems.Remove(item);
            Items.Remove(item);

            if (item.InUse)
            {
                var category = item.GetCategory();
                if (category == ItemCategory.Weapon)
                {
                    RemoveWeapon(item);
                }
                else if (category == ItemCategory.WalkieTalkie)
                {
                    WalkieTalkieItem = new();
                }
                else if (category == ItemCategory.Cellphone)
                {
                    Character.SetCellphone(0);
                    CellphoneItem = new CellphoneItem();
                    await ConfigureCellphone();
                }
                else if (category == ItemCategory.WeaponComponent)
                {
                    var weapons = Items.Where(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
                        && Functions.GetComponentsItemsTemplatesByWeapon(x.GetItemType()).Contains(item.ItemTemplateId))
                        .ToList();
                    foreach (var weapon in weapons)
                        RemoveWeaponComponent(weapon.GetItemType(), item.GetItemType());
                }
            }
        }

        await context.SaveChangesAsync();

        ShowInventory(update: true);
    }

    public async Task RemoveFromFaction()
    {
        if (Faction!.HasDuty)
        {
            SetArmor(0);
            await RemoveItem(Items.Where(x => !Functions.CanDropItem(Faction, x)));
        }

        var context = Functions.GetDatabaseContext();
        if (Faction.CharacterId == Character.Id)
        {
            Faction.ResetCharacterId();
            context.Factions.Update(Faction);
        }

        FactionFlags = [];
        Character.ResetFaction();
        OnDuty = false;
        if (FactionDutySession is not null)
        {
            FactionDutySession.End();
            context.Sessions.Update(FactionDutySession);
        }

        await context.SaveChangesAsync();
        SetOutfit();
    }

    public async Task ConfigureCellphone()
    {
        var context = Functions.GetDatabaseContext();
        Contacts = await context.PhonesContacts.Where(x => x.Origin == Character.Cellphone).ToListAsync();
        if (Contacts.Count == 0)
        {
            var phoneContactEmergency = new PhoneContact();
            phoneContactEmergency.Create(Character.Cellphone, Constants.EMERGENCY_NUMBER, Resources.EmergencyCenter);
            Contacts.Add(phoneContactEmergency);

            var phoneContactMechanic = new PhoneContact();
            phoneContactMechanic.Create(Character.Cellphone, Constants.MECHANIC_NUMBER, Resources.MechanicsCenter);
            Contacts.Add(phoneContactMechanic);

            var phoneContactTaxi = new PhoneContact();
            phoneContactTaxi.Create(Character.Cellphone, Constants.TAXI_NUMBER, Resources.TaxiDriversCenter);
            Contacts.Add(phoneContactTaxi);

            var phoneContactInsurance = new PhoneContact();
            phoneContactInsurance.Create(Character.Cellphone, Constants.INSURANCE_NUMBER, Resources.Insurance);
            Contacts.Add(phoneContactInsurance);

            await context.PhonesContacts.AddRangeAsync(Contacts);
            await context.SaveChangesAsync();
        }

        var groups = Global.PhonesGroups
            .Where(x => x.Users!.Any(y => y.Number == Character.Cellphone))
            .Select(x => new
            {
                x.Id,
                x.Name,
                JoinDate = x.Users!.FirstOrDefault(y => y.Number == Character.Cellphone)!.RegisterDate,
                Users = x.Users!.Select(y => new
                {
                    y.Number,
                    y.Permission,
                }),
            })
            .ToList();

        var calls = await context.PhonesCalls
            .Where(x => x.Origin == Character.Cellphone || x.Number == Character.Cellphone)
            .OrderByDescending(x => x.RegisterDate)
            .Select(x => new
            {
                x.Origin,
                x.Number,
                x.RegisterDate,
                x.Type,
            })
            .ToListAsync();

        Emit("ConfigureCellphone", Character.Cellphone, CellphoneItem.FlightMode,
            CellphoneItem.Wallpaper, CellphoneItem.RingtoneVolume, CellphoneItem.Password, CellphoneItem.Scale,
            Functions.Serialize(Contacts),
            await GetCellphoneLastMessagesJson(),
            Functions.Serialize(calls),
            Functions.Serialize(groups));
    }

    public async Task<string> GetCellphoneLastMessagesJson()
    {
        var groups = Global.PhonesGroups
            .Where(x => x.Users!.Any(y => y.Number == Character.Cellphone))
            .ToList();
        var context = Functions.GetDatabaseContext();
        var dbLastMessages = await context.Database.SqlQuery<PhoneLastMessage>($@"SET @cellphone = {Character.Cellphone};
        WITH LastMessages AS (
            SELECT 
                *,
                ROW_NUMBER() OVER (PARTITION BY 
                IF (PhoneGroupId is not null, PhoneGroupId, IF(Number = @cellphone, Origin, Number))
		        ORDER BY RegisterDate DESC) as Position
            FROM 
                PhonesMessages
	        WHERE Number = @cellphone 
		        OR Origin = @cellPhone 
                OR PhoneGroupId IN (SELECT PhoneGroupId FROM PhonesGroupsUsers WHERE Number = @cellPhone)
        )
        SELECT 
            lm.Id, 
            ifnull(IF(lm.Origin = @cellphone, lm.Number, lm.Origin), 0) `Number`, 
            lm.PhoneGroupId, 
            '' Title,
            lm.RegisterDate, 
            IF(lm.Type = 2, 'Localização', lm.Message) Message,
            IF(Origin = @cellphone OR pmr.Id is not null, 1, 0) `Read`
        FROM 
            LastMessages lm
        LEFT JOIN PhonesMessagesReads pmr ON lm.Id = pmr.PhoneMessageId AND pmr.Number = @cellphone
        WHERE 
            Position = 1;").ToListAsync();

        var lastMessages = new List<PhoneLastMessage>();
        foreach (var group in groups)
        {
            var dbLastMessage = dbLastMessages.FirstOrDefault(x => x.PhoneGroupId == group.Id);

            lastMessages.Add(new PhoneLastMessage
            {
                Id = dbLastMessage?.Id,
                Title = group.Name,
                Message = dbLastMessage?.Message ?? string.Empty,
                Number = 0,
                PhoneGroupId = group.Id,
                Read = dbLastMessage?.Read ?? true,
                RegisterDate = dbLastMessage?.RegisterDate ?? group.Users!.FirstOrDefault(x => x.Number == Character.Cellphone)!.RegisterDate,
            });
        }

        foreach (var dbLastMessage in dbLastMessages.Where(x => x.PhoneGroupId is null))
        {
            dbLastMessage.Title = Contacts.FirstOrDefault(x => x.Number == dbLastMessage.Number)?.Name
                ?? dbLastMessage.Number.ToString();
            lastMessages.Add(dbLastMessage);
        }

        lastMessages = [.. lastMessages.OrderByDescending(x => x.RegisterDate)];

        return Functions.Serialize(lastMessages);
    }

    public async Task UpdateCellphoneDatabase()
    {
        var cellphoneItem = Items.FirstOrDefault(x => x.GetCategory() == ItemCategory.Cellphone && x.InUse);
        if (cellphoneItem is null)
            return;

        cellphoneItem.SetExtra(Functions.Serialize(CellphoneItem));
        var context = Functions.GetDatabaseContext();
        context.CharactersItems.Update(cellphoneItem);
        await context.SaveChangesAsync();
    }

    public async Task EndCellphoneCall()
    {
        CancellationTokenSourceCellphone?.Cancel();
        CancellationTokenSourceCellphone = null;

        CellphoneAudioSpot?.RemoveAllClients();
        CellphoneAudioSpot = null;

        if (GetAttachedObjects().Any(x => x.Model == Constants.CELLPHONE_OBJECT))
            PlayPhoneBaseAnimation();

        Emit("PhonePage:EndCallServer");

        if (PhoneCall.Number == 0)
            return;

        PhoneCall.End();

        var context = Functions.GetDatabaseContext();
        await context.PhonesCalls.AddAsync(PhoneCall);
        await context.SaveChangesAsync();

        AddPhoneCall(PhoneCall);

        CellphoneSpeakers = false;

        var target = Global.SpawnedPlayers.FirstOrDefault(x => x != this && (x.PhoneCall.Number == Character.Cellphone || x.PhoneCall.Origin == Character.Cellphone));
        if (target is not null)
        {
            target.CancellationTokenSourceCellphone?.Cancel();
            target.CancellationTokenSourceCellphone = null;

            target.CellphoneAudioSpot?.RemoveAllClients();
            target.CellphoneAudioSpot = null;

            var otherCellphone = PhoneCall.Origin == target.Character.Cellphone ? PhoneCall.Number : PhoneCall.Origin;
            target.SendMessage(MessageType.None, $"[CELULAR] Sua ligação com {target.GetCellphoneContactName(otherCellphone)} terminou.", Constants.CELLPHONE_SECONDARY_COLOR);

            target.AddPhoneCall(PhoneCall);
            target.PhoneCall = new();

            if (target.GetAttachedObjects().Any(x => x.Model == Constants.CELLPHONE_OBJECT))
                target.PlayPhoneBaseAnimation();

            target.Emit("PhonePage:EndCallServer");

            target.CellphoneSpeakers = false;
        }

        PhoneCall = new();
    }

    public void SetupDrugTimer(bool drugEffect)
    {
        DrugTimer?.Stop();

        if (!drugEffect && !Character.ThresoldDeathEndDate.HasValue)
            return;

        var interval = drugEffect
            ?
            (Character.DrugEndDate! - DateTime.Now).Value.TotalMilliseconds
            :
            (Character.ThresoldDeathEndDate! - DateTime.Now).Value.TotalMilliseconds;

        if (interval < 0)
        {
            DrugTimer_Elapsed(null, null);
            return;
        }

        DrugTimer = new System.Timers.Timer(interval);
        DrugTimer.Elapsed += DrugTimer_Elapsed;
        DrugTimer.Start();

        if (drugEffect)
        {
            var drug = Global.Drugs.FirstOrDefault(x => x.ItemTemplateId == Character.DrugItemTemplateId);
            if (drug is not null)
                SetDrugEffect(drug.ShakeGameplayCamName, drug.ShakeGameplayCamIntensity, drug.TimecycModifier, drug.AnimpostFXName);
        }
    }

    public void SetDrugEffect(string shakeGameplayCamName, float shakeGameplayCamIntensity,
        string timecycModifier, string animpostFXName) => Emit("SetDrugEffect", shakeGameplayCamName, shakeGameplayCamIntensity,
                timecycModifier, animpostFXName);

    public void ClearDrugEffect() => Emit("ClearDrugEffect");

    public void SetMovement() => Emit("SetMovement", Functions.GetMovementByWalkStyle(Character.WalkStyle));

    public void DrugTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (Character.DrugItemTemplateId.HasValue)
        {
            Character.SetThresoldDeathEndDate();
            ClearDrugEffect();
            SetupDrugTimer(false);
            if (GetHealth() > MaxHealth)
                SetHealth(MaxHealth);
        }
        else
        {
            DrugTimer?.Stop();
            Character.ClearDrug();
        }
    }

    public void ShowConfirm(string title, string message, string clientEvent)
    {
        Emit(Constants.CHAT_PAGE_SHOW_CONFIRM, title, message, clientEvent);
    }

    public void SelectCharacter()
    {
        if (Character.PersonalizationStep != CharacterPersonalizationStep.Ready)
        {
            Invincible = true;
            Visible = false;
        }

        Emit("Server:SelecionarPersonagem",
            (int)Character.PersonalizationStep,
            (int)Character.Sex,
            Functions.Serialize(Personalization),
            UsingOutfitsOnDuty ? Character.OutfitsOnDutyJSON : Character.OutfitsJSON,
            Functions.Serialize(GetOutfit()),
            MaxOutfit);
        UpdateMoneyHUD();
        UpdateBankHUD();
    }

    public void EditOutfits(int type)
    {
        Invincible = true;
        Visible = false;
        Emit("EditOutfits", UsingOutfitsOnDuty ? Character.OutfitOnDuty : Character.Outfit,
             type == 3 ? 1 : MaxOutfit, UsingOutfitsOnDuty ? Character.OutfitsOnDutyJSON : Character.OutfitsJSON,
            type, (int)Character.Sex, (int)(Faction?.Type ?? 0),
            Functions.Serialize(Personalization), Functions.Serialize(GetOutfit()));
    }

    public bool CheckIfTargetIsCloseIC(MyPlayer target, float distance)
    {
        return GetPosition().DistanceTo(target.GetPosition()) <= distance && GetDimension() == target.GetDimension() && target.SPECPosition is null;
    }

    public List<AttachedObject> GetAttachedObjects()
    {
        var json = GetSharedDataEx<string>(Constants.PLAYER_META_DATA_ATTACHED_OBJECTS);
        if (string.IsNullOrWhiteSpace(json))
            json = "[]";
        return Functions.Deserialize<List<AttachedObject>>(json);
    }

    public void AttachObject(string model, int boneId, Vector3 position, Vector3 rotation)
    {
        var attachedObject = new AttachedObject
        {
            Model = model,
            BoneId = boneId,
            PosX = position.X,
            PosY = position.Y,
            PosZ = position.Z,
            RotX = rotation.X,
            RotY = rotation.Y,
            RotZ = rotation.Z,
        };

        var attachedObjects = GetAttachedObjects();
        var currentAttachedObject = attachedObjects.FirstOrDefault(x => x.Model == attachedObject.Model);
        if (currentAttachedObject is not null)
            attachedObjects[attachedObjects.IndexOf(currentAttachedObject)] = attachedObject;
        else
            attachedObjects.Add(attachedObject);
        SetSharedDataEx(Constants.PLAYER_META_DATA_ATTACHED_OBJECTS, Functions.Serialize(attachedObjects));
    }

    public void DetachObject(string model)
    {
        var attachedObjects = GetAttachedObjects();
        attachedObjects.RemoveAll(x => x.Model == model);
        SetSharedDataEx(Constants.PLAYER_META_DATA_ATTACHED_OBJECTS, Functions.Serialize(attachedObjects));
    }

    public void EndTestDrive()
    {
        if (TestDriveDealership is not null)
        {
            Invincible = Frozen = false;
            SetPosition(new(TestDriveDealership!.PosX, TestDriveDealership.PosY, TestDriveDealership.PosZ), 0, true);
            TestDriveDealership = null;
            SendMessage(MessageType.Error, "O test drive terminou.");
        }
    }

    public void SetWaypoint(float posX, float posY)
    {
        Emit("Server:SetWaypoint", posX, posY);
    }

    public void EndHotwire()
    {
        HotwireVehicle = null;
    }

    public void NametagsConfig() => Emit("nametags:Config", User.ShowNametagId, User.ShowOwnNametag);

    public void DlConfig() => Emit("dl:Config", User.VehicleTagToggle);

    public void StartNoClip(bool freecam) => Emit("NoClip:Start", freecam);

    public bool CheckCompanyPermission(Company company, CompanyFlag? companyFlag)
    {
        var companyCharacter = company.Characters!.FirstOrDefault(x => x.CharacterId == Character.Id);
        var hasAccess = companyCharacter is not null || company.CharacterId == Character.Id;
        var flags = company.CharacterId == Character.Id ?
            [.. Enum.GetValues<CompanyFlag>()]
            :
            Functions.Deserialize<List<CompanyFlag>>(companyCharacter?.FlagsJSON ?? "[]");
        return hasAccess && (companyFlag is null || flags.Contains(companyFlag.Value));
    }

    public bool IsActionsBlocked()
    {
        return Cuffed || Character.Wound != CharacterWound.None || PlayerCarrying.HasValue;
    }

    public void StopBeingCarried()
    {
        var target = Global.SpawnedPlayers.FirstOrDefault(x => x.Id == PlayerCarrying);
        target?.StopAnimationEx();
        Detach();
        StopAnimationEx();
        PlayerCarrying = null;

        if (Character.Wound != CharacterWound.None)
            PlayAnimationDead();
    }

    public void StartBeingCarried(MyPlayer target)
    {
        PlayerCarrying = target.Id;
        AttachToPlayer(target, 0, new(0.27f, 0.15f, 0.63f), new(0.5f, 0.5f, 180f));
        PlayAnimation("nm", "firemans_carry", (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), true);
        target.PlayAnimation("missfinale_c2mcs_1", "fin_c2_mcs_1_camman", (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), true);
    }

    public void RemoveFromJail()
    {
        Character.SetJailFinalDate(null);
        SetPosition(new(Global.Parameter.PrisonOutsidePosX, Global.Parameter.PrisonOutsidePosY, Global.Parameter.PrisonOutsidePosZ), Global.Parameter.PrisonOutsideDimension, true);
        SendMessage(MessageType.Success, "Você foi liberado da prisão.");
    }

    public void StartBleeding()
    {
        Character.SetBleeding(true);
    }

    public void StopBleeding()
    {
        Character.SetBleeding(false);
    }

    public void CheckAttachedWeapon(uint currentWeapon)
    {
        var weaponOnBody = Items.FirstOrDefault(x => x.InUse && x.GetCategory() == ItemCategory.Weapon
            && Global.WeaponsInfos.FirstOrDefault(y => y.Name == GlobalFunctions.GetWeaponName(x.GetItemType()))?.AttachToBody == true);
        if (weaponOnBody is null || weaponOnBody.GetItemType() == currentWeapon)
        {
            if (!string.IsNullOrWhiteSpace(AttachedWeapon))
            {
                DetachObject(AttachedWeapon);
                AttachedWeapon = null;
            }

            return;
        }

        var weaponBody = Functions.Deserialize<List<WeaponBody>>(Character.WeaponsBodyJSON)
            .FirstOrDefault(x => x.Name == GlobalFunctions.GetWeaponName(weaponOnBody.GetItemType()));
        weaponBody = Functions.CheckDefaultWeaponBody(weaponBody);

        AttachedWeapon = weaponOnBody.GetObjectName();

        if (!string.IsNullOrWhiteSpace(AttachedWeapon))
            AttachObject(AttachedWeapon, 0,
                new(weaponBody.PosX, weaponBody.PosY, weaponBody.PosZ),
                new(weaponBody.RotR, weaponBody.RotP, weaponBody.RotY));
    }

    public void LeaveDuty()
    {
        if (Faction?.HasDuty ?? false)
        {
            RemoveFactionEquippedWeapons();
            SetArmor(0);
        }
    }

    public void AddBank(int value)
    {
        Character.AddBank(value);
        UpdateBankHUD();
    }

    public void RemoveBank(int value)
    {
        Character.RemoveBank(value);
        UpdateBankHUD();
    }

    private void UpdateBankHUD() => Emit("HUDPage:UpdateBank", Character.Bank);

    public async Task<(bool, string)> CheckDiscordBooster()
    {
        if ((User.DiscordBoosterDate ?? DateTime.MinValue) > DateTime.Now)
            return new(false, $"Você só pode executar esse comando novamente em {User.DiscordBoosterDate}.");

        if (Global.DiscordClient is null)
            return new(false, "Bot não configurado. Por favor, reporte o bug.");

        var guild = Global.DiscordClient.GetGuild(Global.MainDiscordGuild);
        if (guild is null)
            return new(false, "Servidor não configurado. Por favor, reporte o bug.");

        var user = guild.GetUser(Convert.ToUInt64(User.DiscordId));
        if (user is null)
            return new(false, "Usuário não encontrado. Por favor, verifique se você está no Discord do servidor e tente novamente mais tarde.");

        if (user.PremiumSince is null)
            return new(false, "Você não está boostando o servidor.");

        var premiumPoints = 12;
        User.AddPremiumPoints(premiumPoints);
        User.SetDiscordBoosterDate(DateTime.Now.AddDays(30));
        await WriteLog(LogType.Premium, $"{premiumPoints} LS Points ganhos por ser booster no Discord do servidor. ({User.Name} {User.DiscordId})", null);
        return new(true, $"Você ganhou {premiumPoints} LS Points por ser booster no Discord do servidor. Execute novamente o comando em {User.DiscordBoosterDate}.");
    }

    public void Edit(int type)
    {
        Invincible = true;
        Visible = false;
        Emit("Character:Edit", (int)Character.Sex, Functions.Serialize(Personalization), Functions.Serialize(GetOutfit()), type);
    }

    public void ToggleAmbientSound() => Emit("ToggleAmbientSound", User.AmbientSoundToggle);

    public async Task UpdatePhoneLastMessages()
    {
        Emit("PhonePage:UpdateLastMessages", await GetCellphoneLastMessagesJson());
        await UpdatePhoneChatMessages();
    }

    public async Task UpdatePhoneChatMessages()
    {
        if (CurrentPhoneChat is null)
            return;

        var context = Functions.GetDatabaseContext();
        var chatMessages = context.PhonesMessages.Include(x => x.Reads).AsQueryable();

        if (CurrentPhoneChat.Value.Item2)
            chatMessages = chatMessages.Where(x => x.PhoneGroupId == CurrentPhoneChat.Value.Item1.ToGuid());
        else
            chatMessages = chatMessages.Where(x => !x.PhoneGroupId.HasValue
                && (x.Origin == Character.Cellphone
                 || x.Number == Character.Cellphone)
                && (x.Origin == Convert.ToInt32(CurrentPhoneChat.Value.Item1)
                 || x.Number == Convert.ToInt32(CurrentPhoneChat.Value.Item1)));

        var json = Functions.Serialize((await chatMessages
            .OrderByDescending(x => x.RegisterDate)
            .Take(100)
            .ToListAsync())
            .Select(x => new
            {
                x.Id,
                x.LocationX,
                x.LocationY,
                x.Type,
                Date = x.RegisterDate,
                Text = x.Message,
                Read = x.Origin == Character.Cellphone || x.Reads!.Any(y => y.Number == Character.Cellphone),
                From = x.Origin == Character.Cellphone ? "me" : Contacts.FirstOrDefault(y => y.Number == x.Origin)?.Name ?? x.Origin.ToString(),
            }));

        Emit("PhonePage:UpdateChatMessagesServer", json);
    }

    public void AddPhoneCall(PhoneCall call)
        => Emit("PhonePage:AddCallServer", Functions.Serialize(new
        {
            call.Origin,
            call.Number,
            call.RegisterDate,
            call.Type,
        }));

    public void SendCellphoneCallMessage(string origin, string message)
    {
        SendMessage(MessageType.None, $"{origin} diz (celular): {message}", Constants.CELLPHONE_MAIN_COLOR);

        if (CellphoneSpeakers)
        {
            var nearPlayers = Global.SpawnedPlayers
                .Where(x => x != this && CheckIfTargetIsCloseIC(x, Constants.RP_DISTANCE))
                .ToList();
            foreach (var player in nearPlayers)
                player.SendMessage(MessageType.None, $"{ICName} (viva-voz): {message}");
        }
    }

    public void PlayPhoneCallAnimation()
        => PlayAnimation("cellphone@", "cellphone_call_listen_base", (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), true);

    public void PlayPhoneBaseAnimation()
        => PlayAnimation("cellphone@", "cellphone_text_read_base", (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), true);

    public void PhoneAnswerCall(uint phone)
    {
        PlayPhoneCallAnimation();
        Emit("PhonePage:AnswerCallServer", GetCellphoneContactName(phone));
    }

    public async Task UpdatePhoneGroups()
    {
        Emit("PhonePage:UpdateGroupsServer", Functions.Serialize(Global.PhonesGroups
        .Where(x => x.Users!.Any(y => y.Number == Character.Cellphone))
        .Select(x => new
        {
            x.Id,
            x.Name,
            Users = x.Users!.Select(y => new
            {
                y.Number,
                y.Permission,
            }),
        })
        .ToList()));
        await UpdatePhoneLastMessages();
    }

    public void EndLoading() => Emit("WebView:EndLoading");

    public void SyncWeather(Weather weather) => Emit("SyncWeather", weather.ToString().ToUpper());

    public void RemoveFromVehicle() => Emit("RemoveFromVehicle");

    public void SetHour(int hour) => Emit("SyncTime", hour, 0, 0);

    public void AddWeaponComponent(uint weapon, uint component)
    {
        Emit("AddWeaponComponent", weapon, component);
    }

    public void RemoveWeaponComponent(uint weapon, uint component)
    {
        Emit("RemoveWeaponComponent", weapon, component);
    }

    public bool HasWeapon(uint weapon) =>
        Items.Any(x => x.InUse && x.GetCategory() == ItemCategory.Weapon && x.GetItemType() == weapon);

    public void ClearBloodDamage()
        => Emit("ClearBloodDamage");

    public void Detach()
    {
        ResetSharedDataEx("AttachToPlayer");
    }

    public void AttachToPlayer(MyPlayer target, int bone, Vector3 position, Vector3 rotation)
    {
        SetSharedDataEx("AttachToPlayer", Functions.Serialize(new
        {
            target.Id,
            bone,
            Position = new { position.X, position.Y, position.Z },
            Rotation = new { rotation.X, rotation.Y, rotation.Z },
        }));
    }

    public void SetModel(uint model)
    {
        Functions.RunOnMainThread(() =>
        {
            NAPI.Player.SetPlayerSkin(this, model);
        });
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

    public T GetSharedDataEx<T>(string key)
    {
        return Functions.RunOnMainThread(() => GetSharedData<T>(key));
    }

    public void Emit(string eventName, params object?[] args)
        => NAPI.ClientEventThreadSafe.TriggerClientEvent(this, eventName, args);

    public void SetHealth(int health)
    {
        Functions.RunOnMainThread(() =>
        {
            Health = health;
        });
    }

    public int GetHealth()
    {
        return Functions.RunOnMainThread(() => Health);
    }

    public void SetArmor(int armor)
    {
        Functions.RunOnMainThread(() =>
        {
            Armor = armor;
        });
    }

    public int GetArmor()
    {
        return Functions.RunOnMainThread(() => Armor);
    }

    public void SetOwnSharedDataEx(string key, object value)
    {
        Functions.RunOnMainThread(() =>
        {
            SetOwnSharedData(key, value);
        });
    }

    public uint GetModel()
    {
        return Functions.RunOnMainThread(() => Model);
    }

    public Vector3 GetPosition()
    {
        return Functions.RunOnMainThread(() => Position);
    }

    public uint GetDimension()
    {
        return Functions.RunOnMainThread(() => Dimension);
    }

    public void SetWeaponAmmo(uint weapon, int ammo)
    {
        Functions.RunOnMainThread(() =>
        {
            SetWeaponAmmo((WeaponHash)weapon, ammo);
        });
    }

    public int GetWeaponAmmo(uint weapon)
    {
        return Functions.RunOnMainThread(() => GetWeaponAmmo((WeaponHash)weapon));
    }

    public void SetCurrentWeapon(uint weapon)
    {
        Functions.RunOnMainThread(() =>
        {
            NAPI.Player.SetPlayerCurrentWeapon(this, weapon);
        });
    }

    public Vector3 GetRotation()
    {
        return Functions.RunOnMainThread(() => Rotation);
    }

    public void SetRotation(Vector3 rotation)
    {
        Functions.RunOnMainThread(() =>
        {
            Rotation = rotation;
        });
    }

    public void SetIntoVehicleEx(NetHandle vehicle, int seat)
    {
        Functions.RunOnMainThread(() =>
        {
            SetIntoVehicle(vehicle, seat);
        });
    }

    public void KickEx(string message)
    {
        Functions.RunOnMainThread(() =>
        {
            Kick(message);
        });
    }

    public MyVehicle? GetVehicle()
    {
        return Functions.RunOnMainThread(() => Vehicle as MyVehicle);
    }

    public void RemoveWeaponEx(WeaponHash weapon)
    {
        Functions.RunOnMainThread(() =>
        {
            var isCurrentWeapon = CurrentWeapon == weapon;
            SetWeaponAmmo(weapon, 0);
            RemoveWeapon(weapon);
            Emit("RemoveWeapon", (uint)weapon);
            if (isCurrentWeapon)
                SetCurrentWeapon((uint)WeaponModel.Fist);
        });
    }

    public void RemoveFactionEquippedWeapons()
    {
        Functions.RunOnMainThread(() =>
        {
            foreach (var equippedWeapon in FactionEquippedWeapons)
                RemoveWeaponEx((WeaponHash)equippedWeapon);

            FactionEquippedWeapons = [];
        });
    }
}