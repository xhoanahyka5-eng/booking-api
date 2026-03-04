using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPropertyAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Addresses_AddressId1",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_AddressId1",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AddressId1",
                table: "Properties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId1",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_AddressId1",
                table: "Properties",
                column: "AddressId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Addresses_AddressId1",
                table: "Properties",
                column: "AddressId1",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
