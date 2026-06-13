using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryAndEmailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryCode",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryCodeExpiry",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeliveryCodeUsed",
                table: "Shipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverEmail",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCode",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryCodeExpiry",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryCodeUsed",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReceiverEmail",
                table: "Shipments");
        }
    }
}
