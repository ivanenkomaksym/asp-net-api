using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    public interface IMyBase
    {
    }

    public class Base1 : IMyBase
    {
        public string Value { get; set; }
    }

    public class Base2 : IMyBase
    {
        public string Value { get; set; }
    }

    public class Implementation
    {
        public IMyBase MyBase { get; set; }
    }

    public sealed class JsonConverterTests
    {
        private readonly ITestOutputHelper Output;

        public JsonConverterTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void DeserializationOfInterfaceNotSupported()
        {
            List<Implementation> values = new()
            {
                new Implementation { MyBase = new Base1 { Value = "Base1Value" }},
                new Implementation { MyBase = new Base2 { Value = "Base2Value" }}
            };

            string json = JsonSerializer.Serialize(values);
            var exception = Assert.Throws<System.NotSupportedException>(() => JsonSerializer.Deserialize<List<Implementation>>(json));
            Assert.Contains("Deserialization of interface types is not supported.", exception.Message);
        }
    }
}
