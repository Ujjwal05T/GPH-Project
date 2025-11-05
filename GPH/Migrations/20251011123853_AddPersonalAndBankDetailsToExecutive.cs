using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalAndBankDetailsToExecutive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AadharNumber",
                table: "SalesExecutives",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "SalesExecutives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "SalesExecutives",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "SalesExecutives",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranch",
                table: "SalesExecutives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "SalesExecutives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "SalesExecutives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IfscCode",
                table: "SalesExecutives",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "SalesExecutives",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AadharNumber",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "BankBranch",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "IfscCode",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "SalesExecutives");
        }
    }
}
