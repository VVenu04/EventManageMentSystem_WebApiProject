using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace infrastucure.Migrations
{
    /// <inheritdoc />
    public partial class passwordhash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Vendors",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Customers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "AdminPassword",
                table: "Admins",
                newName: "PasswordHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Vendors",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Customers",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Admins",
                newName: "AdminPassword");
        }
    }
}
