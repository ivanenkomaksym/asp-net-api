using AspNetApi.Tests;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests.IntegrationTests
{
    public sealed class HealthCheckTests(ITestOutputHelper output)
    {
        [Fact]
        public async Task HealthCheck_WithoutSecretHeader_ShouldFail()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            // Act
            var response = await client.GetAsync("/healthz");

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Authorization failed: Invalid secret header value.", result);
        }

        [Fact]
        public async Task HealthCheck_WithIncorrectValue_ShouldFail()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            client.DefaultRequestHeaders.Add("secret_header", "incorrect_value");

            // Act
            var response = await client.GetAsync("/healthz");

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Authorization failed: Invalid secret header value.", result);
        }

        [Fact]
        public async Task HealthCheck_WithSecretHeaderAndWithoutMinimumAge_ShouldFail()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            client.DefaultRequestHeaders.Add("secret_header", "expected_value");

            // Act
            var response = await client.GetAsync("/healthz");

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Authorization failed: Missing age.", result);
        }

        [Fact]
        public async Task HealthCheck_WithSecretHeaderAndWithIncorrectAgeFormat_ShouldFail()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            client.DefaultRequestHeaders.Add("secret_header", "expected_value");
            client.DefaultRequestHeaders.Add("age", "incorrect");

            // Act
            var response = await client.GetAsync("/healthz");

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Authorization failed: Incorrect age format.", result);
        }

        [Fact]
        public async Task HealthCheck_WithSecretHeaderAndWithUnderage_ShouldFail()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            client.DefaultRequestHeaders.Add("secret_header", "expected_value");
            client.DefaultRequestHeaders.Add("age", "16");

            // Act
            var response = await client.GetAsync("/healthz");

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Authorization failed: Underage.", result);
        }

        [Fact]
        public async Task HealthCheck_WithSecretHeaderAndMinimumAge_ShouldSucceed()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            client.DefaultRequestHeaders.Add("secret_header", "expected_value");
            client.DefaultRequestHeaders.Add("age", "18");

            // Act
            var response = await client.GetAsync("/healthz");

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", result);
        }
    }
}
