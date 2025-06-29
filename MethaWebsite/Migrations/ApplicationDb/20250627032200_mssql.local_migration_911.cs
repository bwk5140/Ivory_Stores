using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_911 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Mpesa");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "CreditDebitCard");

            migrationBuilder.AddColumn<string>(
                name: "ImageSource",
                table: "Mpesa",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoSource",
                table: "Mpesa",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSource",
                table: "CreditDebitCard",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoSource",
                table: "CreditDebitCard",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageSource",
                table: "Mpesa");

            migrationBuilder.DropColumn(
                name: "LogoSource",
                table: "Mpesa");

            migrationBuilder.DropColumn(
                name: "ImageSource",
                table: "CreditDebitCard");

            migrationBuilder.DropColumn(
                name: "LogoSource",
                table: "CreditDebitCard");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Mpesa",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "CreditDebitCard",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
