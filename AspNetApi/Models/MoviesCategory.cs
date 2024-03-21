namespace AspNetApi.Models
{
    public sealed class MoviesCategory : CategoryBase
    {
        public override CategoryType CategoryType => CategoryType.Movies;

        public uint NofMinutes { get; set; }
    }
}
