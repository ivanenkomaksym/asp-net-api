using AspNetApi.Controllers;
using AspNetApi.Converters;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    internal class ApplicationBase : WebApplicationFactory<Program>
    {
        private readonly ITestOutputHelper Output;
        public ApplicationBase(ITestOutputHelper output)
        {
            Output = output;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(WeatherForecastController).Assembly);

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.ConfigureOptions<ConfigureJsonOptions>();
            });

            return base.CreateHost(builder);
        }
    }
}
