using System.Text.Json;

namespace AspNetApi.Converters
{
    public static class JsonConverterExtensions
    {
        public static IServiceProvider GetServiceProvider(this JsonSerializerOptions options)
        {
            return options.Converters.OfType<IServiceProvider>().FirstOrDefault()
                ?? throw new InvalidOperationException("No service provider found in JSON converters");
        }
    }
}
