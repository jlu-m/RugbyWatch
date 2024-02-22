using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RugbyWatch.Migrations
{
    /// <inheritdoc />
    public partial class AddFileIdOnSuspiciousTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainMatchFileId",
                table: "SuspiciousMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousMatchesFileId",
                table: "SuspiciousMatches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainMatchFileId",
                table: "SuspiciousMatches");

            migrationBuilder.DropColumn(
                name: "PreviousMatchesFileId",
                table: "SuspiciousMatches");
        }
    }
}
