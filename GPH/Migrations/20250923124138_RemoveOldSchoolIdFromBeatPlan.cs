using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldSchoolIdFromBeatPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BeatPlans_Schools_SchoolId",
                table: "BeatPlans");

            migrationBuilder.DropIndex(
                name: "IX_BeatPlans_SchoolId",
                table: "BeatPlans");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "BeatPlans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "BeatPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BeatPlans_SchoolId",
                table: "BeatPlans",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_BeatPlans_Schools_SchoolId",
                table: "BeatPlans",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
