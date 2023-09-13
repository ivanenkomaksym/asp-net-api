using AspNetApi.Data;
using AspNetApi.Models;

namespace AspNetApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IProductContext _productContext;

        public ProductRepository(IProductContext productContext)
        {
            _productContext = productContext;
        }

        public Task CreateProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteProduct(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Product> GetProduct(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProductByCategory(string categoryName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProductByName(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProducts()
        {
            return Task.FromResult(_productContext.Products);
        }

        public Task<bool> UpdateProduct(Product product)
        {
            throw new NotImplementedException();
        }
    }
}
