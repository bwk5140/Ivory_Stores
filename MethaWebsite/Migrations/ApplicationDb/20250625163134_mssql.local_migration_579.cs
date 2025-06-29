using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_579 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ShoppingCart_ShoppingCartId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ShoppingCartId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ShoppingCartId",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "ProductIds",
                table: "ShoppingCart",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductIds",
                table: "ShoppingCart");

            migrationBuilder.AddColumn<string>(
                name: "ShoppingCartId",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ShoppingCartId",
                table: "Product",
                column: "ShoppingCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ShoppingCart_ShoppingCartId",
                table: "Product",
                column: "ShoppingCartId",
                principalTable: "ShoppingCart",
                principalColumn: "Id");
        }
    }
}
