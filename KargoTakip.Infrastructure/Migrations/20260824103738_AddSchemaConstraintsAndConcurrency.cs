using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoTakip.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaConstraintsAndConcurrency : Migration
    {
        /// <summary>
        /// Daraltilan metin kolonlari: (tablo, kolon, yeni uzunluk, null olabilir mi).
        /// Onceden her kolon icin sekiz satirlik ayni AlterColumn blogu
        /// tekrarlaniyordu; Up ve Down ile birlikte 54 blok, ~490 satir kopya kod.
        /// Sira ve uretilen sema degismedi, tanim tek yere toplandi.
        /// </summary>
        private static readonly (string Tablo, string Kolon, int Uzunluk, bool NullOlabilir)[] DaraltilanKolonlar =
        {
            ("VehicleTypes", "RouteType", 30, false),
            ("VehicleTypes", "Name", 50, false),
            ("Vehicles", "PlateNumber", 20, false),
            ("Users", "Username", 50, false),
            ("Users", "Role", 20, false),
            ("Users", "PasswordHash", 255, false),
            ("Users", "FullName", 100, false),
            ("TransferRequests", "Status", 20, false),
            ("TransferRequests", "Note", 500, true),
            ("ShipmentStatusHistories", "Status", 30, false),
            ("ShipmentStatusHistories", "ServiceSource", 50, false),
            ("ShipmentStatusHistories", "Note", 500, true),
            ("Shipments", "TrackingCode", 32, false),
            ("Shipments", "SenderName", 100, false),
            ("Shipments", "ReceiverName", 100, false),
            ("Shipments", "ReceiverEmail", 256, true),
            ("Shipments", "ReceiverAddress", 255, false),
            ("Shipments", "Priority", 20, false),
            ("Shipments", "DeliveryCode", 10, true),
            ("Shipments", "CurrentStatus", 30, false),
            ("Notifications", "Message", 500, false),
            ("ConsolidationPlans", "Status", 30, false),
            ("ConsolidationPlanItems", "AddedReason", 50, false),
            ("Cities", "Region", 50, false),
            ("Cities", "Name", 100, false),
            ("Branches", "Name", 100, false),
            ("Branches", "Address", 255, false)
        };

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

            foreach (var (tablo, kolon, uzunluk, nullOlabilir) in DaraltilanKolonlar)
            {
                migrationBuilder.AlterColumn<string>(
                    name: kolon,
                    table: tablo,
                    type: $"nvarchar({uzunluk})",
                    maxLength: uzunluk,
                    nullable: nullOlabilir,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: nullOlabilir);
            }

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Vehicles",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "TransferRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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

            foreach (var (tablo, kolon, uzunluk, nullOlabilir) in DaraltilanKolonlar)
            {
                migrationBuilder.AlterColumn<string>(
                    name: kolon,
                    table: tablo,
                    type: "nvarchar(max)",
                    nullable: nullOlabilir,
                    oldClrType: typeof(string),
                    oldType: $"nvarchar({uzunluk})",
                    oldMaxLength: uzunluk,
                    oldNullable: nullOlabilir);
            }

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_BranchId",
                table: "Shipments",
                column: "BranchId");
        }
    }
}
