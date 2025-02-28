using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum LogType : byte
{
    [Display(Name = Globalization.STAFF)]
    Staff = 1,

    [Display(Name = Globalization.FACTION)]
    Faction = 2,

    [Display(Name = Globalization.FACTION_CHAT)]
    FactionChat = 3,

    [Display(Name = Globalization.MONEY)]
    Money = 4,

    [Display(Name = Globalization.SELL)]
    Sell = 5,

    [Display(Name = Globalization.ENTRANCE)]
    Entrance = 6,

    [Display(Name = Globalization.EXIT)]
    Exit = 7,

    [Display(Name = Globalization.DEATH)]
    Death = 8,

    [Display(Name = Globalization.WEAPON)]
    Weapon = 9,

    [Display(Name = Globalization.NAME_CHANGE)]
    NameChange = 10,

    [Display(Name = Globalization.CHARACTER_DELETE)]
    CharacterDelete = 11,

    [Display(Name = Globalization.PLATE_CHANGE)]
    PlateChange = 12,

    [Display(Name = Globalization.ADVERTISEMENT)]
    Advertisement = 13,

    [Display(Name = Globalization.GOVERNMENT_ADVERTISEMENT)]
    GovernmentAdvertisement = 14,

    [Display(Name = Globalization.DELIVER_ITEM)]
    DeliverItem = 15,

    [Display(Name = Globalization.DROP_ITEM)]
    DropItem = 16,

    [Display(Name = Globalization.GET_GROUND_ITEM)]
    GetGroundItem = 17,

    [Display(Name = Globalization.STEAL_ITEM)]
    StealItem = 18,

    [Display(Name = Globalization.SMUGGLER)]
    Smuggler = 19,

    [Display(Name = Globalization.GIVE_ITEM)]
    GiveItem = 20,

    [Display(Name = Globalization.PUT_PROPERTY_ITEM)]
    PutPropertyItem = 21,

    [Display(Name = Globalization.GET_PROPERTY_ITEM)]
    GetPropertyItem = 22,

    [Display(Name = Globalization.PUT_VEHICLE_ITEM)]
    PutVehicleItem = 23,

    [Display(Name = Globalization.GET_VEHICLE_ITEM)]
    GetVehicleItem = 24,

    [Display(Name = Globalization.FACTION_VEHICLE_REPAIR)]
    FactionVehicleRepair = 25,

    [Display(Name = Globalization.PLAYER_VEHICLE_REPAIR)]
    PlayerVehicleRepair = 26,

    [Display(Name = Globalization.SPAWN_VEHICLE)]
    SpawnVehicle = 27,

    [Display(Name = Globalization.PARK_VEHICLE)]
    ParkVehicle = 28,

    [Display(Name = Globalization.HEAL_ME)]
    HealMe = 29,

    [Display(Name = Globalization.PRIVATE_MESSAGES)]
    PrivateMessages = 30,

    [Display(Name = Globalization.MASK)]
    Mask = 31,

    [Display(Name = Globalization.VEHICLE_DESTRUCTION)]
    VehicleDestruction = 32,

    [Display(Name = Globalization.HACK)]
    Hack = 33,

    [Display(Name = Globalization.DRUG)]
    Drug = 34,

    [Display(Name = Globalization.LOCK_BREAK)]
    LockBreak = 35,

    [Display(Name = Globalization.HOT_WIRE)]
    HotWire = 36,

    [Display(Name = Globalization.DISMANTLING)]
    Dismantling = 37,

    [Display(Name = Globalization.BREAK_IN)]
    BreakIn = 38,

    [Display(Name = Globalization.STEAL_PROPERTY)]
    StealProperty = 39,

    // 40 is free

    [Display(Name = Globalization.VIEW_CRACK_DEN_SALES)]
    ViewCrackDenSales = 41,

    [Display(Name = Globalization.PROPERTY_FURNITURE_EDIT)]
    EditPropertyFurniture = 42,

    [Display(Name = Globalization.GLOBAL_OOC_CHAT)]
    GlobalOOCChat = 43,

    [Display(Name = Globalization.STAFF_CHAT)]
    StaffChat = 44,

    [Display(Name = Globalization.PUT_IN_VEHICLE)]
    PutInVehicle = 45,

    [Display(Name = Globalization.REMOVE_FROM_VEHICLE)]
    RemoveFromVehicle = 46,

    [Display(Name = Globalization.COMPANY)]
    Company = 47,

    [Display(Name = Globalization.COMPANY_ADVERTISEMENT)]
    CompanyAdvertisement = 48,

    [Display(Name = Globalization.MECHANIC_TUNNING)]
    MechanicTunning = 49,

    [Display(Name = Globalization.JOB)]
    Job = 50,

    [Display(Name = Globalization.DOUBLE_IDENTITY)]
    DoubleIdentity = 51,

    [Display(Name = Globalization.INFO)]
    Info = 52,

    [Display(Name = Globalization.USE_ITEM)]
    UseItem = 53,

    [Display(Name = Globalization.GET_BODY_ITEM)]
    GetBodyItem = 54,

    [Display(Name = Globalization.PREMIUM)]
    Premium = 55,

    [Display(Name = Globalization.PROPERTY_FURNITURE_SELL)]
    SellPropertyFurniture = 56,

    [Display(Name = Globalization.DONATE)]
    Donate = 57,

    [Display(Name = Globalization.CELLPHONE)]
    Cellphone = 58,

    [Display(Name = Globalization.BUY)]
    Buy = 59,

    [Display(Name = Globalization.FIX)]
    Fix = 60,

    [Display(Name = Globalization.ATTRIBUTES)]
    Attributes = 61,

    [Display(Name = Globalization.GENERAL)]
    General = 62,

    [Display(Name = Globalization.IC_CHAT)]
    ICChat = 63,

    [Display(Name = Globalization.OOC_CHAT)]
    OOCChat = 64,

    [Display(Name = Globalization.VIEW_LOGS)]
    ViewLogs = 65,
}