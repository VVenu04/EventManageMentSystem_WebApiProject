using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastucure.Migrations
{
    /// <inheritdoc />
    public partial class Updateentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItem_Bookings_BookingID",
                table: "BookingItem");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingItem_Packages_PackageID",
                table: "BookingItem");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingItem_Services_ServiceID",
                table: "BookingItem");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingItem_Vendors_VendorID",
                table: "BookingItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingItem",
                table: "BookingItem");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "BookingItem",
                newName: "BookingItems");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Bookings",
                newName: "EventDate");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItem_VendorID",
                table: "BookingItems",
                newName: "IX_BookingItems_VendorID");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItem_ServiceID",
                table: "BookingItems",
                newName: "IX_BookingItems_ServiceID");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItem_PackageID",
                table: "BookingItems",
                newName: "IX_BookingItems_PackageID");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItem_BookingID",
                table: "BookingItems",
                newName: "IX_BookingItems_BookingID");

            migrationBuilder.AddColumn<decimal>(
                name: "EventPerDayLimit",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TimeLimit",
                table: "Services",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingItems",
                table: "BookingItems",
                column: "BookingItemID");

            migrationBuilder.CreateTable(
                name: "PackageItems",
                columns: table => new
                {
                    PackageItemID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageItems", x => x.PackageItemID);
                    table.ForeignKey(
                        name: "FK_PackageItems_Packages_PackageID",
                        column: x => x.PackageID,
                        principalTable: "Packages",
                        principalColumn: "PackageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageItems_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_PackageID",
                table: "PackageItems",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_ServiceID",
                table: "PackageItems",
                column: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_Bookings_BookingID",
                table: "BookingItems",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_Packages_PackageID",
                table: "BookingItems",
                column: "PackageID",
                principalTable: "Packages",
                principalColumn: "PackageID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_Services_ServiceID",
                table: "BookingItems",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_Vendors_VendorID",
                table: "BookingItems",
                column: "VendorID",
                principalTable: "Vendors",
                principalColumn: "VendorID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_Bookings_BookingID",
                table: "BookingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_Packages_PackageID",
                table: "BookingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_Services_ServiceID",
                table: "BookingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_Vendors_VendorID",
                table: "BookingItems");

            migrationBuilder.DropTable(
                name: "PackageItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingItems",
                table: "BookingItems");

            migrationBuilder.DropColumn(
                name: "EventPerDayLimit",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "TimeLimit",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "BookingItems",
                newName: "BookingItem");

            migrationBuilder.RenameColumn(
                name: "EventDate",
                table: "Bookings",
                newName: "StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItems_VendorID",
                table: "BookingItem",
                newName: "IX_BookingItem_VendorID");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItems_ServiceID",
                table: "BookingItem",
                newName: "IX_BookingItem_ServiceID");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItems_PackageID",
                table: "BookingItem",
                newName: "IX_BookingItem_PackageID");

            migrationBuilder.RenameIndex(
                name: "IX_BookingItems_BookingID",
                table: "BookingItem",
                newName: "IX_BookingItem_BookingID");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingItem",
                table: "BookingItem",
                column: "BookingItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItem_Bookings_BookingID",
                table: "BookingItem",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItem_Packages_PackageID",
                table: "BookingItem",
                column: "PackageID",
                principalTable: "Packages",
                principalColumn: "PackageID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItem_Services_ServiceID",
                table: "BookingItem",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItem_Vendors_VendorID",
                table: "BookingItem",
                column: "VendorID",
                principalTable: "Vendors",
                principalColumn: "VendorID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
