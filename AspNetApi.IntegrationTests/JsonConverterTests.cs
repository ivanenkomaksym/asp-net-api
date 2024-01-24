using System.Text.Json;
using System.Text.Json.Serialization;
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

    [JsonDerivedType(typeof(Derived1), typeDiscriminator: "Derived1")]
    [JsonDerivedType(typeof(Derived2), typeDiscriminator: "Derived2")]
    public record BaseType(int Id);

    public record Derived1(int Id, string Name) : BaseType(Id);
    public record Derived2(int Id, bool IsActive) : BaseType(Id);

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

        [Fact]
        public void CanSerializeInterface()
        {
            var expected = new List<BaseType>
            {
                new Derived1(123, "Foo"),
                new Derived2(456, true)
            };
            string json = JsonSerializer.Serialize(expected);
            var actual = JsonSerializer.Deserialize<List<BaseType>>(json);

            Assert.Equal(expected.Count, actual.Count);

            var expectedDerived1 = Assert.IsType<Derived1>(expected[0]);
            var actualDerived1 = Assert.IsType<Derived1>(actual[0]);

            Assert.Equal(expectedDerived1.Name, actualDerived1.Name);

            var expectedDerived2 = Assert.IsType<Derived2>(expected[1]);
            var actualDerived2 = Assert.IsType<Derived2>(actual[1]);

            Assert.Equal(expectedDerived2.IsActive, actualDerived2.IsActive);
        }
    }
}
