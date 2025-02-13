using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pootis_Bot.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "rule_reaction_role_id",
                table: "server",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rule_reaction_role_id",
                table: "server");
        }
    }
}
