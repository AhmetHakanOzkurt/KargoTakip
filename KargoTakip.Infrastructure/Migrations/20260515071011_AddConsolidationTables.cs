using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsolidationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CityDistances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromCityId = table.Column<int>(type: "int", nullable: false),
                    ToCityId = table.Column<int>(type: "int", nullable: false),
                    DistanceKm = table.Column<int>(type: "int", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityDistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityDistances_Cities_FromCityId",
                        column: x => x.FromCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CityDistances_Cities_ToCityId",
                        column: x => x.ToCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsolidationPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    OriginBranchId = table.Column<int>(type: "int", nullable: false),
                    DestinationCityId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedDepartureAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualDepartureAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    UsedCapacity = table.Column<int>(type: "int", nullable: false),
                    OccupancyRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EstimatedFuelSaving = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidationPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidationPlans_Branches_OriginBranchId",
                        column: x => x.OriginBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsolidationPlans_Cities_DestinationCityId",
                        column: x => x.DestinationCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsolidationPlans_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsolidationPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsolidationPlanId = table.Column<int>(type: "int", nullable: false),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    AddedReason = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidationPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidationPlanItems_ConsolidationPlans_ConsolidationPlanId",
                        column: x => x.ConsolidationPlanId,
                        principalTable: "ConsolidationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsolidationPlanItems_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CityDistances_FromCityId",
                table: "CityDistances",
                column: "FromCityId");

            migrationBuilder.CreateIndex(
                name: "IX_CityDistances_ToCityId",
                table: "CityDistances",
                column: "ToCityId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationPlanItems_ConsolidationPlanId",
                table: "ConsolidationPlanItems",
                column: "ConsolidationPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationPlanItems_ShipmentId",
                table: "ConsolidationPlanItems",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationPlans_DestinationCityId",
                table: "ConsolidationPlans",
                column: "DestinationCityId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationPlans_OriginBranchId",
                table: "ConsolidationPlans",
                column: "OriginBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationPlans_VehicleId",
                table: "ConsolidationPlans",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CityDistances");

            migrationBuilder.DropTable(
                name: "ConsolidationPlanItems");

            migrationBuilder.DropTable(
                name: "ConsolidationPlans");
        }
    }
}
