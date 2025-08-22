using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class CharactersAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LockNumber",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "KeyValue",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "LockValue",
                table: "Parameters");

            migrationBuilder.AddColumn<Guid>(
                name: "CharacterId",
                table: "CompaniesSafesMovements",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "CharactersProperties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PropertyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharactersProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharactersProperties_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharactersProperties_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharactersVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CharacterId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VehicleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharactersVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharactersVehicles_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharactersVehicles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CharactersProperties_CharacterId",
                table: "CharactersProperties",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharactersProperties_PropertyId",
                table: "CharactersProperties",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_CharactersVehicles_CharacterId",
                table: "CharactersVehicles",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharactersVehicles_VehicleId",
                table: "CharactersVehicles",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharactersProperties");

            migrationBuilder.DropTable(
                name: "CharactersVehicles");

            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "CompaniesSafesMovements");

            migrationBuilder.AddColumn<uint>(
                name: "LockNumber",
                table: "Vehicles",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "LockNumber",
                table: "Properties",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "KeyValue",
                table: "Parameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LockValue",
                table: "Parameters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
