using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    // ========================================================================
    // ==============Deserialization of interface not supported================

    public enum MyBaseType
    {
        Base1,
        Base2
    }

    public interface IMyBase{}

    public record Base1(string Value) : IMyBase;

    public record Base2(string Value) : IMyBase;

    public record Implementation(IMyBase MyBase, MyBaseType MyBaseType);

    // ========================================================================
    // ==============Use JsonDerivedType to serialize interface================

    [JsonDerivedType(typeof(Derived1), typeDiscriminator: "Derived1")]
    [JsonDerivedType(typeof(Derived2), typeDiscriminator: "Derived2")]
    public record BaseType(int Id);

    public record Derived1(int Id, string Name) : BaseType(Id);
    public record Derived2(int Id, bool IsActive) : BaseType(Id);

    // ========================================================================
    // ========Use JsonConverter if JsonDerivedType cannot be used=============

    public class ImplementationJsonConverter : JsonConverter<Implementation>
    {
        public override Implementation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            var myBaseTypeString = root.GetProperty(MyBaseTypeString).GetString();
            Enum.TryParse(myBaseTypeString, true, out MyBaseType myBaseType);

            var myBaseRawString = root.TryGetProperty(Encoding.UTF8.GetBytes(MyBaseString), out var myBaseJsonElement)
                ? myBaseJsonElement.GetRawText()
                : null;

            IMyBase myBase = null;

            switch (myBaseType)
            {
                case MyBaseType.Base1:
                    myBase = JsonSerializer.Deserialize<Base1>(myBaseRawString, options);
                    break;
                case MyBaseType.Base2:
                    myBase = JsonSerializer.Deserialize<Base2>(myBaseRawString, options);
                    break;
            }

            return new Implementation(myBase, myBaseType);
        }

        /// <remarks>
        /// This method is required in order to correctly serialize derived types from <see cref="IMyBase"/>
        /// in case using <see cref="JsonDerivedType"/> is not an option (e.g. derived types are split over different packages).
        /// </remarks>
        public override void Write(Utf8JsonWriter writer, Implementation value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(MyBaseTypeString, value.MyBaseType.ToString());

            writer.WritePropertyName(MyBaseString);
            switch (value.MyBaseType)
            {
                case MyBaseType.Base1:
                    JsonSerializer.Serialize(writer, (Base1)value.MyBase, options);
                    break;
                case MyBaseType.Base2:
                    JsonSerializer.Serialize(writer, (Base2)value.MyBase, options);
                    break;
            }

            writer.WriteEndObject();
        }

        private readonly string MyBaseString = "myBase";
        private readonly string MyBaseTypeString = "myBaseType";
    }

    // ========================================================================

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
            // Arrange
            List<Implementation> values = new()
            {
                new Implementation(new Base1("Base1Value"), MyBaseType.Base1),
                new Implementation(new Base2("Base2Value"), MyBaseType.Base2)
            };

            // Act
            string json = JsonSerializer.Serialize(values);
            var exception = Assert.Throws<System.NotSupportedException>(() => JsonSerializer.Deserialize<List<Implementation>>(json));

            // Assert
            Assert.Contains("Deserialization of interface types is not supported.", exception.Message);
        }

        [Fact]
        public void CanSerializeInterface()
        {
            // Arrange
            var expected = new List<BaseType>
            {
                new Derived1(123, "Foo"),
                new Derived2(456, true)
            };

            // Act
            string json = JsonSerializer.Serialize(expected);
            var actual = JsonSerializer.Deserialize<List<BaseType>>(json);

            // Assert
            Assert.Equal(expected.Count, actual.Count);

            var expectedDerived1 = Assert.IsType<Derived1>(expected[0]);
            var actualDerived1 = Assert.IsType<Derived1>(actual[0]);

            Assert.Equal(expectedDerived1.Name, actualDerived1.Name);

            var expectedDerived2 = Assert.IsType<Derived2>(expected[1]);
            var actualDerived2 = Assert.IsType<Derived2>(actual[1]);

            Assert.Equal(expectedDerived2.IsActive, actualDerived2.IsActive);
        }

        [Fact]
        public void CanSerializeInterfaceWithCustomJsonConverter()
        {
            // Arramge
            List<Implementation> expected = new()
            {
                new Implementation(new Base1("Base1Value"), MyBaseType.Base1),
                new Implementation(new Base2("Base2Value"), MyBaseType.Base2)
            };

            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new ImplementationJsonConverter());

            // Act
            string json = JsonSerializer.Serialize(expected, options);
            var actual = JsonSerializer.Deserialize<List<Implementation>>(json, options);

            // Assert
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected[0].MyBaseType, actual[0].MyBaseType);
            Assert.Equal(expected[1].MyBaseType, actual[1].MyBaseType);

            var expectedBase1 = Assert.IsType<Base1>(expected[0].MyBase);
            var actualBase1 = Assert.IsType<Base1>(actual[0].MyBase);

            Assert.Equal(expectedBase1.Value, actualBase1.Value);

            var expectedBase2 = Assert.IsType<Base2>(expected[1].MyBase);
            var actualBase2 = Assert.IsType<Base2>(actual[1].MyBase);

            Assert.Equal(expectedBase2.Value, actualBase2.Value);
        }
    }
}
