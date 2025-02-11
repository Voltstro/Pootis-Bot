using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pootis_Bot.Shared.Messages;

#nullable disable

namespace Pootis_Bot.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:message_type", "goodbye,welcome");

            migrationBuilder.CreateTable(
                name: "profile",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    discord_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xp = table.Column<long>(type: "bigint", nullable: false),
                    last_xp_message_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    profile_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "server",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    discord_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    welcome_goodbye_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    welcome_message_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    goodbye_message_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    rule_reaction_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    rule_reaction_message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    rule_reaction_emoji = table.Column<string>(type: "text", nullable: true),
                    rule_reaction_enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_server", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "server_message",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    server_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<MessageType>(type: "message_type", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_server_message", x => x.id);
                    table.ForeignKey(
                        name: "fk_server_message_server_server_id",
                        column: x => x.server_id,
                        principalTable: "server",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profile_discord_id",
                table: "profile",
                column: "discord_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_server_discord_id",
                table: "server",
                column: "discord_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_server_message_server_id",
                table: "server_message",
                column: "server_id");
            
            //Last update last update time function
            migrationBuilder.Sql(@"
CREATE FUNCTION trg_update_updated_at_time() RETURNS TRIGGER LANGUAGE PLPGSQL AS $$
BEGIN
    NEW.updated_at := now();
    RETURN NEW;
END;
$$;
");

            migrationBuilder.Sql(@"
CREATE TRIGGER trg_server_updated_at_time
    BEFORE INSERT OR UPDATE
    ON server
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_updated_at_time();

CREATE TRIGGER trg_profile_updated_at_time
    BEFORE INSERT OR UPDATE
    ON profile
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_updated_at_time();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile");

            migrationBuilder.DropTable(
                name: "server_message");

            migrationBuilder.DropTable(
                name: "server");
        }
    }
}
