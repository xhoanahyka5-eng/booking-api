using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPropertyAvailabilityRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyAvailabilities_Properties_PropertyId",
                table: "PropertyAvailabilities");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyAvailabilities_Properties_PropertyId",
                table: "PropertyAvailabilities",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyAvailabilities_Properties_PropertyId",
                table: "PropertyAvailabilities");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyAvailabilities_Properties_PropertyId",
                table: "PropertyAvailabilities",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
