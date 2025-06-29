using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_995 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DefaultPaymentMethod",
                table: "Mpesa",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DefaultPaymentMethod",
                table: "CreditDebitCard",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPaymentMethod",
                table: "Mpesa");

            migrationBuilder.DropColumn(
                name: "DefaultPaymentMethod",
                table: "CreditDebitCard");
        }
    }
}
