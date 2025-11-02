using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pootis_Bot.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoVc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "autovc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    server_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_vc_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    base_name = table.Column<string>(type: "text", nullable: false),
                    max_channels = table.Column<int>(type: "integer", nullable: false),
                    max_users = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_autovc", x => x.id);
                    table.ForeignKey(
                        name: "fk_autovc_server_server_id",
                        column: x => x.server_id,
                        principalTable: "server",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_autovc_base_vc_channel_id",
                table: "autovc",
                column: "base_vc_channel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_autovc_server_id",
                table: "autovc",
                column: "server_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "autovc");
        }
    }
}
