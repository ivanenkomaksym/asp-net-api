namespace AspNetApi.Models
{
    public sealed class BooksCategory : CategoryBase
    {
        public override CategoryType CategoryType => CategoryType.Books;

        public uint NofPages { get; set; }
    }
}
