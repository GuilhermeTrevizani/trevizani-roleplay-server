using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class CompanyEntranceBenefit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "EntranceBenefitCooldownHours",
                table: "Parameters",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "EntranceBenefitCooldownUsers",
                table: "Parameters",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "EntranceBenefitValue",
                table: "Parameters",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "EntranceBenefit",
                table: "Companies",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EntranceBenefitCooldown",
                table: "Companies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntranceBenefitUsersJson",
                table: "Companies",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesSafesMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Value = table.Column<uint>(type: "int unsigned", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisterDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesSafesMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesSafesMovements_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesSafesMovements_CompanyId",
                table: "CompaniesSafesMovements",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompaniesSafesMovements");

            migrationBuilder.DropColumn(
                name: "EntranceBenefitCooldownHours",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "EntranceBenefitCooldownUsers",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "EntranceBenefitValue",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "EntranceBenefit",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "EntranceBenefitCooldown",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "EntranceBenefitUsersJson",
                table: "Companies");
        }
    }
}
