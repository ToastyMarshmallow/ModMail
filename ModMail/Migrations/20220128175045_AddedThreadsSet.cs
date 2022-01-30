using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralModMail.Migrations
{
    public partial class AddedThreadsSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadEntity_Guilds_Guild",
                table: "ThreadEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThreadEntity",
                table: "ThreadEntity");

            migrationBuilder.RenameTable(
                name: "ThreadEntity",
                newName: "Threads");

            migrationBuilder.RenameIndex(
                name: "IX_ThreadEntity_Guild",
                table: "Threads",
                newName: "IX_Threads_Guild");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Threads",
                table: "Threads",
                column: "Channel");

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_Guilds_Guild",
                table: "Threads",
                column: "Guild",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Threads_Guilds_Guild",
                table: "Threads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Threads",
                table: "Threads");

            migrationBuilder.RenameTable(
                name: "Threads",
                newName: "ThreadEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Threads_Guild",
                table: "ThreadEntity",
                newName: "IX_ThreadEntity_Guild");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThreadEntity",
                table: "ThreadEntity",
                column: "Channel");

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadEntity_Guilds_Guild",
                table: "ThreadEntity",
                column: "Guild",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
