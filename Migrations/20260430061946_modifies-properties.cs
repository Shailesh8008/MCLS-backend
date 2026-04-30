using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCLS.Migrations
{
    /// <inheritdoc />
    public partial class modifiesproperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogDate",
                table: "VoyageLogs");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "VoyageLogs",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "VoyageLogs",
                newName: "createdAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LogDate",
                table: "VoyageLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
