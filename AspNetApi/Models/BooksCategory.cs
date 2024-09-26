using System.ComponentModel.DataAnnotations;

namespace AspNetApi.Models
{
    public sealed class BooksCategory : CategoryBase
    {
        public override CategoryType CategoryType => CategoryType.Books;

        public required uint NofPages { get; set; }

        [MinLength(1)]
        public required IEnumerable<string> Authors { get; set; }
    }
}
