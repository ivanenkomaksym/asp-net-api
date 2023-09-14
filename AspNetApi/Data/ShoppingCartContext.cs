using AspNetApi.Models;

namespace AspNetApi.Data
{
    public class ShoppingCartContext : IShoppingCartContext
    {
        public ShoppingCartContext(IProductContext productContext)
        {
            ShoppingCarts = new List<ShoppingCart>();

            var product = productContext.Products.FirstOrDefault();
            ShoppingCarts.Add(new ShoppingCart
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerName = "Alice",
                Items = new[]
                {
                    new ShoppingCartItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductPrice = product.Price,
                        Quantity = 1,
                        ImageFile = product.ImageFile
                    }
                }
            });
        }

        public List<ShoppingCart> ShoppingCarts { get; }
    }
}
