using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_493 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_ProductReview_ProductReviewId",
                table: "Rating");

            migrationBuilder.RenameColumn(
                name: "ProductReviewId",
                table: "Rating",
                newName: "ProductReviewID");

            migrationBuilder.RenameIndex(
                name: "IX_Rating_ProductReviewId",
                table: "Rating",
                newName: "IX_Rating_ProductReviewID");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_ProductReview_ProductReviewID",
                table: "Rating",
                column: "ProductReviewID",
                principalTable: "ProductReview",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_ProductReview_ProductReviewID",
                table: "Rating");

            migrationBuilder.RenameColumn(
                name: "ProductReviewID",
                table: "Rating",
                newName: "ProductReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_Rating_ProductReviewID",
                table: "Rating",
                newName: "IX_Rating_ProductReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_ProductReview_ProductReviewId",
                table: "Rating",
                column: "ProductReviewId",
                principalTable: "ProductReview",
                principalColumn: "Id");
        }
    }
}
