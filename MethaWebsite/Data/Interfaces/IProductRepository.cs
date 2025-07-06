namespace MethaWebsite.Data.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetByIds(IEnumerable<string> ids);
    }

}
