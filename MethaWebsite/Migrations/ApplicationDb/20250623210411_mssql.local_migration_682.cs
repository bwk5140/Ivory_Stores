using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_682 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductListId",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductList",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Private = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductList", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ProductList_ProductListId",
                table: "Product");

            migrationBuilder.DropTable(
                name: "ProductList");

            migrationBuilder.DropIndex(
                name: "IX_Product_ProductListId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductListId",
                table: "Product");
        }
    }
}
