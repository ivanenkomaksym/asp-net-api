namespace AspNetApi.Models
{
    public class ShoppingCart
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string? CustomerName { get; init; }
        public IEnumerable<ShoppingCartItem> Items { get; init; } = new List<ShoppingCartItem>();
        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    totalPrice += item.ProductPrice * item.Quantity;
                }
                return totalPrice;
            }
            init
            {

            }
        }
    }
}
