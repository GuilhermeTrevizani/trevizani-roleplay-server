using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class VehicleSpawned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Characters_CharacterId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesSafesMovements_Companies_CompanyId",
                table: "CompaniesSafesMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_DealershipsVehicles_Dealerships_DealershipId",
                table: "DealershipsVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Factions_Characters_CharacterId",
                table: "Factions");

            migrationBuilder.DropForeignKey(
                name: "FK_FactionsFrequencies_Factions_FactionId",
                table: "FactionsFrequencies");

            migrationBuilder.AddColumn<bool>(
                name: "Spawned",
                table: "Vehicles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesSafesMovements_CharacterId",
                table: "CompaniesSafesMovements",
                column: "CharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Characters_CharacterId",
                table: "Companies",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesSafesMovements_Characters_CharacterId",
                table: "CompaniesSafesMovements",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesSafesMovements_Companies_CompanyId",
                table: "CompaniesSafesMovements",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DealershipsVehicles_Dealerships_DealershipId",
                table: "DealershipsVehicles",
                column: "DealershipId",
                principalTable: "Dealerships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Factions_Characters_CharacterId",
                table: "Factions",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FactionsFrequencies_Factions_FactionId",
                table: "FactionsFrequencies",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Characters_CharacterId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesSafesMovements_Characters_CharacterId",
                table: "CompaniesSafesMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_CompaniesSafesMovements_Companies_CompanyId",
                table: "CompaniesSafesMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_DealershipsVehicles_Dealerships_DealershipId",
                table: "DealershipsVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Factions_Characters_CharacterId",
                table: "Factions");

            migrationBuilder.DropForeignKey(
                name: "FK_FactionsFrequencies_Factions_FactionId",
                table: "FactionsFrequencies");

            migrationBuilder.DropIndex(
                name: "IX_CompaniesSafesMovements_CharacterId",
                table: "CompaniesSafesMovements");

            migrationBuilder.DropColumn(
                name: "Spawned",
                table: "Vehicles");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Characters_CharacterId",
                table: "Companies",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompaniesSafesMovements_Companies_CompanyId",
                table: "CompaniesSafesMovements",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DealershipsVehicles_Dealerships_DealershipId",
                table: "DealershipsVehicles",
                column: "DealershipId",
                principalTable: "Dealerships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Factions_Characters_CharacterId",
                table: "Factions",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FactionsFrequencies_Factions_FactionId",
                table: "FactionsFrequencies",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
