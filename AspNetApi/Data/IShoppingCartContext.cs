using AspNetApi.Models;

namespace AspNetApi.Data
{
    public interface IShoppingCartContext
    {
        public List<ShoppingCart> ShoppingCarts { get; }
    }
}
