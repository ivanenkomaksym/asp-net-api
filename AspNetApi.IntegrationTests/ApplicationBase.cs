using AspNetApi.Controllers;
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
            });

            return base.CreateHost(builder);
        }
    }
}
