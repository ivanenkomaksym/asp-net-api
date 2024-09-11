using System.Text.Json.Serialization;

namespace AspNetApi.Models
{
    [JsonDerivedType(typeof(BooksCategory))]
    [JsonDerivedType(typeof(MoviesCategory))]
    public class CategoryBase
    {
        public virtual CategoryType CategoryType { get; }
    }
}
