using AspNetApi.Models;
using AspNetApi.Tests;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    public class ProductValidationFilterIntegrationTests(ITestOutputHelper output)
    {
        [Fact]
        public async Task Should_Return_BadRequest_With_ValidationErrors_When_Product_Is_Invalid()
        {
            // Arrange
            using var application = new ApplicationBase(output);
            var client = application.CreateClient();

            // Create an invalid Product with empty Authors in the BooksCategory
            var invalidProduct = new
            {
                Id = Guid.NewGuid(),
                Name = "Sample Product",
                Category = "Books",
                CategoryInfo = new BooksCategory
                {
                    NofPages = 500,
                    Authors = new List<string>() // Invalid empty authors list (should trigger [MinLength(1)] validation error)
                },
                Price = 25.99M
            };

            // Serialize the product object into JSON
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var serializedProduct = JsonSerializer.Serialize(invalidProduct, options);
            var requestContent = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

            // Act: Send POST request to the product creation endpoint
            var response = await client.PostAsync("/api/products", requestContent);

            // Assert: Check that the response is a 400 Bad Request
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Deserialize the response content into ValidationProblemDetails
            var responseContent = await response.Content.ReadAsStringAsync();
            var validationErrors = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Assert: Ensure that the validation error messages are returned for the correct fields
            Assert.NotNull(validationErrors);

            Assert.True(validationErrors.Errors.ContainsKey("Authors"), "Expected validation error for 'Authors'.");
            Assert.Contains("The field Authors must be a string or array type with a minimum length of '1'.", validationErrors.Errors["Authors"]);
        }
    }
}
