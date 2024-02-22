using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RugbyWatch.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspiciousMatchTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SuspiciousMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    IllegalPlayers = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuspiciousMatches_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousMatches_MatchId",
                table: "SuspiciousMatches",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuspiciousMatches");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Matches");
        }
    }
}
