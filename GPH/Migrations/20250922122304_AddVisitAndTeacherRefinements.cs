using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitAndTeacherRefinements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Visits",
                type: "float",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Visits",
                type: "float",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "PermissionToMeetTeachers",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrincipalRemarks",
                table: "Visits",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerifiedByExecutive",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VisitDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitDetails_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitDetails_VisitId",
                table: "VisitDetails",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitDetails");

            migrationBuilder.DropColumn(
                name: "PermissionToMeetTeachers",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "PrincipalRemarks",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsVerifiedByExecutive",
                table: "Teachers");

            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                table: "Visits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Visits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldMaxLength: 50);
        }
    }
}
