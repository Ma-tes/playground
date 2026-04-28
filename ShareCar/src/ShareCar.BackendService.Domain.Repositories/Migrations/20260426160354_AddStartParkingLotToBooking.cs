using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareCar.BackendService.Domain.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddStartParkingLotToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StartParkingLotId",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StartParkingLotId",
                table: "Bookings",
                column: "StartParkingLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ParkingLots_StartParkingLotId",
                table: "Bookings",
                column: "StartParkingLotId",
                principalTable: "ParkingLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ParkingLots_StartParkingLotId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StartParkingLotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StartParkingLotId",
                table: "Bookings");
        }
    }
}
