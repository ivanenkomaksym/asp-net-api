using AspNetApi.Models;

namespace AspNetApi.Data
{
    public class IProductContext
    {
        public virtual IEnumerable<Product> Products { get; }
    }
}
