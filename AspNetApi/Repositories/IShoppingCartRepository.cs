using AspNetApi.Models;

namespace AspNetApi.Repositories
{
    public interface IShoppingCartRepository
    {
        Task<List<ShoppingCart>> GetShoppingCarts();
        Task<ShoppingCart> GetShoppingCart(Guid customerId);

        Task CreateShoppingCart(ShoppingCart shoppingCart);
        Task<ShoppingCart> UpdateShoppingCart(ShoppingCart shoppingCart);
        Task<bool> DeleteShoppingCart(Guid customerId);

        Task<bool> Checkout(Guid customerId);
    }
}
