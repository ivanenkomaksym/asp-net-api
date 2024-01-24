using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace AspNetApi.IntegrationTests
{
    // ========================================================================
    // ==============Deserialization of interface not supported================

    public enum BaseType
    {
        Base1,
        Base2
    }

    public interface IBase
    { }

    public class Base1 : IBase
    {
        public string Value { get; set; }
    }

    public class Base2 : IBase
    {
        public bool Active { get; set; }
    }

    public class Implementation
    {
        public BaseType BaseType;
        public IBase Base { get; set; }
    }

    // ========================================================================
    // ==============Use JsonDerivedType to serialize interface================

    [JsonDerivedType(typeof(Derived1), typeDiscriminator: "Derived1")]
    [JsonDerivedType(typeof(Derived2), typeDiscriminator: "Derived2")]
    public interface IBase2
    { }

    public class Derived1 : IBase2
    {
        public string Name { get; set; }
    }

    public record Derived2 : IBase2
    {
        public bool IsActive { get; set; }
    }

    public class Container
    {
        public BaseType BaseType;
        public IBase2 Base { get; set; }
    }

    // ========================================================================
    // ========Use JsonConverter if JsonDerivedType cannot be used=============

    public class ImplementationJsonConverter : JsonConverter<Implementation>
    {
        public override Implementation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            var myBaseTypeString = root.GetProperty(MyBaseTypeString).GetString();
            Enum.TryParse(myBaseTypeString, true, out BaseType myBaseType);

            var myBaseRawString = root.TryGetProperty(Encoding.UTF8.GetBytes(MyBaseString), out var myBaseJsonElement)
                ? myBaseJsonElement.GetRawText()
                : null;

            IBase myBase = null;

            switch (myBaseType)
            {
                case BaseType.Base1:
                    myBase = JsonSerializer.Deserialize<Base1>(myBaseRawString, options);
                    break;
                case BaseType.Base2:
                    myBase = JsonSerializer.Deserialize<Base2>(myBaseRawString, options);
                    break;
            }

            return new Implementation { BaseType = myBaseType, Base = myBase };
        }

        /// <remarks>
        /// This method is required in order to correctly serialize derived types from <see cref="IBase"/>
        /// in case using <see cref="JsonDerivedType"/> is not an option (e.g. derived types are split over different packages).
        /// </remarks>
        public override void Write(Utf8JsonWriter writer, Implementation value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(MyBaseTypeString, value.BaseType.ToString());

            writer.WritePropertyName(MyBaseString);
            switch (value.BaseType)
            {
                case BaseType.Base1:
                    JsonSerializer.Serialize(writer, (Base1)value.Base, options);
                    break;
                case BaseType.Base2:
                    JsonSerializer.Serialize(writer, (Base2)value.Base, options);
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
        public void CanSerializeInterfaceInNewtonsoftButNotInSystemTextJson()
        {
            // Arrange
            List<Implementation> values = new()
            {
                new Implementation { BaseType = BaseType.Base1, Base = new Base1 {Value = "Base1Value" } },
                new Implementation { BaseType = BaseType.Base2, Base = new Base2 {Active = true } }
            };

            // Act
            string newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(values);
            string systemTextJson = JsonSerializer.Serialize(values);

            // Assert
            Assert.Contains("Base1Value", newtonsoftJson);
            Assert.DoesNotContain("Base1Value", systemTextJson);
        }

        [Fact]
        public void DeserializationOfInterfaceNotSupported()
        {
            // Arrange
            List<Implementation> values = new()
            {
                new Implementation { BaseType = BaseType.Base1, Base = new Base1 {Value = "Base1Value" } },
                new Implementation { BaseType = BaseType.Base2, Base = new Base2 {Active = true } }
            };

            // Act
            string json = JsonSerializer.Serialize(values);
            var exception = Assert.Throws<System.NotSupportedException>(() => JsonSerializer.Deserialize<List<Implementation>>(json));

            // Assert
            Assert.DoesNotContain("Base1Value", json);
            Assert.Contains("Deserialization of interface types is not supported.", exception.Message);
        }

        [Fact]
        public void CanSerializeInterface()
        {
            // Arrange
            var expected = new List<Container>
            {
                new() { BaseType = BaseType.Base1, Base = new Derived1 { Name = "Foo" } },
                new() { BaseType = BaseType.Base2, Base = new Derived2 { IsActive = true } },
            };

            // Act
            string json = JsonSerializer.Serialize(expected);
            var actual = JsonSerializer.Deserialize<List<Container>>(json);

            // Assert
            Assert.Equal(expected.Count, actual.Count);

            var expectedDerived1 = Assert.IsType<Derived1>(expected[0].Base);
            var actualDerived1 = Assert.IsType<Derived1>(actual[0].Base);

            Assert.Equal(expectedDerived1.Name, actualDerived1.Name);

            var expectedDerived2 = Assert.IsType<Derived2>(expected[1].Base);
            var actualDerived2 = Assert.IsType<Derived2>(actual[1].Base);

            Assert.Equal(expectedDerived2.IsActive, actualDerived2.IsActive);
        }

        [Fact]
        public void CanSerializeInterfaceWithCustomJsonConverter()
        {
            // Arramge
            List<Implementation> expected = new()
            {
                new Implementation { BaseType = BaseType.Base1, Base = new Base1 {Value = "Base1Value" } },
                new Implementation { BaseType = BaseType.Base2, Base = new Base2 {Active = true } }
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
            Assert.Equal<IntegrationTests.BaseType>((IntegrationTests.BaseType)expected[0].BaseType, (IntegrationTests.BaseType)actual[0].BaseType);
            Assert.Equal<IntegrationTests.BaseType>((IntegrationTests.BaseType)expected[1].BaseType, (IntegrationTests.BaseType)actual[1].BaseType);

            var expectedBase1 = Assert.IsType<Base1>(expected[0].Base);
            var actualBase1 = Assert.IsType<Base1>(actual[0].Base);

            Assert.Equal(expectedBase1.Value, actualBase1.Value);

            var expectedBase2 = Assert.IsType<Base2>(expected[1].Base);
            var actualBase2 = Assert.IsType<Base2>(actual[1].Base);

            Assert.Equal(expectedBase2.Active, actualBase2.Active);
        }
    }
}
