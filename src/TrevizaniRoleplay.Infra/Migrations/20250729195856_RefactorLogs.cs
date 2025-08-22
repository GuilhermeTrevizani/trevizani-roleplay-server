using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "InactivePropertiesDate",
                table: "Parameters");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginUserId",
                table: "Logs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_OriginUserId",
                table: "Logs",
                column: "OriginUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Users_OriginUserId",
                table: "Logs",
                column: "OriginUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Users_OriginUserId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_OriginUserId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "OriginUserId",
                table: "Logs");

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Properties",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InactivePropertiesDate",
                table: "Parameters",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
