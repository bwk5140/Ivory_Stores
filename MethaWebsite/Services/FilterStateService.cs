using MethaWebsite.Data;

namespace MethaWebsite.Services
{
    public class FilterStateService
    {
        public List<string>? Genders { get; set; } = new List<string>();

        public List<string>? Brands { get; set; } = new List<string>();

        public List<string>? Lifestyle { get; set; } = new List<string>();

        public Dictionary<string, bool>? SelectedLifestyle { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool>? SelectedGenders { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool>? SelectedBrands { get; set; } = new Dictionary<string, bool>();
        public List<Rating>? Rating { get; set; } = new List<Rating>();
        public double Low { get; set; }
        public double High { get; set; } = 3991;
        private FilterStateService? filterStateService { get; set; }

        public FilterStateService getFilterState(List<Product> products)
        {
            if (filterStateService is null)
            {
                filterStateService = new FilterStateService();
                filterStateService.InitializeFilters(products);
            }
            return filterStateService;
        }
        private void InitializeFilters(List<Product> products)
        {
            foreach (var product in products)
            {
                SelectedLifestyle![product.Lifestyle!] = false;
                SelectedGenders![product.Gender!] = false;
                SelectedBrands![product.Brand!] = false;

                if (!Brands.Contains(product.Brand))
                {
                    Brands.Add(product.Brand);
                }
                if (!Lifestyle.Contains(product.Lifestyle))
                {
                    Lifestyle.Add(product.Lifestyle);

                }
                if (!Genders.Contains(product.Gender))
                {
                    Genders.Add(product.Gender);
                }
            }
            High = 3991;
            Low = 1;
            Rating = Enumerable.Range(0, 5)
                .Select(_ => new Rating { Selected = false })
                .ToList();
        }
    }
}
