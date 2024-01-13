using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetApi.Converters
{
    public class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
    {
        public ConfigureJsonOptions(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            HttpContextAccessor = httpContextAccessor;
            ServiceProvider = serviceProvider;
        }

        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly IServiceProvider ServiceProvider;

        public void Configure(JsonOptions options)
        {
            options.JsonSerializerOptions.Converters.Add(new ServiceProviderDummyConverter(HttpContextAccessor, ServiceProvider));
        }
    }
}
