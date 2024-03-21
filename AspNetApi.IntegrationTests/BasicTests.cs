using AspNetApi.Converters;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
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
    }
}
