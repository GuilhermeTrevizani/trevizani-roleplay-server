using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class Character : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string RegisterIp { get; private set; } = string.Empty;
    public DateTime LastAccessDate { get; private set; } = DateTime.Now;
    public string? LastAccessIp { get; private set; }
    public uint Model { get; private set; }
    public float PosX { get; private set; }
    public float PosY { get; private set; }
    public float PosZ { get; private set; }
    public int Health { get; private set; } = 100;
    public int Armor { get; private set; }
    public uint Dimension { get; private set; }
    public int ConnectedTime { get; private set; }
    public Guid? FactionId { get; private set; }
    public Guid? FactionRankId { get; private set; }
    public int Bank { get; private set; } = 5000;
    public DateTime? DeathDate { get; private set; }
    public string DeathReason { get; private set; } = string.Empty;
    public CharacterJob Job { get; private set; } = CharacterJob.Unemployed;
    public string? PersonalizationJSON { get; private set; }
    public string History { get; private set; } = string.Empty;
    public Guid? EvaluatingStaffUserId { get; private set; }
    public Guid? EvaluatorStaffUserId { get; private set; }
    public string RejectionReason { get; private set; } = string.Empty;
    public CharacterNameChangeStatus NameChangeStatus { get; private set; } = CharacterNameChangeStatus.Allowed;
    public CharacterPersonalizationStep PersonalizationStep { get; private set; } = CharacterPersonalizationStep.Character;
    public DateTime? DeletedDate { get; private set; }
    public DateTime? JailFinalDate { get; private set; }
    public DateTime? DriverLicenseValidDate { get; private set; }
    public DateTime? DriverLicenseBlockedDate { get; private set; }
    public Guid? PoliceOfficerBlockedDriverLicenseCharacterId { get; private set; }
    public int Badge { get; private set; }
    public DateTime? AnnouncementLastUseDate { get; private set; }
    public int ExtraPayment { get; private set; }
    public string WoundsJSON { get; private set; } = "[]";
    public CharacterWound Wound { get; private set; } = CharacterWound.None;
    public CharacterSex Sex { get; private set; }
    public ulong Mask { get; private set; }
    public string FactionFlagsJSON { get; private set; } = "[]";
    public Guid? DrugItemTemplateId { get; private set; }
    public DateTime? DrugEndDate { get; private set; }
    public byte ThresoldDeath { get; private set; }
    public DateTime? ThresoldDeathEndDate { get; private set; }
    public bool CKAvaliation { get; private set; }
    public int BankAccount { get; private set; }
    public int Outfit { get; private set; } = 1;
    public string OutfitsJSON { get; private set; } = string.Empty;
    public int OutfitOnDuty { get; private set; } = 1;
    public string OutfitsOnDutyJSON { get; private set; } = string.Empty;
    public CharacterBloodType BloodType { get; private set; }
    public bool Bleeding { get; private set; }
    public string WeaponsBodyJSON { get; private set; } = "[]";
    public CharacterWeaponLicenseType WeaponLicenseType { get; private set; } = CharacterWeaponLicenseType.PF;
    public DateTime? WeaponLicenseValidDate { get; private set; }
    public Guid? PoliceOfficerWeaponLicenseCharacterId { get; private set; }
    public CharacterWalkStyle WalkStyle { get; private set; } = CharacterWalkStyle.Default;
    public int InitialHelpHours { get; private set; }
    public int Age { get; private set; }
    public string Attributes { get; private set; } = string.Empty;
    public uint Cellphone { get; private set; }
    public bool Connected { get; private set; }

    [JsonIgnore]
    public User? User { get; private set; }

    [JsonIgnore]
    public Faction? Faction { get; private set; }

    [JsonIgnore]
    public FactionRank? FactionRank { get; private set; }

    [JsonIgnore]
    public User? EvaluatingStaffUser { get; private set; }

    [JsonIgnore]
    public User? EvaluatorStaffUser { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerBlockedDriverLicenseCharacter { get; private set; }

    [JsonIgnore]
    public ICollection<CharacterItem>? Items { get; private set; }

    [JsonIgnore]
    public ICollection<Vehicle>? Vehicles { get; private set; }

    [JsonIgnore]
    public ICollection<Session>? Sessions { get; private set; }

    [JsonIgnore]
    public Character? PoliceOfficerWeaponLicenseCharacter { get; private set; }

    [JsonIgnore]
    public ItemTemplate? DrugItemTemplate { get; private set; }

    [JsonIgnore]
    public ICollection<Property>? Properties { get; private set; }

    [JsonIgnore]
    public ICollection<Company>? Companies { get; private set; }

    [JsonIgnore]
    public ICollection<CharacterProperty>? PropertiesAccess { get; private set; }

    [JsonIgnore]
    public ICollection<CharacterVehicle>? VehiclesAccess { get; private set; }

    public void Create(string name, int age, string history, CharacterSex sex,
        Guid userId, string ip, uint model, int health, Guid? evaluatorStaffUserId, int bankAccount,
        CharacterBloodType bloodType, int initialHelpHours, float posX, float posY, float posZ)
    {
        Name = name;
        Age = age;
        History = history;
        Sex = sex;
        UserId = userId;
        RegisterIp = LastAccessIp = ip;
        Model = model;
        Health = health;
        EvaluatorStaffUserId = evaluatorStaffUserId;
        BankAccount = bankAccount;
        BloodType = bloodType;
        InitialHelpHours = initialHelpHours;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
    }

    public void UpdateApplication(string name, int age, string history, CharacterSex sex, uint model)
    {
        Name = name;
        Age = age;
        History = history;
        Sex = sex;
        EvaluatorStaffUserId = null;
        RejectionReason = string.Empty;
        Model = model;
    }

    public void Update(uint model, float posX, float posY, float posZ, int health, int armor, uint dimension,
        string personalizationJSON, string woundsJSON, string factionFlagsJSON)
    {
        Model = model;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        Health = health;
        Armor = armor;
        Dimension = dimension;
        PersonalizationJSON = personalizationJSON;
        WoundsJSON = woundsJSON;
        FactionFlagsJSON = factionFlagsJSON;
        SetLastAccessDate();
    }

    public void Delete()
    {
        DeletedDate = DateTime.Now;
    }

    public void SetJob(CharacterJob job)
    {
        Job = job;
    }

    public void QuitJob()
    {
        Job = CharacterJob.Unemployed;
        ResetExtraPayment();
    }

    public void ResetExtraPayment()
    {
        ExtraPayment = 0;
    }

    public void AddExtraPayment(int extraPayment)
    {
        ExtraPayment += extraPayment;
    }

    public void SetEvaluatingStaffUser(Guid userId)
    {
        EvaluatingStaffUserId = userId;
    }

    public void AcceptAplication(Guid userId)
    {
        EvaluatorStaffUserId = userId;
        EvaluatingStaffUserId = null;
    }

    public void RejectApplication(Guid userId, string reason)
    {
        EvaluatorStaffUserId = userId;
        EvaluatingStaffUserId = null;
        RejectionReason = reason;
    }

    public void SetJailFinalDate(DateTime? date)
    {
        JailFinalDate = date;
    }

    public void SetWound(CharacterWound wound)
    {
        Wound = wound;
    }

    public void SetFaction(Guid factionId, Guid factionRankId, bool criminal)
    {
        FactionId = factionId;
        FactionRankId = factionRankId;
        if (!criminal)
            Job = CharacterJob.Unemployed;
    }

    public void UpdateFaction(Guid factionRankId, string factionFlagsJSON, int badge)
    {
        FactionRankId = factionRankId;
        FactionFlagsJSON = factionFlagsJSON;
        Badge = badge;
    }

    public void UseDrug(Guid drugItemTemplateId, int thresoldDeath, int minutesDuration)
    {
        var newThresoldDeath = ThresoldDeath + thresoldDeath;
        ThresoldDeath = Convert.ToByte(Math.Min(newThresoldDeath, 100));
        DrugItemTemplateId = drugItemTemplateId;
        DrugEndDate = (ThresoldDeath == 100 ? DateTime.Now : DrugEndDate ?? DateTime.Now).AddMinutes(minutesDuration);
        ThresoldDeathEndDate = null;
    }

    public void ClearDrug()
    {
        DrugItemTemplateId = null;
        DrugEndDate = null;
        ThresoldDeath = 0;
        ThresoldDeathEndDate = null;
    }

    public void SetThresoldDeathEndDate()
    {
        DrugItemTemplateId = null;
        DrugEndDate = null;
        ThresoldDeathEndDate = DateTime.Now.AddHours(1);
    }

    public void AddBank(int value)
    {
        Bank += value;
    }

    public void RemoveBank(int value)
    {
        Bank -= value;
    }

    public void SetAnnouncementLastUseDate()
    {
        AnnouncementLastUseDate = DateTime.Now;
    }

    public void SetMask(ulong mask)
    {
        Mask = mask;
    }

    public void ResetFaction()
    {
        FactionId = FactionRankId = null;
        Badge = 0;
        FactionFlagsJSON = "[]";
        Armor = 0;

        if (WeaponLicenseType == CharacterWeaponLicenseType.LEO)
            RemoveWeaponLicense(null);
    }

    public void SetLastAccessDate()
    {
        LastAccessDate = DateTime.Now;
    }

    public void AddConnectedTime()
    {
        ConnectedTime++;
    }

    public void SetBank(int bank)
    {
        Bank = bank;
    }

    public void SetNameChangeStatus(CharacterNameChangeStatus status)
    {
        NameChangeStatus = status;
    }

    public void UpdateLastAccess(string ip)
    {
        LastAccessIp = ip;
        Mask = 0;
    }

    public void SetPoliceOfficerBlockedDriverLicenseCharacterId(Guid id)
    {
        PoliceOfficerBlockedDriverLicenseCharacterId = id;
    }

    public void SetPersonalizationStep(CharacterPersonalizationStep step)
    {
        PersonalizationStep = step;
    }

    public void SetPersonalizationJSON(string json)
    {
        PersonalizationJSON = json;
    }

    public void SetDriverLicense()
    {
        DriverLicenseValidDate = DateTime.Now.AddMonths(3);
        PoliceOfficerBlockedDriverLicenseCharacterId = null;
        DriverLicenseBlockedDate = null;
    }

    public void RemoveDeath()
    {
        DeathDate = null;
        DeathReason = string.Empty;
        CKAvaliation = false;
    }

    public void SetDeath(string reason)
    {
        DeathDate = DateTime.Now;
        DeathReason = reason;
        CKAvaliation = false;
    }

    public void SetCKAvaliation()
    {
        CKAvaliation = true;
    }

    public void SetNameChangeStatus()
    {
        NameChangeStatus = NameChangeStatus == CharacterNameChangeStatus.Allowed
            ?
            CharacterNameChangeStatus.Blocked
            :
            CharacterNameChangeStatus.Allowed;
    }

    public void SetOutfit(int outfit, string outfitsJson)
    {
        Outfit = outfit;
        OutfitsJSON = outfitsJson;
    }

    public void SetOutfitOnDuty(int outfit, string outfitsJson)
    {
        OutfitOnDuty = outfit;
        OutfitsOnDutyJSON = outfitsJson;
    }

    public void SetDriverLicenseBlockedDate(DateTime? driverLicenseBlockedDate)
    {
        DriverLicenseBlockedDate = driverLicenseBlockedDate;
    }

    public void SetBleeding(bool bleeding)
    {
        Bleeding = bleeding;
    }

    public void SetWeaponsBodyJSON(string weaponsBodyJSON)
    {
        WeaponsBodyJSON = weaponsBodyJSON;
    }

    public void SetWeaponLicense(CharacterWeaponLicenseType type, int months, Guid policeOfficerCharacterId)
    {
        WeaponLicenseType = type;
        WeaponLicenseValidDate = DateTime.Now.AddMonths(months);
        PoliceOfficerWeaponLicenseCharacterId = policeOfficerCharacterId;
    }

    public void RemoveWeaponLicense(Guid? policeOfficerCharacterId)
    {
        WeaponLicenseType = CharacterWeaponLicenseType.PF;
        WeaponLicenseValidDate = null;
        PoliceOfficerWeaponLicenseCharacterId = policeOfficerCharacterId;
    }

    public void SetWalkStyle(CharacterWalkStyle walkStyle)
    {
        WalkStyle = walkStyle;
    }

    public void ReduceInitialHelpHours()
    {
        InitialHelpHours--;
    }

    public void SetAttributes(string attributes, int age)
    {
        Attributes = attributes;
        Age = age;
    }

    public void FixPosition()
    {
        PosX = 0;
        PosY = 0;
        PosZ = 0;
        Dimension = 0;
    }

    public void SetCellphone(uint cellphone)
    {
        Cellphone = cellphone;
    }

    public void SetConnected(bool connected)
    {
        Connected = connected;
    }

    public CharacterStatus GetStatus()
    {
        var status = CharacterStatus.Alive;

        if (CKAvaliation)
            status = CharacterStatus.CKAvaliation;
        else if (DeathDate.HasValue)
            status = CharacterStatus.Dead;
        else if (!string.IsNullOrWhiteSpace(RejectionReason))
            status = CharacterStatus.Rejected;
        else if (!EvaluatorStaffUserId.HasValue)
            status = CharacterStatus.AwaitingEvaluation;

        return status;
    }

    public void SetName(string name)
    {
        Name = name;
    }
}