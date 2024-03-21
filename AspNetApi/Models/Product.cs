using System.Text.Json.Serialization;

namespace AspNetApi.Models
{
    public record Product
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Summary { get; set; }
        public string? ImageFile { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CategoryBase? CategoryInfo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Currency Currency { get; set; }
    }
}
