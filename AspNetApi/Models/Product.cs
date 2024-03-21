﻿namespace AspNetApi.Models
{
    public record Product
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Summary { get; set; }
        public string? ImageFile { get; set; }
        public decimal Price { get; set; }

        public CategoryBase? CategoryInfo { get; set; }
    }
}
