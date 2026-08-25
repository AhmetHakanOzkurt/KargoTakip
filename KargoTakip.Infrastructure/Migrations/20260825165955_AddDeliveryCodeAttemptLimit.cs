using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryCodeAttemptLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryCodeFailedAttempts",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryCodeLockedUntil",
                table: "Shipments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCodeFailedAttempts",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryCodeLockedUntil",
                table: "Shipments");
        }
    }
}
