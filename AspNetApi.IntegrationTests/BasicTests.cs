using Microsoft.AspNetCore.Mvc.Testing;
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

            // Act
            var response = await client.GetAsync("/WeatherForecast");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }
    }
}
