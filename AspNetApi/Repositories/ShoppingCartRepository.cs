using AspNetApi.Data;
using AspNetApi.Models;

namespace AspNetApi.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly IShoppingCartContext _context;

        public ShoppingCartRepository(IShoppingCartContext context)
        {
            _context = context;
        }

        public Task<bool> Checkout(Guid customerId)
        {
            throw new NotImplementedException();
        }

        public Task CreateShoppingCart(ShoppingCart shoppingCart)
        {
            _context.ShoppingCarts.Add(shoppingCart);
            return Task.CompletedTask;
        }

        public Task<bool> DeleteShoppingCart(Guid customerId)
        {
            var result = _context.ShoppingCarts.RemoveAll(cart => cart.CustomerId == customerId);

            return Task.FromResult(result != 0);
        }

        public Task<List<ShoppingCart>> GetShoppingCarts()
        {
            return Task.FromResult(_context.ShoppingCarts);
        }

        public Task<ShoppingCart> GetShoppingCart(Guid customerId)
        {
            var result = _context.ShoppingCarts.FirstOrDefault(cart => cart.CustomerId == customerId);
            return Task.FromResult(result);
        }

        public Task<ShoppingCart> UpdateShoppingCart(ShoppingCart shoppingCart)
        {
            var index = _context.ShoppingCarts.FindIndex(cart => cart.CustomerId == shoppingCart.CustomerId);

            if (index == -1)
                throw new ArgumentOutOfRangeException();

            var newVersionNr = (ushort)(_context.ShoppingCarts[index].Version.Number + 1);

            _context.ShoppingCarts[index] = shoppingCart;
            _context.ShoppingCarts[index].Version.Number = newVersionNr;

            return Task.FromResult(shoppingCart);
        }
    }
}
