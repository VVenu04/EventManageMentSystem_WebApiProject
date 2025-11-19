using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastucure.Migrations
{
    /// <inheritdoc />
    public partial class Addbookinggg : Migration
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

            migrationBuilder.RenameTable(
                name: "BookingItem",
                newName: "BookingItems");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingItems",
                table: "BookingItems",
                column: "BookingItemID");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingItems",
                table: "BookingItems");

            migrationBuilder.DropColumn(
                name: "EventPerDayLimit",
                table: "Services");

            migrationBuilder.RenameTable(
                name: "BookingItems",
                newName: "BookingItem");

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
