using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetApi.Converters
{
    public class WeatherForecastConverter : JsonConverter<WeatherForecast>
    {
        public override WeatherForecast? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TestGetService(options);

            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("Expected the JSON data to start with an object.");
                }

                DateTime? date = null;
                int temperatureC = 0;
                string? summary = null;

                foreach (var property in root.EnumerateObject())
                {
                    switch (property.Name)
                    {
                        case "Date":
                            date = JsonSerializer.Deserialize<DateTime>(property.Value.GetRawText(), options);
                            break;
                        case "TemperatureC":
                            temperatureC = property.Value.GetInt32();
                            break;
                        case "Summary":
                            summary = JsonSerializer.Deserialize<string?>(property.Value.GetRawText(), options);
                            break;
                        // Add more cases for other properties if needed
                        default:
                            // Ignore unknown properties
                            break;
                    }
                }

                if (!date.HasValue)
                {
                    throw new JsonException("Missing 'Date' property in JSON data.");
                }

                return new WeatherForecast
                {
                    Date = new DateOnly(date.Value.Year, date.Value.Month, date.Value.Day),
                    TemperatureC = temperatureC,
                    Summary = summary
                    // You may need to add other properties here based on your actual class
                };
            }
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
