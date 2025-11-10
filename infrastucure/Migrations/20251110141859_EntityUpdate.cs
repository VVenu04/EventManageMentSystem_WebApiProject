using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastucure.Migrations
{
    /// <inheritdoc />
    public partial class EntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Packages_PackageID",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Services_ServiceID",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Events_EventID",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Tracking_BookingID",
                table: "Tracking");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PackageID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ServiceID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PackageID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "Payment",
                table: "Packages",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Bookings",
                newName: "TotalPrice");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventID",
                table: "Services",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BookingStatus",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingItem",
                columns: table => new
                {
                    BookingItemID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackageID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VendorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrackingStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItem", x => x.BookingItemID);
                    table.ForeignKey(
                        name: "FK_BookingItem_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingItem_Packages_PackageID",
                        column: x => x.PackageID,
                        principalTable: "Packages",
                        principalColumn: "PackageID");
                    table.ForeignKey(
                        name: "FK_BookingItem_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID");
                    table.ForeignKey(
                        name: "FK_BookingItem_Vendors_VendorID",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tracking_BookingID",
                table: "Tracking",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItem_BookingID",
                table: "BookingItem",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItem_PackageID",
                table: "BookingItem",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItem_ServiceID",
                table: "BookingItem",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItem_VendorID",
                table: "BookingItem",
                column: "VendorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Events_EventID",
                table: "Services",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "EventID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Events_EventID",
                table: "Services");

            migrationBuilder.DropTable(
                name: "BookingItem");

            migrationBuilder.DropIndex(
                name: "IX_Tracking_BookingID",
                table: "Tracking");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "BookingStatus",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Packages",
                newName: "Payment");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Bookings",
                newName: "Price");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventID",
                table: "Services",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackageID",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceID",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tracking_BookingID",
                table: "Tracking",
                column: "BookingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PackageID",
                table: "Bookings",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ServiceID",
                table: "Bookings",
                column: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Packages_PackageID",
                table: "Bookings",
                column: "PackageID",
                principalTable: "Packages",
                principalColumn: "PackageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Services_ServiceID",
                table: "Bookings",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ServiceID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Events_EventID",
                table: "Services",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "EventID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
