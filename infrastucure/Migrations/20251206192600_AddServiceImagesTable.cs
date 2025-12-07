using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImage_ServiceItems_ServiceItemID",
                table: "ServiceImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceImage",
                table: "ServiceImage");

            migrationBuilder.RenameTable(
                name: "ServiceImage",
                newName: "ServiceImages");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceImage_ServiceItemID",
                table: "ServiceImages",
                newName: "IX_ServiceImages_ServiceItemID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceImages",
                table: "ServiceImages",
                column: "ServiceImageID");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceImages_ServiceItems_ServiceItemID",
                table: "ServiceImages",
                column: "ServiceItemID",
                principalTable: "ServiceItems",
                principalColumn: "ServiceItemID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImages_ServiceItems_ServiceItemID",
                table: "ServiceImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceImages",
                table: "ServiceImages");

            migrationBuilder.RenameTable(
                name: "ServiceImages",
                newName: "ServiceImage");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceImages_ServiceItemID",
                table: "ServiceImage",
                newName: "IX_ServiceImage_ServiceItemID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceImage",
                table: "ServiceImage",
                column: "ServiceImageID");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceImage_ServiceItems_ServiceItemID",
                table: "ServiceImage",
                column: "ServiceItemID",
                principalTable: "ServiceItems",
                principalColumn: "ServiceItemID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
