using AspNetApi.Configuration;
using AspNetApi.Controllers;
using AspNetApi.Converters;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.Memory;
using Xunit.Abstractions;

namespace AspNetApi.Tests
{
    internal class ApplicationBase : WebApplicationFactory<Program>
    {
        private readonly ITestOutputHelper Output;
        public ApplicationBase(ITestOutputHelper output, bool useAuthentication = true, bool useHttpLogging = false)
        {
            Output = output;
            UseAuthentication = useAuthentication;
            UseHttpLogging = useHttpLogging;
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
                if (UseHttpLogging)
                {
                    services.AddLogging(logging =>
                    {
                        logging.AddProvider(new XunitLoggerProvider(Output)); // Use your custom logger
                    });

                    services.AddHttpLogging(options =>
                    {
                        options.LoggingFields = HttpLoggingFields.RequestPath
                                                | HttpLoggingFields.RequestMethod
                                                | HttpLoggingFields.RequestQuery
                                                | HttpLoggingFields.RequestHeaders
                                                | HttpLoggingFields.RequestBody
                                                | HttpLoggingFields.ResponseHeaders
                                                | HttpLoggingFields.ResponseBody
                                                | HttpLoggingFields.ResponseStatusCode;
                    });
                }

                services.AddControllers().AddApplicationPart(typeof(WeatherForecastController).Assembly);
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.ConfigureOptions<ConfigureJsonOptions>();
            });

            return base.CreateHost(builder);
        }

        private readonly bool UseAuthentication;
        private readonly bool UseHttpLogging;
    }
}
