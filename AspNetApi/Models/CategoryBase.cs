using System.Text.Json.Serialization;

namespace AspNetApi.Models
{
    [JsonDerivedType(typeof(BooksCategory))]
    [JsonDerivedType(typeof(MoviesCategory))]
    public abstract class CategoryBase
    {
        abstract public CategoryType CategoryType { get; }
    }
}
