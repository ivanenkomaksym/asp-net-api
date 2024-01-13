using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetApi.Converters
{
    /// <summary>
    /// This isn't a real converter. It only exists as a hack to expose IServiceProvider on the JsonSerializerOptions.
    /// </summary>
    public class ServiceProviderDummyConverter : JsonConverter<object>, IServiceProvider
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public ServiceProviderDummyConverter(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            HttpContextAccessor = httpContextAccessor;
            ServiceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            // Use the request services, if available, to be able to resolve scoped services.
            // If there isn't a current HttpContext, just use the root service provider.
            var services = HttpContextAccessor.HttpContext?.RequestServices ?? ServiceProvider;
            return services.GetService(serviceType);
        }

        public override bool CanConvert(Type typeToConvert) => false;

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
