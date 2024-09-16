using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public class CompositeAuthorizationResultTransformer : IAuthorizationMiddlewareResultHandler
    {
        private readonly IEnumerable<IAuthorizationMiddlewareResultHandler> _handlers;

        public CompositeAuthorizationResultTransformer(IEnumerable<IAuthorizationMiddlewareResultHandler> handlers)
        {
            _handlers = handlers;
        }

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            foreach (var handler in _handlers)
            {
                // Allow each handler to process the request
                await handler.HandleAsync(next, context, policy, authorizeResult);

                // After the first handler processes the result, stop further handlers from processing
                if (context.Response.HasStarted)
                    return;
            }

            // Continue the request pipeline if no response was started
            await next(context);
        }
    }

}
