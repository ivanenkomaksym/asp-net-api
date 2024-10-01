using AspNetApi.Models;
using AspNetApi.Tests;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    /// <summary>
    /// 3 ways to do integration test for error handling:
    /// 1. Using <see cref="WebApplicationFactory"/> implementation for existing <see cref="Program"/>.
    /// 2. Mocking <see cref="HttpMessageHandler"/>.
    /// 3. Using in-memory test server with minimal API and filter applied.
    /// </summary>
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

        public class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync(request, cancellationToken);
            }
        }

        [Fact]
        public async Task Should_Return_Mocked_BadRequestResponse_With_NSubstitute()
        {
            // Arrange: Create the mock response with validation error details
            var errorDetails = new ValidationProblemDetails
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Errors = new Dictionary<string, string[]>
                {
                    { "NofPages", new[] { "The field NofPages must be greater than 0." } },
                    { "Authors", new[] { "The Authors field must have at least 1 element." } }
                }
            };

            // Serialize the mock response content
            var mockResponseContent = new StringContent(JsonSerializer.Serialize(errorDetails), Encoding.UTF8, "application/json");

            var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = mockResponseContent
                });
            });

            // Create HttpClient using the mocked handler
            var client = new HttpClient(mockHandler)
            {
                // Set the BaseAddress to a valid URI
                BaseAddress = new Uri("http://localhost")
            };

            // Act: Send a POST request to the product endpoint
            var response = await client.PostAsync("/api/products", new StringContent("{}"));

            // Assert: Check if the response is a BadRequest and contains the expected error details
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var validationErrors = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Check if the validation errors match what we mocked
            Assert.NotNull(validationErrors);
            Assert.Contains("NofPages", validationErrors.Errors.Keys);
            Assert.Contains("The field NofPages must be greater than 0.", validationErrors.Errors["NofPages"]);

            Assert.Contains("Authors", validationErrors.Errors.Keys);
            Assert.Contains("The Authors field must have at least 1 element.", validationErrors.Errors["Authors"]);
        }

        [Fact]
        public async Task Should_Return_BadRequest_With_ValidationError()
        {
            // Arrange
            using var client = CreateClientWithDummyActionAndFilter();

            // Act: Send a POST request to the test route
            var response = await client.PostAsync("/test", null);

            // Assert: Ensure the response is BadRequest
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Deserialize the validation error response
            var content = await response.Content.ReadAsStringAsync();
            var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Assert: Verify the error details
            Assert.NotNull(validationProblemDetails);
            Assert.Equal("Validation failed", validationProblemDetails.Title);
            Assert.Contains("Field", validationProblemDetails.Errors.Keys);
            Assert.Contains("This field is required.", validationProblemDetails.Errors["Field"]);
        }

        private static HttpClient CreateClientWithDummyActionAndFilter()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Set up a simple dummy route for testing
                        endpoints.MapPost("/test", async context =>
                        {
                            await context.Response.WriteAsync("Dummy action");
                        })
                        .AddEndpointFilter(async (invocationContext, next) =>
                        {
                            var errorResponse = new ValidationProblemDetails
                            {
                                Title = "Validation failed",
                                Status = StatusCodes.Status400BadRequest,
                                Errors = { { "Field", new[] { "This field is required." } } }
                            };

                            return Results.BadRequest(errorResponse);
                        });
                    });
                });

            var server = new TestServer(builder);
            return server.CreateClient();
        }
    }
}
