using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVisitToSchoolForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Schools_LocationId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_LocationId",
                table: "Visits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Visits_LocationId",
                table: "Visits",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Schools_LocationId",
                table: "Visits",
                column: "LocationId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
