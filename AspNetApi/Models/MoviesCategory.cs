namespace AspNetApi.Models
{
    public sealed class MoviesCategory : CategoryBase
    {
        public override CategoryType CategoryType => CategoryType.Movies;

        public required uint NofMinutes { get; set; }
    }
}
