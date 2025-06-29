using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_708 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryFee",
                table: "Product",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShoppingCartId",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DefaultAddressId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShoppingCardId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Subtotal = table.Column<double>(type: "float", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ShoppingCart_ShoppingCartId",
                table: "Product");

            migrationBuilder.DropTable(
                name: "ShoppingCart");

            migrationBuilder.DropIndex(
                name: "IX_Product_ShoppingCartId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ShoppingCartId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DefaultAddressId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ShoppingCardId",
                table: "AspNetUsers");
        }
    }
}
