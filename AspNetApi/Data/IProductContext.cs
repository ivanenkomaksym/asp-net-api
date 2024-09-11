using AspNetApi.Models;

namespace AspNetApi.Data
{
    public class IProductContext
    {
        public virtual List<Product> Products { get; }
    }
}
