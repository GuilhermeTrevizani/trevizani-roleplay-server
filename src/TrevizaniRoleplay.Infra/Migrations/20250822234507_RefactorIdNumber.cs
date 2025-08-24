using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RefactorIdNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Badge",
                table: "Characters",
                newName: "IdNumber");

            migrationBuilder.Sql(@"SET @row_number = 0;

            UPDATE Characters p
            JOIN (
                SELECT id, (@row_number := @row_number + 1) AS rn
                FROM characters
                ORDER BY registerdate ASC
            ) ranked ON p.id = ranked.id
            SET p.IdNumber = ranked.rn;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdNumber",
                table: "Characters",
                newName: "Badge");
        }
    }
}
