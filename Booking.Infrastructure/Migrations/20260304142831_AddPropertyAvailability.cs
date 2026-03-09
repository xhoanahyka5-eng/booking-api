using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Migrations
{
    public partial class AddPropertyAvailability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PropertyAvailabilities");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "PropertyAvailabilities",
                newName: "Date");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "PropertyAvailabilities",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "PropertyAvailabilities");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "PropertyAvailabilities",
                newName: "StartDate");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "PropertyAvailabilities",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
