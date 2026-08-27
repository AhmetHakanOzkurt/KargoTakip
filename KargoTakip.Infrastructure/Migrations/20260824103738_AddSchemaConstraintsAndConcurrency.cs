using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaConstraintsAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Bu migration metin kolonlarini nvarchar(max) -> nvarchar(n) olarak
-- daraltir. Sinirdan uzun veri varsa ALTER COLUMN anlasilmaz bir hata ile
-- durur. Asagidaki on kontrol, hangi tablo ve kolonun soruna yol actigini
-- acikca bildirerek migration'i basta durdurur.
DECLARE @ihlaller TABLE (
    TabloAdi sysname, KolonAdi sysname, SinirDegeri int, IhlalSayisi int);

INSERT INTO @ihlaller (TabloAdi, KolonAdi, SinirDegeri, IhlalSayisi)
                SELECT 'VehicleTypes','RouteType',30,COUNT(*) FROM [VehicleTypes] WHERE DATALENGTH([RouteType]) / 2 > 30
                UNION ALL
                SELECT 'VehicleTypes','Name',50,COUNT(*) FROM [VehicleTypes] WHERE DATALENGTH([Name]) / 2 > 50
                UNION ALL
                SELECT 'Vehicles','PlateNumber',20,COUNT(*) FROM [Vehicles] WHERE DATALENGTH([PlateNumber]) / 2 > 20
                UNION ALL
                SELECT 'Users','Username',50,COUNT(*) FROM [Users] WHERE DATALENGTH([Username]) / 2 > 50
                UNION ALL
                SELECT 'Users','Role',20,COUNT(*) FROM [Users] WHERE DATALENGTH([Role]) / 2 > 20
                UNION ALL
                SELECT 'Users','PasswordHash',255,COUNT(*) FROM [Users] WHERE DATALENGTH([PasswordHash]) / 2 > 255
                UNION ALL
                SELECT 'Users','FullName',100,COUNT(*) FROM [Users] WHERE DATALENGTH([FullName]) / 2 > 100
                UNION ALL
                SELECT 'TransferRequests','Status',20,COUNT(*) FROM [TransferRequests] WHERE DATALENGTH([Status]) / 2 > 20
                UNION ALL
                SELECT 'TransferRequests','Note',500,COUNT(*) FROM [TransferRequests] WHERE DATALENGTH([Note]) / 2 > 500
                UNION ALL
                SELECT 'ShipmentStatusHistories','Status',30,COUNT(*) FROM [ShipmentStatusHistories] WHERE DATALENGTH([Status]) / 2 > 30
                UNION ALL
                SELECT 'ShipmentStatusHistories','ServiceSource',50,COUNT(*) FROM [ShipmentStatusHistories] WHERE DATALENGTH([ServiceSource]) / 2 > 50
                UNION ALL
                SELECT 'ShipmentStatusHistories','Note',500,COUNT(*) FROM [ShipmentStatusHistories] WHERE DATALENGTH([Note]) / 2 > 500
                UNION ALL
                SELECT 'Shipments','TrackingCode',32,COUNT(*) FROM [Shipments] WHERE DATALENGTH([TrackingCode]) / 2 > 32
                UNION ALL
                SELECT 'Shipments','SenderName',100,COUNT(*) FROM [Shipments] WHERE DATALENGTH([SenderName]) / 2 > 100
                UNION ALL
                SELECT 'Shipments','ReceiverName',100,COUNT(*) FROM [Shipments] WHERE DATALENGTH([ReceiverName]) / 2 > 100
                UNION ALL
                SELECT 'Shipments','ReceiverEmail',256,COUNT(*) FROM [Shipments] WHERE DATALENGTH([ReceiverEmail]) / 2 > 256
                UNION ALL
                SELECT 'Shipments','ReceiverAddress',255,COUNT(*) FROM [Shipments] WHERE DATALENGTH([ReceiverAddress]) / 2 > 255
                UNION ALL
                SELECT 'Shipments','Priority',20,COUNT(*) FROM [Shipments] WHERE DATALENGTH([Priority]) / 2 > 20
                UNION ALL
                SELECT 'Shipments','DeliveryCode',10,COUNT(*) FROM [Shipments] WHERE DATALENGTH([DeliveryCode]) / 2 > 10
                UNION ALL
                SELECT 'Shipments','CurrentStatus',30,COUNT(*) FROM [Shipments] WHERE DATALENGTH([CurrentStatus]) / 2 > 30
                UNION ALL
                SELECT 'Notifications','Message',500,COUNT(*) FROM [Notifications] WHERE DATALENGTH([Message]) / 2 > 500
                UNION ALL
                SELECT 'ConsolidationPlans','Status',30,COUNT(*) FROM [ConsolidationPlans] WHERE DATALENGTH([Status]) / 2 > 30
                UNION ALL
                SELECT 'ConsolidationPlanItems','AddedReason',50,COUNT(*) FROM [ConsolidationPlanItems] WHERE DATALENGTH([AddedReason]) / 2 > 50
                UNION ALL
                SELECT 'Cities','Region',50,COUNT(*) FROM [Cities] WHERE DATALENGTH([Region]) / 2 > 50
                UNION ALL
                SELECT 'Cities','Name',100,COUNT(*) FROM [Cities] WHERE DATALENGTH([Name]) / 2 > 100
                UNION ALL
                SELECT 'Branches','Name',100,COUNT(*) FROM [Branches] WHERE DATALENGTH([Name]) / 2 > 100
                UNION ALL
                SELECT 'Branches','Address',255,COUNT(*) FROM [Branches] WHERE DATALENGTH([Address]) / 2 > 255;

DELETE FROM @ihlaller WHERE IhlalSayisi = 0;

IF EXISTS (SELECT 1 FROM @ihlaller)
BEGIN
    DECLARE @detay nvarchar(2000) = (
        SELECT STRING_AGG(
            CONCAT(TabloAdi, '.', KolonAdi, ' (sinir ', SinirDegeri,
                   ', asan kayit: ', IhlalSayisi, ')'), '; ')
        FROM @ihlaller);

    DECLARE @mesaj nvarchar(3000) =
        CONCAT('Migration durduruldu: asagidaki kolonlarda sinir degerini ',
               'asan veri var. Once veriyi kisaltin veya duzeltin. -> ', @detay);

    THROW 51000, @mesaj, 1;
END
");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_BranchId",
                table: "Shipments");

            migrationBuilder.AlterColumn<string>(
                name: "RouteType",
                table: "VehicleTypes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "VehicleTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PlateNumber",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Vehicles",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TransferRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "TransferRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "TransferRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ShipmentStatusHistories",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceSource",
                table: "ShipmentStatusHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ShipmentStatusHistories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingCode",
                table: "Shipments",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SenderName",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverName",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverEmail",
                table: "Shipments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverAddress",
                table: "Shipments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Shipments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryCode",
                table: "Shipments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStatus",
                table: "Shipments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ConsolidationPlans",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AddedReason",
                table: "ConsolidationPlanItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Cities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Branches",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PlateNumber",
                table: "Vehicles",
                column: "PlateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_BranchId_CurrentStatus",
                table: "Shipments",
                columns: new[] { "BranchId", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TrackingCode",
                table: "Shipments",
                column: "TrackingCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_PlateNumber",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_BranchId_CurrentStatus",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_TrackingCode",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "TransferRequests");

            migrationBuilder.AlterColumn<string>(
                name: "RouteType",
                table: "VehicleTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "VehicleTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PlateNumber",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TransferRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "TransferRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ShipmentStatusHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceSource",
                table: "ShipmentStatusHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ShipmentStatusHistories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingCode",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "SenderName",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverName",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverEmail",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverAddress",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryCode",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStatus",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ConsolidationPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "AddedReason",
                table: "ConsolidationPlanItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_BranchId",
                table: "Shipments",
                column: "BranchId");
        }
    }
}
