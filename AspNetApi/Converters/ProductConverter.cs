using AspNetApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetApi.Converters
{
    public class ProductConverter : JsonConverter<Product>
    {
        public override Product? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject token");

            var product = new Product();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return product;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();

                    reader.Read();
                    switch (propertyName)
                    {
                        case "id":
                            product.Id = reader.GetGuid();
                            break;
                        case "name":
                            product.Name = reader.GetString();
                            break;
                        case "category":
                            product.Category = reader.GetString();
                            break;
                        case "summary":
                            product.Summary = reader.GetString();
                            break;
                        case "imageFile":
                            product.ImageFile = reader.GetString();
                            break;
                        case "price":
                            product.Price = reader.GetDecimal();
                            break;
                        case "currency":
                            Enum.TryParse(reader.GetString(), out Currency currency);
                            product.Currency = currency;
                            break;
                        case "categoryInfo":
                            product.CategoryInfo = ReadCategoryInfo(ref reader, options);
                            break;
                    }
                }
            }
            throw new JsonException("Invalid JSON structure");
        }

        private static CategoryBase? ReadCategoryInfo(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                var rootElement = document.RootElement;

                if (!rootElement.TryGetProperty("categoryType", out var categoryTypeProp))
                    throw new JsonException("Missing CategoryType");

                var categoryTypePropString = categoryTypeProp.GetString();
                var parseResult = Enum.TryParse(categoryTypePropString, out CategoryType categoryType);
                if (!parseResult)
                    throw new JsonException($"Unknown CategoryType: {categoryTypePropString}");

                return categoryType switch
                {
                    CategoryType.Books => JsonSerializer.Deserialize<BooksCategory>(rootElement.GetRawText(), options),
                    CategoryType.Movies => JsonSerializer.Deserialize<MoviesCategory>(rootElement.GetRawText(), options),
                    _ => throw new JsonException($"Unknown CategoryType: {categoryType}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("id", value.Id);
            writer.WriteString("name", value.Name);
            writer.WriteString("category", value.Category);
            writer.WriteString("summary", value.Summary);
            writer.WriteString("imageFile", value.ImageFile);
            writer.WriteNumber("price", value.Price);
            writer.WriteString("currency", value.Currency.ToString());

            if (value.CategoryInfo != null)
            {
                writer.WritePropertyName("categoryInfo");
                WriteCategoryInfo(writer, value.CategoryInfo, options);
            }

            writer.WriteEndObject();
        }

        private void WriteCategoryInfo(Utf8JsonWriter writer, CategoryBase categoryInfo, JsonSerializerOptions options)
        {
            switch (categoryInfo)
            {
                case BooksCategory booksCategory:
                    JsonSerializer.Serialize(writer, booksCategory, options);
                    break;
                case MoviesCategory moviesCategory:
                    JsonSerializer.Serialize(writer, moviesCategory, options);
                    break;
                default:
                    throw new JsonException($"Unknown CategoryInfo type: {categoryInfo.GetType()}");
            }
        }
    }
}
