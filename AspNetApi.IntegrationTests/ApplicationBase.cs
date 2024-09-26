using AspNetApi.Configuration;
using AspNetApi.Controllers;
using AspNetApi.Converters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.Memory;
using Xunit.Abstractions;

namespace AspNetApi.Tests
{
    internal class ApplicationBase : WebApplicationFactory<Program>
    {
        private readonly ITestOutputHelper Output;
        public ApplicationBase(ITestOutputHelper output, bool useAuthentication = true)
        {
            Output = output;
            UseAuthentication = useAuthentication;
        }

        protected virtual IEnumerable<KeyValuePair<string, string>> GetMemoryConfiguration()
        {
            return
            [
                KeyValuePair.Create(AuthenticationOptions.Name, UseAuthentication.ToString())
            ];
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                var configuration = GetMemoryConfiguration();
                if (configuration == null)
                    return;

                configurationBuilder.Sources.Clear();
                var fromMemory = new MemoryConfigurationSource { InitialData = configuration };
                configurationBuilder.Add(fromMemory);
            }).ConfigureServices(services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(WeatherForecastController).Assembly);

                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.ConfigureOptions<ConfigureJsonOptions>();
                });

            return base.CreateHost(builder);
        }

        private readonly bool UseAuthentication;
    }
}
