using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCleanArchitectureChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyAvailabilities_PropertyId",
                table: "PropertyAvailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAvailabilities_PropertyId_Date",
                table: "PropertyAvailabilities",
                columns: new[] { "PropertyId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyAvailabilities_PropertyId_Date",
                table: "PropertyAvailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAvailabilities_PropertyId",
                table: "PropertyAvailabilities",
                column: "PropertyId");
        }
    }
}
