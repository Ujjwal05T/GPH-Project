using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBiltyPhotoFromConsignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiltyPhotoUrl",
                table: "Consignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BiltyPhotoUrl",
                table: "Consignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
