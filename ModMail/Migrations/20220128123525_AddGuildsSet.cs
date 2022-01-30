using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralModMail.Migrations
{
    public partial class AddGuildsSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadEntity_GuildEntity_Guild",
                table: "ThreadEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildEntity",
                table: "GuildEntity");

            migrationBuilder.RenameTable(
                name: "GuildEntity",
                newName: "Guilds");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadEntity_Guilds_Guild",
                table: "ThreadEntity",
                column: "Guild",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadEntity_Guilds_Guild",
                table: "ThreadEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds");

            migrationBuilder.RenameTable(
                name: "Guilds",
                newName: "GuildEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildEntity",
                table: "GuildEntity",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadEntity_GuildEntity_Guild",
                table: "ThreadEntity",
                column: "Guild",
                principalTable: "GuildEntity",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
