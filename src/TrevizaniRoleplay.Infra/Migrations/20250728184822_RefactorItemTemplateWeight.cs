using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevizaniRoleplay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RefactorItemTemplateWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ItemsTemplates SET Weight = Weight * 1000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ItemsTemplates SET Weight = Weight / 1000");
        }
    }
}
