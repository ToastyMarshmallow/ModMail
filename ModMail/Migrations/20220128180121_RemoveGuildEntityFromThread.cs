using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralModMail.Migrations
{
    public partial class RemoveGuildEntityFromThread : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Threads_Guilds_Guild",
                table: "Threads");

            migrationBuilder.DropIndex(
                name: "IX_Threads_Guild",
                table: "Threads");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Threads_Guild",
                table: "Threads",
                column: "Guild");

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_Guilds_Guild",
                table: "Threads",
                column: "Guild",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
