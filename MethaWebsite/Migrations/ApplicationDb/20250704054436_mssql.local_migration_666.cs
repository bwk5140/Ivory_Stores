using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_666 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "CreditDebitCard");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Mpesa",
                newName: "Number");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Transactions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "CreditDebitCard",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CreditDebitCard");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Mpesa",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "CreditDebitCard",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
