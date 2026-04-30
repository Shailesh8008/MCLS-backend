using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCLS.Migrations
{
    /// <inheritdoc />
    public partial class modifiedvessel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VesselType",
                table: "Vessels",
                newName: "Type");

            migrationBuilder.AddColumn<double>(
                name: "SpeedInKiloMeter",
                table: "VoyageLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "createdAt",
                table: "VoyageLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Flag",
                table: "Vessels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeedInKiloMeter",
                table: "VoyageLogs");

            migrationBuilder.DropColumn(
                name: "createdAt",
                table: "VoyageLogs");

            migrationBuilder.DropColumn(
                name: "Flag",
                table: "Vessels");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Vessels",
                newName: "VesselType");
        }
    }
}
