using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddConsignmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SalesExecutives");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SalesExecutives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Consignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransportCompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BiltyNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DispatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FreightCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consignments_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsignmentId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Consignments_ConsignmentId",
                        column: x => x.ConsignmentId,
                        principalTable: "Consignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_BookId",
                table: "ConsignmentItems",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_ConsignmentId",
                table: "ConsignmentItems",
                column: "ConsignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_SalesExecutiveId",
                table: "Consignments",
                column: "SalesExecutiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsignmentItems");

            migrationBuilder.DropTable(
                name: "Consignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesExecutives");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SalesExecutives",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
