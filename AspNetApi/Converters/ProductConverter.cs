using AspNetApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetApi.Converters
{
    public class ProductConverter : JsonConverter<Product>
    {
        private static readonly string IdString = StringConverter.ToCamelCaseFromPascal(nameof(Product.Id));
        private static readonly string NameString = StringConverter.ToCamelCaseFromPascal(nameof(Product.Name));
        private static readonly string CategoryString = StringConverter.ToCamelCaseFromPascal(nameof(Product.Category));
        private static readonly string SummaryString = StringConverter.ToCamelCaseFromPascal(nameof(Product.Summary));
        private static readonly string ImageFileString = StringConverter.ToCamelCaseFromPascal(nameof(Product.ImageFile));
        private static readonly string PriceString = StringConverter.ToCamelCaseFromPascal(nameof(Product.Price));
        private static readonly string CurrencyString = StringConverter.ToCamelCaseFromPascal(nameof(Product.Currency));
        private static readonly string CategoryInfoString = StringConverter.ToCamelCaseFromPascal(nameof(Product.CategoryInfo));

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
                    if (propertyName.Equals(IdString))
                        product.Id = reader.GetGuid();
                    else if (propertyName.Equals(NameString))
                        product.Name = reader.GetString();
                    else if (propertyName.Equals(CategoryString))
                        product.Category = reader.GetString();
                    else if (propertyName.Equals(SummaryString))
                        product.Summary = reader.GetString();
                    else if (propertyName.Equals(ImageFileString))
                        product.ImageFile = reader.GetString();
                    else if (propertyName.Equals(PriceString))
                        product.Price = reader.GetDecimal();
                    else if (propertyName.Equals(CurrencyString))
                    {
                        var parseResult = Enum.TryParse(reader.GetString(), out Currency currency);
                        if (!parseResult)
                            throw new JsonException($"Failed to parse `{CurrencyString}`.");
                        product.Currency = currency;
                    }
                    else if (propertyName.Equals(CategoryInfoString))
                        product.CategoryInfo = ReadCategoryInfo(ref reader, options);
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
                    throw new JsonException($"Unknown {nameof(Product.CategoryInfo.CategoryType)}: {categoryTypePropString}.");

                switch (categoryType)
                {
                    case CategoryType.Books:
                        ValidateBooksCategory(rootElement);
                        return JsonSerializer.Deserialize<BooksCategory>(rootElement.GetRawText(), options);

                    case CategoryType.Movies:
                        ValidateMoviesCategory(rootElement);
                        return JsonSerializer.Deserialize<MoviesCategory>(rootElement.GetRawText(), options);

                    default:
                        throw new JsonException($"Unknown CategoryType: {categoryType}");
                }
            }
        }

        private static void ValidateMoviesCategory(JsonElement element)
        {
            var propertyName = StringConverter.ToCamelCaseFromPascal(nameof(MoviesCategory.NofMinutes));
            // Check for required properties
            if (!element.TryGetProperty(propertyName, out _))
            {
                throw new JsonException($"{nameof(MoviesCategory)} requires '{propertyName}'.");
            }

            // Check for any unsupported properties
            if (!HasExactNumberOfProperties(element, 1))
            {
                throw new JsonException($"{nameof(MoviesCategory)} contains unsupported properties.");
            }
        }

        private static void ValidateBooksCategory(JsonElement element)
        {
            var propertyName = StringConverter.ToCamelCaseFromPascal(nameof(BooksCategory.NofPages));
            // Check for required properties
            if (!element.TryGetProperty(propertyName, out _))
            {
                throw new JsonException($"{nameof(BooksCategory)} requires '{propertyName}'.");
            }

            // Check for any unsupported properties
            if (!HasExactNumberOfProperties(element, 2))
            {
                throw new JsonException($"{nameof(BooksCategory)} contains unsupported properties.");
            }
        }
        
        private static bool HasExactNumberOfProperties(JsonElement element, ushort nofProperties)
        {
            return element.ValueKind == JsonValueKind.Object && element.EnumerateObject().Count() == nofProperties;
        }

        public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(IdString, value.Id);
            writer.WriteString(NameString, value.Name);
            writer.WriteString(CategoryString, value.Category);
            writer.WriteString(SummaryString, value.Summary);
            writer.WriteString(ImageFileString, value.ImageFile);
            writer.WriteNumber(PriceString, value.Price);
            writer.WriteString(CurrencyString, value.Currency.ToString());

            if (value.CategoryInfo != null)
            {
                writer.WritePropertyName(CategoryInfoString);
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
