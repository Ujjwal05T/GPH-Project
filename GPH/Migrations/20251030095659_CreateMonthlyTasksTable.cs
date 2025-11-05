using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class CreateMonthlyTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    AssignedMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyTasks_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyTasks_SalesExecutiveId",
                table: "MonthlyTasks",
                column: "SalesExecutiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyTasks");
        }
    }
}
