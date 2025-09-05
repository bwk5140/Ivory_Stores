using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MethaWebsite.Migrations
{
    /// <inheritdoc />
    public partial class mssqllocal_migration_482 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Account = table.Column<bool>(type: "bit", nullable: false),
                    ShippingAndDelivery = table.Column<bool>(type: "bit", nullable: false),
                    Deals = table.Column<bool>(type: "bit", nullable: false),
                    SalesEvents = table.Column<bool>(type: "bit", nullable: false),
                    SeasonalAndCurrentTrends = table.Column<bool>(type: "bit", nullable: false),
                    ProductRecommendations = table.Column<bool>(type: "bit", nullable: false),
                    NewReleases = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
