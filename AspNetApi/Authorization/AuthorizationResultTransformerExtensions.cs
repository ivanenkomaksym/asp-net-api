using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public static class AuthorizationResultTransformerExtensions
    {
        // Generic registration method to resolve multiple handler types dynamically
        public static void AddCompositeAuthorizationResultTransformer(this IServiceCollection services, params Type[] handlerTypes)
        {
            services.AddSingleton<IAuthorizationMiddlewareResultHandler>(sp =>
            {
                var handlers = new List<IAuthorizationMiddlewareResultHandler>();

                foreach (var handlerType in handlerTypes)
                {
                    // Check if handlerType implements IAuthorizationMiddlewareResultHandler
                    if (!typeof(IAuthorizationMiddlewareResultHandler).IsAssignableFrom(handlerType))
                    {
                        throw new InvalidOperationException($"{handlerType.Name} does not implement IAuthorizationMiddlewareResultHandler.");
                    }

                    // Try to resolve the handler from the service provider
                    var handler = sp.GetService(handlerType) as IAuthorizationMiddlewareResultHandler ?? throw new InvalidOperationException($"Service for {handlerType.Name} could not be found.");
                    handlers.Add(handler);
                }

                // Return the composite transformer with all handlers
                return new CompositeAuthorizationResultTransformer(handlers);
            });
        }
    }

}
