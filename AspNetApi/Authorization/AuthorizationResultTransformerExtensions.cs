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
                // Resolve all specified handlers dynamically
                var handlers = handlerTypes
                    .Select(handlerType => sp.GetRequiredService(handlerType) as IAuthorizationMiddlewareResultHandler)
                    .ToList();

                // Return the composite handler
                return new CompositeAuthorizationResultTransformer(handlers);
            });
        }
    }

}
