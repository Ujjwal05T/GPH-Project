using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddBeatPlanEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeatPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    PlanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeatPlans_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatPlans_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeatPlans_SalesExecutiveId",
                table: "BeatPlans",
                column: "SalesExecutiveId");

            migrationBuilder.CreateIndex(
                name: "IX_BeatPlans_SchoolId",
                table: "BeatPlans",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatPlans");
        }
    }
}
