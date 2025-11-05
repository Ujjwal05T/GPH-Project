using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToExecutive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "SalesExecutives");

            migrationBuilder.CreateIndex(
                name: "IX_SalesExecutives_MobileNumber",
                table: "SalesExecutives",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesExecutives_Username",
                table: "SalesExecutives",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesExecutives_MobileNumber",
                table: "SalesExecutives");

            migrationBuilder.DropIndex(
                name: "IX_SalesExecutives_Username",
                table: "SalesExecutives");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "SalesExecutives",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
