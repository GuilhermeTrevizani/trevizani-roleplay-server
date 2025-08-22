using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UserReceiveNotificationsOnDiscord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceiveNotificationsOnDiscord",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiveNotificationsOnDiscord",
                table: "Users");
        }
    }
}
