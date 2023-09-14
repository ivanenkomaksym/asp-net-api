namespace AspNetApi.Models
{
    public record ShoppingCartItem
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }

        public string? ProductName { get; init; }
        public decimal ProductPrice { get; init; }
        public ushort Quantity { get; init; }
        public string? ImageFile { get; set; }
    }
}
