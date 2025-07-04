using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_451 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ProductList_ProductListId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ProductListId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductListId",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "ProductIds",
                table: "ProductList",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductIds",
                table: "ProductList");

            migrationBuilder.AddColumn<string>(
                name: "ProductListId",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductListId",
                table: "Product",
                column: "ProductListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ProductList_ProductListId",
                table: "Product",
                column: "ProductListId",
                principalTable: "ProductList",
                principalColumn: "Id");
        }
    }
}
