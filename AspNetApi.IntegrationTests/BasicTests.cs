using AspNetApi.Converters;
using AspNetApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    public class MyOptions
    {
        [MinLength(32, ErrorMessage = "ConnectionString must be at least 32 characters.")]
        [Required]
        public string ConnectionString { get; set; }
        // Other options properties...
    }

    public sealed class BasicTests
    {
        private readonly ITestOutputHelper Output;

        public BasicTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
        
        // in test class
        [Fact]
        public void TestMyOptions()
        {
            var options = new MyOptions
            {
                ConnectionString = "Short"
            };
            var result = ValidateModel(options);

            Assert.True(result.Any(
                v => v.MemberNames.Contains("ConnectionString") &&
                     v.ErrorMessage.Contains("must be at least")));
        }

        [Fact]
        public void OptionsValidation_FailsOnInvalidConfiguration()
        {    
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "MyOptions:ConnectionString", "Short" } // Invalid configuration
                })
                .Build();

            services.AddOptions<MyOptions>()
                .BindConfiguration("MyOptions")
                .ValidateDataAnnotations();

            services.AddSingleton<IConfiguration>(configuration);

            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value);
            Assert.Contains("ConnectionString must be at least 32 characters.", exception.Message);
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

        [Fact]
        public async Task GetProducts()
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
            var response = await client.GetAsync("/api/Products");

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>(jsonOptions);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }

        [Fact]
        public async Task CreateProduct()
        {
            // Arrange
            using var application = new ApplicationBase(Output);
            var client = application.CreateClient();

            // Setup JsonOptions
            var jsonOptions = new JsonSerializerOptions { };

            // Act
            var json = "{ " +
                "\"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\", " +
                "\"name\": \"string\", " +
                "\"category\": \"string\", " +
                "\"summary\": \"string\", " +
                "\"imageFile\": \"string\", " +
                "\"price\": 0, " +
                "\"categoryInfo\": { " +
                    "\"categoryType\": \"Books\", " +
                    "\"nofPages\": 500 " +
                "}, " +
                "\"currency\": \"USD\" " +
            "}";

            using var stringContent = new StringContent(json, EncodingType, "application/json");
            var response = await client.PostAsync("/api/Products", stringContent);

            var result = await response.Content.ReadFromJsonAsync<Product>(jsonOptions);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }

        [Fact]
        public async Task InvalidCategoryType_ShouldThrowJsonException()
        {
            // Arrange
            using var application = new ApplicationBase(Output);
            var client = application.CreateClient();

            // Setup JsonOptions
            var jsonOptions = new JsonSerializerOptions { };

            // Act
            var json = "{ " +
                "\"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\", " +
                "\"name\": \"string\", " +
                "\"category\": \"string\", " +
                "\"summary\": \"string\", " +
                "\"imageFile\": \"string\", " +
                "\"price\": 0, " +
                "\"categoryInfo\": { " +
                    "\"categoryType\": \"InvalidType\" " +
                "}, " +
                "\"currency\": \"USD\" " +
            "}";

            using var stringContent = new StringContent(json, EncodingType, "application/json");
            var response = await client.PostAsync("/api/Products", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var validationError = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.Contains($"Unknown {nameof(Product.CategoryInfo.CategoryType)}: InvalidType.", validationError.Errors["$"]);            
        }

        [Fact]
        public async Task MissingRequiredProperty_ShouldThrowJsonException()
        {
            // Arrange
            using var application = new ApplicationBase(Output);
            var client = application.CreateClient();

            // Setup JsonOptions
            var jsonOptions = new JsonSerializerOptions { };

            // Act
            var json = "{ " +
                "\"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\", " +
                "\"name\": \"string\", " +
                "\"category\": \"string\", " +
                "\"summary\": \"string\", " +
                "\"imageFile\": \"string\", " +
                "\"price\": 0, " +
                "\"categoryInfo\": { " +
                    "\"categoryType\": \"Books\" " +
                "}, " +
                "\"currency\": \"USD\" " +
            "}";

            using var stringContent = new StringContent(json, EncodingType, "application/json");
            var response = await client.PostAsync("/api/Products", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var validationError = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.Contains($"{nameof(BooksCategory)} requires '{StringConverter.ToCamelCaseFromPascal(nameof(BooksCategory.NofPages))}'.", validationError.Errors["$"]);
        }

        [Fact]
        public async Task ExtraUnsupportedProperty_ShouldThrowJsonException()
        {
            // Arrange
            using var application = new ApplicationBase(Output);
            var client = application.CreateClient();

            // Setup JsonOptions
            var jsonOptions = new JsonSerializerOptions { };

            // Act
            var json = "{ " +
                "\"id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\", " +
                "\"name\": \"string\", " +
                "\"category\": \"string\", " +
                "\"summary\": \"string\", " +
                "\"imageFile\": \"string\", " +
                "\"price\": 0, " +
                "\"categoryInfo\": { " +
                    "\"categoryType\": \"Movies\", " +
                    "\"nofPages\": 500, " +
                    "\"nofMinutes\": 120 " +
                "}, " +
                "\"currency\": \"USD\" " +
            "}";

            using var stringContent = new StringContent(json, EncodingType, "application/json");
            var response = await client.PostAsync("/api/Products", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var validationError = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.Contains($"{nameof(MoviesCategory)} contains unsupported properties.", validationError.Errors["$"]);
        }

        private static readonly Encoding EncodingType = Encoding.UTF8;
    }
}
