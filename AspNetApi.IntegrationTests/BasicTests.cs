using AspNetApi.Converters;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    public sealed class BasicTests
    {
        private readonly ITestOutputHelper Output;

        public BasicTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public async Task GetWeatherForecast()
        {
            // Arrange
            using var application = new ApplicationBase(Output);
            var client = application.CreateClient();

            // Setup converter
            var httpContextAccessor = application.Services.GetRequiredService<IHttpContextAccessor>();
            var serviceProvdier = application.Services.GetRequiredService<IServiceProvider>();
            var serviceProviderConverter = new ServiceProviderDummyConverter(httpContextAccessor, serviceProvdier);

            // Setup JsonOptions
            var jsonOptions = new JsonSerializerOptions { };
            jsonOptions.Converters.Add(serviceProviderConverter);

            // Act
            var response = await client.GetAsync("/WeatherForecast");

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(jsonOptions);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }
    }
}
