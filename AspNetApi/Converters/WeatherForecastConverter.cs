using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetApi.Converters
{
    public class WeatherForecastConverter : JsonConverter<WeatherForecast>
    {
        public override WeatherForecast? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TestGetService(options);

            var weatherForecast = JsonDocument.ParseValue(ref reader).Deserialize<WeatherForecast>();
            return weatherForecast;
        }

        public override void Write(Utf8JsonWriter writer, WeatherForecast value, JsonSerializerOptions options)
        {
            TestGetService(options);

            writer.WriteStartObject();

            writer.WriteString("Date", value.Date.ToString("yyyy-MM-dd"));
            writer.WriteNumber("TemperatureC", value.TemperatureC);
            writer.WriteNumber("TemperatureF", value.TemperatureF);
            writer.WriteString("Summary", value.Summary);

            writer.WriteEndObject();
        }

        private void TestGetService(JsonSerializerOptions options)
        {
            var serviceProvider = options.GetServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<WeatherForecastConverter>>();

            logger.LogInformation("Inject a service into a System.Text.Json converter");
        }
    }
}
