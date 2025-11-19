using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastucure.Migrations
{
    /// <inheritdoc />
    public partial class addservicephoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Services");

            migrationBuilder.CreateTable(
                name: "ServiceImage",
                columns: table => new
                {
                    ServiceImageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCover = table.Column<bool>(type: "bit", nullable: false),
                    ServiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceImage", x => x.ServiceImageID);
                    table.ForeignKey(
                        name: "FK_ServiceImage_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImage_ServiceID",
                table: "ServiceImage",
                column: "ServiceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceImage");

            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
