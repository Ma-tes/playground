using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareCar.BackendService.Domain.Repositories.Migrations
{
  /// <inheritdoc />
  public partial class InitialSchema : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "ParkingLots",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
          Latitude = table.Column<double>(type: "REAL", nullable: false),
          Longitude = table.Column<double>(type: "REAL", nullable: false),
          TotalCapacity = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ParkingLots", x => x.Id);
        });

      migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
          PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
          Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
          Role = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

      migrationBuilder.CreateTable(
        name: "Vehicles",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Model = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
          PlateNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
          CurrentParkingLotId = table.Column<int>(type: "INTEGER", nullable: true),
          Status = table.Column<int>(type: "INTEGER", nullable: false),
          Odometer = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Vehicles", x => x.Id);
          table.ForeignKey(
            name: "FK_Vehicles_ParkingLots_CurrentParkingLotId",
            column: x => x.CurrentParkingLotId,
            principalTable: "ParkingLots",
            principalColumn: "Id");
        });

      migrationBuilder.CreateTable(
        name: "BlockLogs",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
          AdminId = table.Column<int>(type: "INTEGER", nullable: false),
          StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
          EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
          Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_BlockLogs", x => x.Id);
          table.ForeignKey(
            name: "FK_BlockLogs_Users_AdminId",
            column: x => x.AdminId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
            name: "FK_BlockLogs_Vehicles_VehicleId",
            column: x => x.VehicleId,
            principalTable: "Vehicles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        });

      migrationBuilder.CreateTable(
        name: "Bookings",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          UserId = table.Column<int>(type: "INTEGER", nullable: false),
          VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
          StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
          EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
          StartOdometer = table.Column<int>(type: "INTEGER", nullable: false),
          EndOdometer = table.Column<int>(type: "INTEGER", nullable: true),
          TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
          IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Bookings", x => x.Id);
          table.ForeignKey(
            name: "FK_Bookings_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
          table.ForeignKey(
            name: "FK_Bookings_Vehicles_VehicleId",
            column: x => x.VehicleId,
            principalTable: "Vehicles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        });

      migrationBuilder.CreateTable(
        name: "StatusHistories",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
          OldStatus = table.Column<int>(type: "INTEGER", nullable: false),
          NewStatus = table.Column<int>(type: "INTEGER", nullable: false),
          ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
          TriggeredBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_StatusHistories", x => x.Id);
          table.ForeignKey(
            name: "FK_StatusHistories_Vehicles_VehicleId",
            column: x => x.VehicleId,
            principalTable: "Vehicles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        });

      migrationBuilder.CreateIndex(
        name: "IX_BlockLogs_AdminId",
        table: "BlockLogs",
        column: "AdminId");

      migrationBuilder.CreateIndex(
        name: "IX_BlockLogs_VehicleId",
        table: "BlockLogs",
        column: "VehicleId");

      migrationBuilder.CreateIndex(
        name: "IX_Bookings_IsActive",
        table: "Bookings",
        column: "IsActive");

      migrationBuilder.CreateIndex(
        name: "IX_Bookings_UserId",
        table: "Bookings",
        column: "UserId");

      migrationBuilder.CreateIndex(
        name: "IX_Bookings_VehicleId",
        table: "Bookings",
        column: "VehicleId");

      migrationBuilder.CreateIndex(
        name: "IX_StatusHistories_VehicleId",
        table: "StatusHistories",
        column: "VehicleId");

      migrationBuilder.CreateIndex(
        name: "IX_Users_Username",
        table: "Users",
        column: "Username",
        unique: true);

      migrationBuilder.CreateIndex(
        name: "IX_Vehicles_CurrentParkingLotId",
        table: "Vehicles",
        column: "CurrentParkingLotId");

      migrationBuilder.CreateIndex(
        name: "IX_Vehicles_PlateNumber",
        table: "Vehicles",
        column: "PlateNumber",
        unique: true);

      migrationBuilder.CreateIndex(
        name: "IX_Vehicles_Status",
        table: "Vehicles",
        column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
        name: "BlockLogs");

      migrationBuilder.DropTable(
        name: "Bookings");

      migrationBuilder.DropTable(
        name: "StatusHistories");

      migrationBuilder.DropTable(
        name: "Users");

      migrationBuilder.DropTable(
        name: "Vehicles");

      migrationBuilder.DropTable(
        name: "ParkingLots");
    }
  }
}
