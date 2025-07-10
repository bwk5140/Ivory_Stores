using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MethaWebsite.Data;
using MethaWebsite.Data.SizeGuides;

namespace MethaWebsite.Data.Contexts
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<MethaWebsite.Data.ProductGroup> ProductGroup { get; set; } = default!;
        public DbSet<MethaWebsite.Data.ProductColorGroup> ProductColorGroup { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Category> Category { get; set; } = default!;
        public DbSet<MethaWebsite.Data.ProductImage> ProductImage { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Product> Product { get; set; } = default!;
        public DbSet<MethaWebsite.Data.SizeGuides.ShirtsSizeGuide> ShirtsSizeGuide { get; set; } = default!;
        public DbSet<MethaWebsite.Data.SizeGuides.ShoesSizeGuide> ShoesSizeGuide { get; set; } = default!;
        public DbSet<MethaWebsite.Data.SizeGuides.TrousersSizeGuide> TrousersSizeGuide { get; set; } = default!;
        public DbSet<MethaWebsite.Data.ProductList> ProductList { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Address> Address { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Country> Country { get; set; } = default!;
        public DbSet<MethaWebsite.Data.ShoppingCart> ShoppingCart { get; set; } = default!;
        public DbSet<MethaWebsite.Data.CreditDebitCard> CreditDebitCard { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Mpesa> Mpesa { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Shipping> Shipping { get; set; } = default!;
        public DbSet<MethaWebsite.Data.ShoppingCartProduct> ShoppingCartProduct { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Order> Order { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Transaction> Transactions { get; set; } = default!;
        public DbSet<MethaWebsite.Data.ProductReview> ProductReview { get; set; } = default!;
        public DbSet<MethaWebsite.Data.Rating> Rating { get; set; } = default!;
        public DbSet<MethaWebsite.Data.SearchQueryVector> SearchQueryVector { get; set; } = default!;
    }
}
