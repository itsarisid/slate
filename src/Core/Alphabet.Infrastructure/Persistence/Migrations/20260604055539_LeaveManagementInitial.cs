using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alphabet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LeaveManagementInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetLocations_Location_LocationId",
                table: "AssetLocations");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "AssetLocations",
                newName: "Id");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "AssetLocations",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "AssetLocations",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AssetLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "AssetLocations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "AssetLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AssetLocations",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AssetLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AssetLocations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentLocationId",
                table: "AssetLocations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "AssetLocations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "AssetLocations",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_AssetLocations_Code",
                table: "AssetLocations",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetLocations_Code",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "ParentLocationId",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AssetLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AssetLocations");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AssetLocations",
                newName: "LocationId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "AssetLocations",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "AssetLocations",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    TempId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempId2 = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.UniqueConstraint("AK_Location_TempId1", x => x.TempId1);
                    table.UniqueConstraint("AK_Location_TempId2", x => x.TempId2);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLocations_Location_LocationId",
                table: "AssetLocations",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
