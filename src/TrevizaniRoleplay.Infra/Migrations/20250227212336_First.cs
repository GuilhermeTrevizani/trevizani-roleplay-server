using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdminObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Model = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RotR = table.Column<float>(type: "float", nullable: false),
                    RotP = table.Column<float>(type: "float", nullable: false),
                    RotY = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminObjects", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Animations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Display = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dictionary = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Flag = table.Column<int>(type: "int", nullable: false),
                    OnlyInVehicle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Category = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Scenario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Blips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Type = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Color = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blips", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CrackDens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    OnlinePoliceOfficers = table.Column<int>(type: "int", nullable: false),
                    CooldownQuantityLimit = table.Column<int>(type: "int", nullable: false),
                    CooldownHours = table.Column<int>(type: "int", nullable: false),
                    CooldownDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrackDens", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Crimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrisonMinutes = table.Column<int>(type: "int", nullable: false),
                    FineValue = table.Column<int>(type: "int", nullable: false),
                    DriverLicensePoints = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crimes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Dealerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    VehiclePosX = table.Column<float>(type: "float", nullable: false),
                    VehiclePosY = table.Column<float>(type: "float", nullable: false),
                    VehiclePosZ = table.Column<float>(type: "float", nullable: false),
                    VehicleRotR = table.Column<float>(type: "float", nullable: false),
                    VehicleRotP = table.Column<float>(type: "float", nullable: false),
                    VehicleRotY = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealerships", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmergencyCalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosLocation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyCalls", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    FireSpanLife = table.Column<float>(type: "float", nullable: false),
                    MaxFireSpan = table.Column<int>(type: "int", nullable: false),
                    SecondsNewFireSpan = table.Column<int>(type: "int", nullable: false),
                    PositionNewFireSpan = table.Column<float>(type: "float", nullable: false),
                    FireSpanDamage = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fires", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Furnitures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Model = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Door = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AudioOutput = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TVTexture = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subcategory = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UseSlot = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Furnitures", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemsTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Category = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<float>(type: "float", nullable: false),
                    Image = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectModel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsTemplates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterJob = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Salary = table.Column<int>(type: "int", nullable: false),
                    BlipType = table.Column<uint>(type: "int unsigned", nullable: false),
                    BlipColor = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    BlipName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleRentModel = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleRentValue = table.Column<int>(type: "int", nullable: false),
                    VehicleRentPosX = table.Column<float>(type: "float", nullable: false),
                    VehicleRentPosY = table.Column<float>(type: "float", nullable: false),
                    VehicleRentPosZ = table.Column<float>(type: "float", nullable: false),
                    VehicleRentRotR = table.Column<float>(type: "float", nullable: false),
                    VehicleRentRotP = table.Column<float>(type: "float", nullable: false),
                    VehicleRentRotY = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MaxCharactersOnline = table.Column<int>(type: "int", nullable: false),
                    HospitalValue = table.Column<int>(type: "int", nullable: false),
                    BarberValue = table.Column<int>(type: "int", nullable: false),
                    ClothesValue = table.Column<int>(type: "int", nullable: false),
                    DriverLicenseBuyValue = table.Column<int>(type: "int", nullable: false),
                    DriverLicenseRenewValue = table.Column<int>(type: "int", nullable: false),
                    FuelValue = table.Column<int>(type: "int", nullable: false),
                    Paycheck = table.Column<int>(type: "int", nullable: false),
                    AnnouncementValue = table.Column<int>(type: "int", nullable: false),
                    ExtraPaymentGarbagemanValue = table.Column<int>(type: "int", nullable: false),
                    Blackout = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InactivePropertiesDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    KeyValue = table.Column<int>(type: "int", nullable: false),
                    LockValue = table.Column<int>(type: "int", nullable: false),
                    IPLsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TattooValue = table.Column<int>(type: "int", nullable: false),
                    CooldownDismantleHours = table.Column<int>(type: "int", nullable: false),
                    PropertyRobberyConnectedTime = table.Column<int>(type: "int", nullable: false),
                    CooldownPropertyRobberyRobberHours = table.Column<int>(type: "int", nullable: false),
                    CooldownPropertyRobberyPropertyHours = table.Column<int>(type: "int", nullable: false),
                    PoliceOfficersPropertyRobbery = table.Column<int>(type: "int", nullable: false),
                    InitialTimeCrackDen = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    EndTimeCrackDen = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FirefightersBlockHeal = table.Column<int>(type: "int", nullable: false),
                    PropertyProtectionLevelPercentageValue = table.Column<float>(type: "float", nullable: false),
                    VehicleDismantlingPercentageValue = table.Column<float>(type: "float", nullable: false),
                    VehicleDismantlingSeizedDays = table.Column<int>(type: "int", nullable: false),
                    VehicleDismantlingPartsChanceJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleDismantlingMinutes = table.Column<int>(type: "int", nullable: false),
                    PlasticSurgeryValue = table.Column<int>(type: "int", nullable: false),
                    WhoCanLogin = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FishingItemsChanceJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleInsurancePercentage = table.Column<float>(type: "float", nullable: false),
                    PrisonInsidePosX = table.Column<float>(type: "float", nullable: false),
                    PrisonInsidePosY = table.Column<float>(type: "float", nullable: false),
                    PrisonInsidePosZ = table.Column<float>(type: "float", nullable: false),
                    PrisonInsideDimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    PrisonOutsidePosX = table.Column<float>(type: "float", nullable: false),
                    PrisonOutsidePosY = table.Column<float>(type: "float", nullable: false),
                    PrisonOutsidePosZ = table.Column<float>(type: "float", nullable: false),
                    PrisonOutsideDimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    WeaponsInfosJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BodyPartsDamagesJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeaponLicenseMonths = table.Column<int>(type: "int", nullable: false),
                    WeaponLicenseMaxWeapon = table.Column<int>(type: "int", nullable: false),
                    WeaponLicenseMaxAmmo = table.Column<int>(type: "int", nullable: false),
                    WeaponLicenseMaxAttachment = table.Column<int>(type: "int", nullable: false),
                    WeaponLicensePurchaseDaysInterval = table.Column<int>(type: "int", nullable: false),
                    PremiumItemsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AudioRadioStationsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnemploymentAssistance = table.Column<int>(type: "int", nullable: false),
                    PremiumPointPackagesJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MOTD = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesCalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Origin = table.Column<uint>(type: "int unsigned", nullable: false),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesCalls", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Origin = table.Column<uint>(type: "int unsigned", nullable: false),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Favorite = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Blocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesContacts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesGroups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesNumbers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Smugglers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Cellphone = table.Column<uint>(type: "int unsigned", nullable: false),
                    Model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RotR = table.Column<float>(type: "float", nullable: false),
                    RotP = table.Column<float>(type: "float", nullable: false),
                    RotY = table.Column<float>(type: "float", nullable: false),
                    AllowedCharactersJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CooldownQuantityLimit = table.Column<int>(type: "int", nullable: false),
                    CooldownMinutes = table.Column<int>(type: "int", nullable: false),
                    CooldownDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Smugglers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Spots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spots", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TruckerLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    DeliveryValue = table.Column<int>(type: "int", nullable: false),
                    LoadWaitTime = table.Column<int>(type: "int", nullable: false),
                    UnloadWaitTime = table.Column<int>(type: "int", nullable: false),
                    AllowedVehiclesJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckerLocations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiscordId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscordUsername = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscordDisplayName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterIp = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastAccessIp = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastAccessDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Staff = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NameChanges = table.Column<int>(type: "int", nullable: false),
                    HelpRequestsAnswersQuantity = table.Column<int>(type: "int", nullable: false),
                    StaffDutyTime = table.Column<int>(type: "int", nullable: false),
                    TimeStampToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Premium = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PremiumValidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PMToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FactionChatToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PlateChanges = table.Column<int>(type: "int", nullable: false),
                    AnnouncementToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ChatFontType = table.Column<int>(type: "int", nullable: false),
                    ChatLines = table.Column<int>(type: "int", nullable: false),
                    ChatFontSize = table.Column<int>(type: "int", nullable: false),
                    StaffFlagsJSON = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FactionToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CharacterApplicationsQuantity = table.Column<int>(type: "int", nullable: false),
                    CooldownDismantle = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PropertyRobberyCooldown = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VehicleTagToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ChatBackgroundColor = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AjailMinutes = table.Column<int>(type: "int", nullable: false),
                    ShowNametagId = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PremiumPoints = table.Column<int>(type: "int", nullable: false),
                    NumberChanges = table.Column<int>(type: "int", nullable: false),
                    CharacterSlots = table.Column<int>(type: "int", nullable: false),
                    ExtraInteriorFurnitureSlots = table.Column<int>(type: "int", nullable: false),
                    ExtraOutfitSlots = table.Column<int>(type: "int", nullable: false),
                    DiscordBoosterDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AmbientSoundToggle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayResolution = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FreezingTimePropertyEntrance = table.Column<int>(type: "int", nullable: false),
                    ShowOwnNametag = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReceiveSMSDiscord = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WeaponsIds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponsIds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DealershipsVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DealershipId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Model = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealershipsVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealershipsVehicles_Dealerships_DealershipId",
                        column: x => x.DealershipId,
                        principalTable: "Dealerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CrackDensItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CrackDenId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrackDensItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrackDensItems_CrackDens_CrackDenId",
                        column: x => x.CrackDenId,
                        principalTable: "CrackDens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrackDensItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ThresoldDeath = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    GarbageCollectorMultiplier = table.Column<float>(type: "float", nullable: false),
                    TruckerMultiplier = table.Column<float>(type: "float", nullable: false),
                    MinutesDuration = table.Column<int>(type: "int", nullable: false),
                    Warn = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShakeGameplayCamName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShakeGameplayCamIntensity = table.Column<float>(type: "float", nullable: false),
                    TimecycModifier = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnimpostFXName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drugs_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RotR = table.Column<float>(type: "float", nullable: false),
                    RotP = table.Column<float>(type: "float", nullable: false),
                    RotY = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesGroupsUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PhoneGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    Permission = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesGroupsUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhonesGroupsUsers_PhonesGroups_PhoneGroupId",
                        column: x => x.PhoneGroupId,
                        principalTable: "PhonesGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Origin = table.Column<uint>(type: "int unsigned", nullable: false),
                    Number = table.Column<uint>(type: "int unsigned", nullable: true),
                    PhoneGroupId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Message = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationX = table.Column<float>(type: "float", nullable: true),
                    LocationY = table.Column<float>(type: "float", nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhonesMessages_PhonesGroups_PhoneGroupId",
                        column: x => x.PhoneGroupId,
                        principalTable: "PhonesGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TruckerLocationsDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TruckerLocationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckerLocationsDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckerLocationsDeliveries_TruckerLocations_TruckerLocationId",
                        column: x => x.TruckerLocationId,
                        principalTable: "TruckerLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HelpRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AnswerDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StaffUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpRequests_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PremiumPointPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreferenceId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumPointPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PremiumPointPurchases_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PremiumPointPurchases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UCPActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UCPActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UCPActions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UCPActionsExecuted",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UCPActionRegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UCPActionsExecuted", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UCPActionsExecuted_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhonesMessagesReads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PhoneMessageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhonesMessagesReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhonesMessagesReads_PhonesMessages_PhoneMessageId",
                        column: x => x.PhoneMessageId,
                        principalTable: "PhonesMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Banishments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StaffUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Banishments_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Banishments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bodies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Model = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    PlaceOfDeath = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonalizationJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutfitJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WoundsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MorgueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bodies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BodiesItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BodyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodiesItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BodiesItems_Bodies_BodyId",
                        column: x => x.BodyId,
                        principalTable: "Bodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BodiesItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastAccessDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastAccessIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Model = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    ConnectedTime = table.Column<int>(type: "int", nullable: false),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FactionRankId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Bank = table.Column<int>(type: "int", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeathReason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Job = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PersonalizationJSON = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    History = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvaluatingStaffUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    EvaluatorStaffUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RejectionReason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameChangeStatus = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PersonalizationStep = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    JailFinalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DriverLicenseValidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DriverLicenseBlockedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PoliceOfficerBlockedDriverLicenseCharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Badge = table.Column<int>(type: "int", nullable: false),
                    AnnouncementLastUseDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExtraPayment = table.Column<int>(type: "int", nullable: false),
                    WoundsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Wound = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Sex = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Mask = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    FactionFlagsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DrugItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DrugEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ThresoldDeath = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ThresoldDeathEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CKAvaliation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BankAccount = table.Column<int>(type: "int", nullable: false),
                    Outfit = table.Column<int>(type: "int", nullable: false),
                    OutfitsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutfitOnDuty = table.Column<int>(type: "int", nullable: false),
                    OutfitsOnDutyJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BloodType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Bleeding = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WeaponsBodyJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeaponLicenseType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    WeaponLicenseValidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PoliceOfficerWeaponLicenseCharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Premium = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PremiumValidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WalkStyle = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    InitialHelpHours = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Attributes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cellphone = table.Column<uint>(type: "int unsigned", nullable: false),
                    Connected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Characters_PoliceOfficerBlockedDriverLicenseChara~",
                        column: x => x.PoliceOfficerBlockedDriverLicenseCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Characters_Characters_PoliceOfficerWeaponLicenseCharacterId",
                        column: x => x.PoliceOfficerWeaponLicenseCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Characters_ItemsTemplates_DrugItemTemplateId",
                        column: x => x.DrugItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Characters_Users_EvaluatingStaffUserId",
                        column: x => x.EvaluatingStaffUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Characters_Users_EvaluatorStaffUserId",
                        column: x => x.EvaluatorStaffUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Characters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharactersItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    InUse = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OnlyOnDuty = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharactersItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharactersItems_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharactersItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    WeekRentValue = table.Column<int>(type: "int", nullable: false),
                    RentPaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Color = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlipType = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    BlipColor = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Safe = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CrackDensSells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CrackDenId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrackDensSells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrackDensSells_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrackDensSells_CrackDens_CrackDenId",
                        column: x => x.CrackDenId,
                        principalTable: "CrackDens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrackDensSells_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Color = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slots = table.Column<int>(type: "int", nullable: false),
                    ChatColor = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ShortName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factions_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Graffitis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Text = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Font = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RotR = table.Column<float>(type: "float", nullable: false),
                    RotP = table.Column<float>(type: "float", nullable: false),
                    RotY = table.Column<float>(type: "float", nullable: false),
                    ColorR = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ColorG = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ColorB = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ColorA = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Graffitis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Graffitis_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Infos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Image = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Infos_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginCharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OriginIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginSocialClubName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetCharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TargetIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetSocialClubName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Characters_OriginCharacterId",
                        column: x => x.OriginCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Logs_Characters_TargetCharacterId",
                        column: x => x.TargetCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Punishments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StaffUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Punishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Punishments_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Punishments_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FinalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Ip = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SocialClubName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FlagsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesCharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesCharacters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CostPrice = table.Column<int>(type: "int", nullable: false),
                    SellPrice = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesItems_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompaniesItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesSells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CostPrice = table.Column<int>(type: "int", nullable: false),
                    SellPrice = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesSells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesSells_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompaniesSells_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompaniesSells_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesTuningPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CostPercentagePrice = table.Column<float>(type: "float", nullable: false),
                    SellPercentagePrice = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesTuningPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesTuningPrices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Confiscations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PoliceOfficerCharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Identifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Confiscations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Confiscations_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Confiscations_Characters_PoliceOfficerCharacterId",
                        column: x => x.PoliceOfficerCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Confiscations_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Doors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hash = table.Column<long>(type: "bigint", nullable: false),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Locked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doors_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doors_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsEquipmentss",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyOrVehicle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SWAT = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UPR = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsEquipmentss", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsEquipmentss_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsFrequencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsFrequencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsFrequencies_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsRanks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salary = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsRanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsRanks_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsStorages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsStorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsStorages_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsUniforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutfitJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Sex = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsUniforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsUniforms_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FinalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsUnits_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactionsUnits_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliceOfficerCharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DriverLicensePoints = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fines_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fines_Characters_PoliceOfficerCharacterId",
                        column: x => x.PoliceOfficerCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fines_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ForensicTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Identifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForensicTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForensicTests_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForensicTests_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Jails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliceOfficerCharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jails_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jails_Characters_PoliceOfficerCharacterId",
                        column: x => x.PoliceOfficerCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jails_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Interior = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    EntrancePosX = table.Column<float>(type: "float", nullable: false),
                    EntrancePosY = table.Column<float>(type: "float", nullable: false),
                    EntrancePosZ = table.Column<float>(type: "float", nullable: false),
                    EntranceDimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    ExitPosX = table.Column<float>(type: "float", nullable: false),
                    ExitPosY = table.Column<float>(type: "float", nullable: false),
                    ExitPosZ = table.Column<float>(type: "float", nullable: false),
                    Address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<uint>(type: "int unsigned", nullable: false),
                    LockNumber = table.Column<uint>(type: "int unsigned", nullable: false),
                    Locked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProtectionLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RobberyValue = table.Column<int>(type: "int", nullable: false),
                    RobberyCooldown = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentPropertyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    EntranceRotR = table.Column<float>(type: "float", nullable: false),
                    EntranceRotP = table.Column<float>(type: "float", nullable: false),
                    EntranceRotY = table.Column<float>(type: "float", nullable: false),
                    ExitRotR = table.Column<float>(type: "float", nullable: false),
                    ExitRotP = table.Column<float>(type: "float", nullable: false),
                    ExitRotY = table.Column<float>(type: "float", nullable: false),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Time = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    Weather = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Properties_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Properties_ParentPropertyId",
                        column: x => x.ParentPropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Model = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RotR = table.Column<float>(type: "float", nullable: false),
                    RotP = table.Column<float>(type: "float", nullable: false),
                    RotY = table.Column<float>(type: "float", nullable: false),
                    Color1R = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Color1G = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Color1B = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Color2R = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Color2G = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Color2B = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Plate = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    EngineHealth = table.Column<float>(type: "float", nullable: false),
                    Livery = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SeizedValue = table.Column<int>(type: "int", nullable: false),
                    Fuel = table.Column<int>(type: "int", nullable: false),
                    Sold = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DamagesJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BodyHealth = table.Column<float>(type: "float", nullable: false),
                    LockNumber = table.Column<uint>(type: "int unsigned", nullable: false),
                    ProtectionLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    XMR = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ModsJSON = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WheelType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    WheelVariation = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    WheelColor = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeonColorR = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeonColorG = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeonColorB = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeonLeft = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NeonRight = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NeonFront = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NeonBack = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HeadlightColor = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LightsMultiplier = table.Column<float>(type: "float", nullable: false),
                    WindowTint = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TireSmokeColorR = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TireSmokeColorG = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TireSmokeColorB = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SeizedDismantling = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SeizedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InsuranceDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExtrasJSON = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewPlate = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dimension = table.Column<uint>(type: "int unsigned", nullable: false),
                    Drift = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConfiscationsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConfiscationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Identifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiscationsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiscationsItems_Confiscations_ConfiscationId",
                        column: x => x.ConfiscationId,
                        principalTable: "Confiscations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiscationsItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsEquipmentsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionEquipmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsEquipmentsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsEquipmentsItems_FactionsEquipmentss_FactionEquipment~",
                        column: x => x.FactionEquipmentId,
                        principalTable: "FactionsEquipmentss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactionsEquipmentsItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsStoragesItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionStorageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Price = table.Column<int>(type: "int", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsStoragesItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsStoragesItems_FactionsStorages_FactionStorageId",
                        column: x => x.FactionStorageId,
                        principalTable: "FactionsStorages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactionsStoragesItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FactionsUnitsCharacters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FactionUnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionsUnitsCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactionsUnitsCharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactionsUnitsCharacters_FactionsUnits_FactionUnitId",
                        column: x => x.FactionUnitId,
                        principalTable: "FactionsUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropertiesEntrances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PropertyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EntrancePosX = table.Column<float>(type: "float", nullable: false),
                    EntrancePosY = table.Column<float>(type: "float", nullable: false),
                    EntrancePosZ = table.Column<float>(type: "float", nullable: false),
                    ExitPosX = table.Column<float>(type: "float", nullable: false),
                    ExitPosY = table.Column<float>(type: "float", nullable: false),
                    ExitPosZ = table.Column<float>(type: "float", nullable: false),
                    EntranceRotR = table.Column<float>(type: "float", nullable: false),
                    EntranceRotP = table.Column<float>(type: "float", nullable: false),
                    EntranceRotY = table.Column<float>(type: "float", nullable: false),
                    ExitRotR = table.Column<float>(type: "float", nullable: false),
                    ExitRotP = table.Column<float>(type: "float", nullable: false),
                    ExitRotY = table.Column<float>(type: "float", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertiesEntrances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertiesEntrances_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropertiesFurnitures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PropertyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Model = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PosX = table.Column<float>(type: "float", nullable: false),
                    PosY = table.Column<float>(type: "float", nullable: false),
                    PosZ = table.Column<float>(type: "float", nullable: false),
                    RotR = table.Column<float>(type: "float", nullable: false),
                    RotP = table.Column<float>(type: "float", nullable: false),
                    RotY = table.Column<float>(type: "float", nullable: false),
                    Interior = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Locked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertiesFurnitures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertiesFurnitures_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropertiesItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PropertyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertiesItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertiesItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertiesItems_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SeizedVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VehicleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliceOfficerCharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeizedVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeizedVehicles_Characters_PoliceOfficerCharacterId",
                        column: x => x.PoliceOfficerCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeizedVehicles_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeizedVehicles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VehiclesItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VehicleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ItemTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Subtype = table.Column<uint>(type: "int unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiclesItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiclesItems_ItemsTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ItemsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehiclesItems_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wanted",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliceOfficerCharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WantedCharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    WantedVehicleId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PoliceOfficerDeletedCharacterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wanted", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wanted_Characters_PoliceOfficerCharacterId",
                        column: x => x.PoliceOfficerCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wanted_Characters_PoliceOfficerDeletedCharacterId",
                        column: x => x.PoliceOfficerDeletedCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wanted_Characters_WantedCharacterId",
                        column: x => x.WantedCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wanted_Vehicles_WantedVehicleId",
                        column: x => x.WantedVehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ForensicTestsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ForensicTestId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginConfiscationItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TargetConfiscationItemId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Identifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Result = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForensicTestsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForensicTestsItems_ConfiscationsItems_OriginConfiscationItem~",
                        column: x => x.OriginConfiscationItemId,
                        principalTable: "ConfiscationsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForensicTestsItems_ConfiscationsItems_TargetConfiscationItem~",
                        column: x => x.TargetConfiscationItemId,
                        principalTable: "ConfiscationsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForensicTestsItems_ForensicTests_ForensicTestId",
                        column: x => x.ForensicTestId,
                        principalTable: "ForensicTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Banishments_CharacterId",
                table: "Banishments",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Banishments_StaffUserId",
                table: "Banishments",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Banishments_UserId",
                table: "Banishments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodies_CharacterId",
                table: "Bodies",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_BodiesItems_BodyId",
                table: "BodiesItems",
                column: "BodyId");

            migrationBuilder.CreateIndex(
                name: "IX_BodiesItems_ItemTemplateId",
                table: "BodiesItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_DrugItemTemplateId",
                table: "Characters",
                column: "DrugItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_EvaluatingStaffUserId",
                table: "Characters",
                column: "EvaluatingStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_EvaluatorStaffUserId",
                table: "Characters",
                column: "EvaluatorStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_FactionId",
                table: "Characters",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_FactionRankId",
                table: "Characters",
                column: "FactionRankId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_PoliceOfficerBlockedDriverLicenseCharacterId",
                table: "Characters",
                column: "PoliceOfficerBlockedDriverLicenseCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_PoliceOfficerWeaponLicenseCharacterId",
                table: "Characters",
                column: "PoliceOfficerWeaponLicenseCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CharactersItems_CharacterId",
                table: "CharactersItems",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharactersItems_ItemTemplateId",
                table: "CharactersItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CharacterId",
                table: "Companies",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesCharacters_CharacterId",
                table: "CompaniesCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesCharacters_CompanyId",
                table: "CompaniesCharacters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesItems_CompanyId",
                table: "CompaniesItems",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesItems_ItemTemplateId",
                table: "CompaniesItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesSells_CharacterId",
                table: "CompaniesSells",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesSells_CompanyId",
                table: "CompaniesSells",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesSells_ItemTemplateId",
                table: "CompaniesSells",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesTuningPrices_CompanyId",
                table: "CompaniesTuningPrices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Confiscations_CharacterId",
                table: "Confiscations",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Confiscations_FactionId",
                table: "Confiscations",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Confiscations_PoliceOfficerCharacterId",
                table: "Confiscations",
                column: "PoliceOfficerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiscationsItems_ConfiscationId",
                table: "ConfiscationsItems",
                column: "ConfiscationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiscationsItems_ItemTemplateId",
                table: "ConfiscationsItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CrackDensItems_CrackDenId",
                table: "CrackDensItems",
                column: "CrackDenId");

            migrationBuilder.CreateIndex(
                name: "IX_CrackDensItems_ItemTemplateId",
                table: "CrackDensItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CrackDensSells_CharacterId",
                table: "CrackDensSells",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CrackDensSells_CrackDenId",
                table: "CrackDensSells",
                column: "CrackDenId");

            migrationBuilder.CreateIndex(
                name: "IX_CrackDensSells_ItemTemplateId",
                table: "CrackDensSells",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DealershipsVehicles_DealershipId",
                table: "DealershipsVehicles",
                column: "DealershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Doors_CompanyId",
                table: "Doors",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Doors_FactionId",
                table: "Doors",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_ItemTemplateId",
                table: "Drugs",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_CharacterId",
                table: "Factions",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactionsEquipmentsItems_FactionEquipmentId",
                table: "FactionsEquipmentsItems",
                column: "FactionEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsEquipmentsItems_ItemTemplateId",
                table: "FactionsEquipmentsItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsEquipmentss_FactionId",
                table: "FactionsEquipmentss",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsFrequencies_FactionId",
                table: "FactionsFrequencies",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsRanks_FactionId",
                table: "FactionsRanks",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsStorages_FactionId",
                table: "FactionsStorages",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsStoragesItems_FactionStorageId",
                table: "FactionsStoragesItems",
                column: "FactionStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsStoragesItems_ItemTemplateId",
                table: "FactionsStoragesItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsUniforms_FactionId",
                table: "FactionsUniforms",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsUnits_CharacterId",
                table: "FactionsUnits",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsUnits_FactionId",
                table: "FactionsUnits",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsUnitsCharacters_CharacterId",
                table: "FactionsUnitsCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionsUnitsCharacters_FactionUnitId",
                table: "FactionsUnitsCharacters",
                column: "FactionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CharacterId",
                table: "FinancialTransactions",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_CharacterId",
                table: "Fines",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_FactionId",
                table: "Fines",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_PoliceOfficerCharacterId",
                table: "Fines",
                column: "PoliceOfficerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ForensicTests_CharacterId",
                table: "ForensicTests",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ForensicTests_FactionId",
                table: "ForensicTests",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ForensicTestsItems_ForensicTestId",
                table: "ForensicTestsItems",
                column: "ForensicTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ForensicTestsItems_OriginConfiscationItemId",
                table: "ForensicTestsItems",
                column: "OriginConfiscationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ForensicTestsItems_TargetConfiscationItemId",
                table: "ForensicTestsItems",
                column: "TargetConfiscationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Graffitis_CharacterId",
                table: "Graffitis",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpRequests_StaffUserId",
                table: "HelpRequests",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpRequests_UserId",
                table: "HelpRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Infos_CharacterId",
                table: "Infos",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemTemplateId",
                table: "Items",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Jails_CharacterId",
                table: "Jails",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Jails_FactionId",
                table: "Jails",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Jails_PoliceOfficerCharacterId",
                table: "Jails",
                column: "PoliceOfficerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_OriginCharacterId",
                table: "Logs",
                column: "OriginCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_TargetCharacterId",
                table: "Logs",
                column: "TargetCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonesGroupsUsers_PhoneGroupId",
                table: "PhonesGroupsUsers",
                column: "PhoneGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonesMessages_PhoneGroupId",
                table: "PhonesMessages",
                column: "PhoneGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PhonesMessagesReads_PhoneMessageId",
                table: "PhonesMessagesReads",
                column: "PhoneMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumPointPurchases_TargetUserId",
                table: "PremiumPointPurchases",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiumPointPurchases_UserId",
                table: "PremiumPointPurchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CharacterId",
                table: "Properties",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CompanyId",
                table: "Properties",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_FactionId",
                table: "Properties",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ParentPropertyId",
                table: "Properties",
                column: "ParentPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesEntrances_PropertyId",
                table: "PropertiesEntrances",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesFurnitures_PropertyId",
                table: "PropertiesFurnitures",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesItems_ItemTemplateId",
                table: "PropertiesItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertiesItems_PropertyId",
                table: "PropertiesItems",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Punishments_CharacterId",
                table: "Punishments",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Punishments_StaffUserId",
                table: "Punishments",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeizedVehicles_FactionId",
                table: "SeizedVehicles",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SeizedVehicles_PoliceOfficerCharacterId",
                table: "SeizedVehicles",
                column: "PoliceOfficerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_SeizedVehicles_VehicleId",
                table: "SeizedVehicles",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CharacterId",
                table: "Sessions",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckerLocationsDeliveries_TruckerLocationId",
                table: "TruckerLocationsDeliveries",
                column: "TruckerLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_UCPActions_UserId",
                table: "UCPActions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UCPActionsExecuted_UserId",
                table: "UCPActionsExecuted",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CharacterId",
                table: "Vehicles",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FactionId",
                table: "Vehicles",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclesItems_ItemTemplateId",
                table: "VehiclesItems",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclesItems_VehicleId",
                table: "VehiclesItems",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Wanted_PoliceOfficerCharacterId",
                table: "Wanted",
                column: "PoliceOfficerCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Wanted_PoliceOfficerDeletedCharacterId",
                table: "Wanted",
                column: "PoliceOfficerDeletedCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Wanted_WantedCharacterId",
                table: "Wanted",
                column: "WantedCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Wanted_WantedVehicleId",
                table: "Wanted",
                column: "WantedVehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banishments_Characters_CharacterId",
                table: "Banishments",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bodies_Characters_CharacterId",
                table: "Bodies",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_FactionsRanks_FactionRankId",
                table: "Characters",
                column: "FactionRankId",
                principalTable: "FactionsRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Factions_FactionId",
                table: "Characters",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Factions_Characters_CharacterId",
                table: "Factions");

            migrationBuilder.DropTable(
                name: "AdminObjects");

            migrationBuilder.DropTable(
                name: "Animations");

            migrationBuilder.DropTable(
                name: "Banishments");

            migrationBuilder.DropTable(
                name: "Blips");

            migrationBuilder.DropTable(
                name: "BodiesItems");

            migrationBuilder.DropTable(
                name: "CharactersItems");

            migrationBuilder.DropTable(
                name: "CompaniesCharacters");

            migrationBuilder.DropTable(
                name: "CompaniesItems");

            migrationBuilder.DropTable(
                name: "CompaniesSells");

            migrationBuilder.DropTable(
                name: "CompaniesTuningPrices");

            migrationBuilder.DropTable(
                name: "CrackDensItems");

            migrationBuilder.DropTable(
                name: "CrackDensSells");

            migrationBuilder.DropTable(
                name: "Crimes");

            migrationBuilder.DropTable(
                name: "DealershipsVehicles");

            migrationBuilder.DropTable(
                name: "Doors");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "EmergencyCalls");

            migrationBuilder.DropTable(
                name: "FactionsEquipmentsItems");

            migrationBuilder.DropTable(
                name: "FactionsFrequencies");

            migrationBuilder.DropTable(
                name: "FactionsStoragesItems");

            migrationBuilder.DropTable(
                name: "FactionsUniforms");

            migrationBuilder.DropTable(
                name: "FactionsUnitsCharacters");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "Fines");

            migrationBuilder.DropTable(
                name: "Fires");

            migrationBuilder.DropTable(
                name: "ForensicTestsItems");

            migrationBuilder.DropTable(
                name: "Furnitures");

            migrationBuilder.DropTable(
                name: "Graffitis");

            migrationBuilder.DropTable(
                name: "HelpRequests");

            migrationBuilder.DropTable(
                name: "Infos");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Jails");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.DropTable(
                name: "PhonesCalls");

            migrationBuilder.DropTable(
                name: "PhonesContacts");

            migrationBuilder.DropTable(
                name: "PhonesGroupsUsers");

            migrationBuilder.DropTable(
                name: "PhonesMessagesReads");

            migrationBuilder.DropTable(
                name: "PhonesNumbers");

            migrationBuilder.DropTable(
                name: "PremiumPointPurchases");

            migrationBuilder.DropTable(
                name: "PropertiesEntrances");

            migrationBuilder.DropTable(
                name: "PropertiesFurnitures");

            migrationBuilder.DropTable(
                name: "PropertiesItems");

            migrationBuilder.DropTable(
                name: "Punishments");

            migrationBuilder.DropTable(
                name: "SeizedVehicles");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Smugglers");

            migrationBuilder.DropTable(
                name: "Spots");

            migrationBuilder.DropTable(
                name: "TruckerLocationsDeliveries");

            migrationBuilder.DropTable(
                name: "UCPActions");

            migrationBuilder.DropTable(
                name: "UCPActionsExecuted");

            migrationBuilder.DropTable(
                name: "VehiclesItems");

            migrationBuilder.DropTable(
                name: "Wanted");

            migrationBuilder.DropTable(
                name: "WeaponsIds");

            migrationBuilder.DropTable(
                name: "Bodies");

            migrationBuilder.DropTable(
                name: "CrackDens");

            migrationBuilder.DropTable(
                name: "Dealerships");

            migrationBuilder.DropTable(
                name: "FactionsEquipmentss");

            migrationBuilder.DropTable(
                name: "FactionsStorages");

            migrationBuilder.DropTable(
                name: "FactionsUnits");

            migrationBuilder.DropTable(
                name: "ConfiscationsItems");

            migrationBuilder.DropTable(
                name: "ForensicTests");

            migrationBuilder.DropTable(
                name: "PhonesMessages");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "TruckerLocations");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Confiscations");

            migrationBuilder.DropTable(
                name: "PhonesGroups");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "FactionsRanks");

            migrationBuilder.DropTable(
                name: "ItemsTemplates");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Factions");
        }
    }
}
