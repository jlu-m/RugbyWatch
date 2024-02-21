using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RugbyWatch.Migrations
{
    /// <inheritdoc />
    public partial class MatchDateFormatChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "regionalMatchReportId",
                table: "LastDownloadedMatchReports",
                newName: "RegionalMatchReportId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Time",
                table: "Matches",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegionalMatchReportId",
                table: "LastDownloadedMatchReports",
                newName: "regionalMatchReportId");

            migrationBuilder.AlterColumn<string>(
                name: "Time",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
