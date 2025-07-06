using MethaWebsite.Data;
using MethaWebsite.Data.Contexts;
using MethaWebsite.Data.Interfaces;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Product> GetByIds(IEnumerable<string> ids)
    {
        return _db.Product.Where(p => ids.Contains(p.Id)).ToList();
    }
}