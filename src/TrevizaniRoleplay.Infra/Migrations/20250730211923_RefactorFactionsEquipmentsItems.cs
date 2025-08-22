using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFactionsEquipmentsItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM FactionsEquipmentsItems");

            migrationBuilder.DropForeignKey(
                name: "FK_FactionsEquipmentsItems_ItemsTemplates_ItemTemplateId",
                table: "FactionsEquipmentsItems");

            migrationBuilder.DropIndex(
                name: "IX_FactionsEquipmentsItems_ItemTemplateId",
                table: "FactionsEquipmentsItems");

            migrationBuilder.DropColumn(
                name: "Extra",
                table: "FactionsEquipmentsItems");

            migrationBuilder.DropColumn(
                name: "ItemTemplateId",
                table: "FactionsEquipmentsItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "FactionsEquipmentsItems");

            migrationBuilder.DropColumn(
                name: "OnlyOnDuty",
                table: "CharactersItems");

            migrationBuilder.RenameColumn(
                name: "Subtype",
                table: "FactionsEquipmentsItems",
                newName: "Ammo");

            migrationBuilder.AddColumn<string>(
                name: "ComponentsJson",
                table: "FactionsEquipmentsItems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Weapon",
                table: "FactionsEquipmentsItems",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComponentsJson",
                table: "FactionsEquipmentsItems");

            migrationBuilder.DropColumn(
                name: "Weapon",
                table: "FactionsEquipmentsItems");

            migrationBuilder.RenameColumn(
                name: "Ammo",
                table: "FactionsEquipmentsItems",
                newName: "Subtype");

            migrationBuilder.AddColumn<string>(
                name: "Extra",
                table: "FactionsEquipmentsItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemTemplateId",
                table: "FactionsEquipmentsItems",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "FactionsEquipmentsItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OnlyOnDuty",
                table: "CharactersItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FactionsEquipmentsItems_ItemTemplateId",
                table: "FactionsEquipmentsItems",
                column: "ItemTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_FactionsEquipmentsItems_ItemsTemplates_ItemTemplateId",
                table: "FactionsEquipmentsItems",
                column: "ItemTemplateId",
                principalTable: "ItemsTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
