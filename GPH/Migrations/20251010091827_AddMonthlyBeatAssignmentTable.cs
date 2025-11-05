using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyBeatAssignmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyBeatAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    AssignedMonth = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyBeatAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyBeatAssignments_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBeatAssignments_SalesExecutiveId",
                table: "MonthlyBeatAssignments",
                column: "SalesExecutiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyBeatAssignments");
        }
    }
}
