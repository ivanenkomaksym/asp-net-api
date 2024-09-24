namespace AspNetApi.Configuration
{
    public record AuthenticationOptions
    {
        public const string Name = nameof(AuthenticationOptions);

        public bool Enable { get; set; }
    }
}
