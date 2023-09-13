using AspNetApi.Models;

namespace AspNetApi.Data
{
    public class ProductContext : IProductContext
    {
        public ProductContext()
        {
            Products = ProductContextSeed.GetPreconfiguredProducts();
        }

        public override IEnumerable<Product> Products { get; }
    }
}
