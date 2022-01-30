using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstralModMail.Migrations
{
    public partial class InitalCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildEntity",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Category = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Log = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildEntity", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ThreadEntity",
                columns: table => new
                {
                    Channel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Recipient = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Guild = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadEntity", x => x.Channel);
                    table.ForeignKey(
                        name: "FK_ThreadEntity_GuildEntity_Guild",
                        column: x => x.Guild,
                        principalTable: "GuildEntity",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreadEntity_Guild",
                table: "ThreadEntity",
                column: "Guild");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreadEntity");

            migrationBuilder.DropTable(
                name: "GuildEntity");
        }
    }
}
